namespace CloudInteractive.HomNetBridge.Services
{
    public class LocalSerialClient : ISerialClient
    {
        public void SendAsync(string content) => throw new NotImplementedException();

        public event EventHandler<ISerialClient.SerialReceiveEventArgs>? ReceivedEvent;

        //TODO: Serial communication via USB Adapter.
    }
}
