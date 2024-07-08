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

    public class NullSerialClient : ISerialClient
    {
        private readonly ILogger _logger;

        public NullSerialClient(ILogger<NullEthernetCapture> logger)
        {
            _logger = logger;
            _logger.LogWarning("Using NullSerialClient. Use test purpose only.");
        }

        public void SendAsync(string hex) => _logger.LogInformation($"Send => {hex}");
        public event EventHandler<ISerialClient.SerialReceiveEventArgs>? ReceivedEvent;
    }


}
