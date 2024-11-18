using PacketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudInteractive.HomNetBridge.Services
{
    public interface IEthernetCapture
    {
        public event EventHandler<EthernetReceiveEventArgs> ReceivedEvent;

        public class EthernetReceiveEventArgs : EventArgs
        {
            public IPPacket Content { get; set; }
            public EthernetReceiveEventArgs(IPPacket packet) => Content = packet;
        }
    }

    public class NullEthernetCapture : IEthernetCapture
    {
        public NullEthernetCapture(ILogger<NullEthernetCapture> logger) =>
            logger.LogWarning("Using NullEthernetCapture. Use test purpose only.");
        public event EventHandler<IEthernetCapture.EthernetReceiveEventArgs>? ReceivedEvent;
    }
}
