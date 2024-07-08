using System.IO.Ports;
using Microsoft.Extensions.Configuration;

namespace CloudInteractive.HomNetBridge.Services
{
    public class LocalSerialClient : ISerialClient
    {
        private readonly ILogger<LocalSerialClient> _logger;
        private readonly SerialPort _port;
        public LocalSerialClient(ILogger<LocalSerialClient> logger, IConfiguration config)
        {
            _logger = logger;
            string interfaces = "Available serial devices:\n";
            foreach (string x in SerialPort.GetPortNames())
            {
                interfaces += $"{x}\n";
            }
            _logger.LogInformation(interfaces);

            var section = config.GetSection("LocalSerialClient");
            string? interfaceName = section.GetValue<string>("InterfaceName");

            if (interfaceName is null)
            {
                _logger.LogWarning("InterfaceName is not configured!");
                throw new NullReferenceException("InterfaceName was null.");
            }
            
            _logger.LogInformation($"Open device {interfaceName}...");
            _port = new SerialPort(interfaceName)
            {
                BaudRate = 9600,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
                DiscardNull = true
            };
            _port.DataReceived += PortOnDataReceived;
            _port.ErrorReceived += PortOnErrorReceived;
            _port.Open();
        }
        ~LocalSerialClient() => _port.Close();
        
        private void PortOnErrorReceived(object sender, SerialErrorReceivedEventArgs e) => _logger.LogWarning($"Error on Serial device {_port.PortName}: {e.EventType.ToString()}");
        private void PortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
        }

        public void SendAsync(string content)
        {
            
        }
        public event EventHandler<ISerialClient.SerialReceiveEventArgs>? ReceivedEvent;
    }
}
