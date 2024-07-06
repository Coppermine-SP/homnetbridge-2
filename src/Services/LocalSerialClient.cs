using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudInteractive.HomNetBridge.Services
{
    public class LocalSerialClient : ISerialClient
    {
        public event EventHandler<ISerialClient.SerialReceiveEventArgs>? ReceivedEvent;

        //TODO: Serial communication via USB Adapter.
    }
}
