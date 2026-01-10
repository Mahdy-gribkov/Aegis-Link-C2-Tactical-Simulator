using System;
using System.Threading.Tasks;

namespace AegisLink.Core
{
    public interface IDataLink : IDisposable
    {
        /// <summary>
        /// Fired when a valid TelemetryFrame is received.
        /// Warning: This event is raised on a background thread.
        /// </summary>
        event Action<TelemetryFrame> OnFrameReceived;

        /// <summary>
        /// Sends a command asynchronously to the tactical unit.
        /// </summary>
        Task SendCommandAsync(byte[] command);
    }
}
