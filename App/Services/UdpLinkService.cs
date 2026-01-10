using AegisLink.Core;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AegisLink.App.Services
{
    public class UdpLinkService : IDataLink
    {
        private readonly UdpClient _udpClient;
        private readonly CancellationTokenSource _cts;
        private readonly Task _listenerTask;
        private bool _disposed;
        
        // Security State
        private bool _isHandshakeComplete = false;
        private byte _currentChallenge;
        private IPEndPoint? _lockedEndPoint;

        public event Action<TelemetryFrame> OnFrameReceived;

        public UdpLinkService()
        {
            _udpClient = new UdpClient(TacticalConstants.DEFAULT_PORT);
            _cts = new CancellationTokenSource();
            
            // Start listening on a background thread immediately
            _listenerTask = Task.Run(ListenerLoopAsync, _cts.Token);
        }

        private async Task ListenerLoopAsync()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    // ReceiveAsync returns the remote endpoint
                    var result = await _udpClient.ReceiveAsync();
                    byte[] payload = result.Buffer;
                    IPEndPoint remoteEp = result.RemoteEndPoint;

                    ProcessIncomingPacket(payload, remoteEp);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
            }
            catch (ObjectDisposedException)
            {
                // Socket was closed during receive
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UDP LISTENER ERROR] {ex.Message}");
            }
        }

        private void ProcessIncomingPacket(byte[] payload, IPEndPoint remoteEp)
        {
            // IF NOT AUTHENTICATED: Treat as Handshake Request (or ignore)
            if (!_isHandshakeComplete)
            {
                // Simple Logic: Any packet triggers a Challenge from the PC
                // In a real scenario, we might wait for a specific "HELLO" opcode.
                // Here, we assume the Cardputer beaconing sends *something*.
                
                // If this is the Response (Length 1)
                if (payload.Length == 1 && _lockedEndPoint != null && remoteEp.Equals(_lockedEndPoint))
                {
                    byte response = payload[0];
                    byte expected = (byte)(_currentChallenge ^ TacticalConstants.XOR_SHIELD_KEY);

                    if (response == expected)
                    {
                        Debug.WriteLine($"[HANDSHAKE] SUCCESS. Endpoint {remoteEp} Trusted.");
                        _isHandshakeComplete = true;
                        // Trigger a "Connected" event or just start accepting data?
                        // For this phase, we just flip the bool. 
                        // The NEXT packet will be parsed as Telemetry.
                    }
                    else
                    {
                       Debug.WriteLine($"[HANDSHAKE] FAILED. Expected {expected:X2}, Got {response:X2}");
                       // Reset or keep trying?
                    }
                }
                else
                {
                     // Send Challenge
                     SendChallenge(remoteEp);
                }
                return;
            }

            // IF AUTHENTICATED: Ensure it comes from the trusted source
            if (!_lockedEndPoint!.Equals(remoteEp))
            {
                Debug.WriteLine($"[SECURITY_ALERT] Packet from untrusted source {remoteEp}");
                return;
            }

            ValidateAndDispatch(payload);
        }

        private void SendChallenge(IPEndPoint target)
        {
             // Generate random byte
             _currentChallenge = (byte)new Random().Next(0, 255);
             _lockedEndPoint = target; // Lock onto this candidate
             
             // Send Challenge
             byte[] challengePacket = new byte[] { _currentChallenge };
             _udpClient.Send(challengePacket, 1, target);
             
             Debug.WriteLine($"[HANDSHAKE] Sent Challenge {_currentChallenge:X2} to {target}");
        }

        private void ValidateAndDispatch(byte[] payload)
        {
            int expectedSize = Marshal.SizeOf<TelemetryFrame>();

            if (payload.Length != expectedSize)
            {
                Debug.WriteLine($"[MALFORMED_PACKET] Expected {expectedSize} bytes, got {payload.Length}");
                return;
            }

            // Zero-copy conversion (well, using the buffer directly)
            TelemetryFrame frame = ProtocolMapper.FromBytes(payload);

            // Fire event on background thread (UI-Agnostic)
            OnFrameReceived?.Invoke(frame);
        }

        public async Task SendCommandAsync(byte[] command)
        {
             if (_disposed) return;
             // Implementation for sending would go here, usually strictly directed to a known endpoint
             // _udpClient.SendAsync(command, ...);
             await Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _cts.Cancel();
                
                // Closing the client breaks the pending ReceiveAsync
                _udpClient.Close(); 
                _udpClient.Dispose();

                try
                {
                    // Ensure the background task has acknowledged cancellation
                   _listenerTask.Wait(TimeSpan.FromMilliseconds(500));
                }
                catch (Exception)
                {
                    // Ignore task cancellation exceptions during disposal
                }

                _cts.Dispose();
            }

            _disposed = true;
        }
    }
}
