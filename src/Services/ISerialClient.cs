namespace CloudInteractive.HomNetBridge.Services
{
    public interface ISerialClient
    {
        public void SendAsync(string hex);
        public event EventHandler<ISerialClient.SerialReceiveEventArgs> ReceivedEvent;

        public class SerialReceiveEventArgs : EventArgs
        {
            public string Content;
            public SerialReceiveEventArgs(string content) => Content = content;
        }
    }


}
