using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudInteractive.HomNetBridge.Services
{
    public interface ISerialClient
    {
        public event EventHandler<ISerialClient.SerialReceiveEventArgs> ReceivedEvent;


        public class SerialReceiveEventArgs : EventArgs
        {
            public string Content;
            public SerialReceiveEventArgs(string content)
            {
                Content = content;
            }
        }
    }


}
