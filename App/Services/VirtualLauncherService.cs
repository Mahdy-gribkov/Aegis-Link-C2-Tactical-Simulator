using AegisLink.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AegisLink.App.Services
{
    public class VirtualLauncherService : IDataLink
    {
        private readonly CancellationTokenSource _cts;
        private readonly Task _simTask;
        private bool _disposed;
        
        // Simulation State
        private double _simAzimuth = 0;
        private double _simElevation = 0;
        private int _simBattery = 100;
        private float _simSignal = -40;

        public event Action<TelemetryFrame> OnFrameReceived;

        public VirtualLauncherService()
        {
            _cts = new CancellationTokenSource();
            _simTask = Task.Run(SimulationLoopAsync, _cts.Token);
        }

        private async Task SimulationLoopAsync()
        {
            var random = new Random();

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    // Oscillate Azimuth/Elevation (Sine wave pattern)
                    double time = DateTime.Now.Ticks / 10000000.0; // Seconds
                    _simAzimuth = 180 + (45 * Math.Sin(time));
                    _simElevation = 30 + (15 * Math.Cos(time));

                    // Drain Battery slowly
                    if (random.Next(0, 100) > 98 && _simBattery > 0)
                    {
                        _simBattery--;
                    }

                    // Fluctuate Signal
                    _simSignal = -40 + random.Next(-5, 5);

                    var frame = new TelemetryFrame(
                        _simBattery,
                        _simSignal,
                        _simAzimuth,
                        _simElevation,
                        0x00 // OK
                    );

                    OnFrameReceived?.Invoke(frame);

                    // 60Hz update rate roughly
                    await Task.Delay(16, _cts.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        public Task SendCommandAsync(byte[] command)
        {
            // Simulate processing delay
            return Task.Delay(50);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _cts.Cancel();
                try
                {
                    _simTask.Wait(500);
                }
                catch { }
                _cts.Dispose();
            }
            _disposed = true;
        }
    }
}
