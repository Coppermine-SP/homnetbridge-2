using System.IO.Ports;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;

namespace CloudInteractive.HomNetBridge.Services
{
    public class LocalSerialClient : ISerialClient
    {
        private const int BufferSize = 512;
        
        private readonly ILogger<LocalSerialClient> _logger;
        private readonly SerialPort _port;
        private readonly byte[] _buf = new byte[BufferSize];
        private int _idx = 0;
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
            int size = _port.BytesToRead;
            for (int i = 0; i < size; i++)
            {
                byte b = (byte)_port.ReadByte();
                if (b == 0x02)
                {
                    _idx = 0;
                    _buf[_idx++] = b;
                }   
                else if (b == 0x03 && _idx != 0)
                {
                    _buf[_idx++] = b;
                    byte[] tmp = new byte[_idx];
                    Array.Copy(_buf, tmp, _idx);

                    string str = BitConverter.ToString(tmp).Replace("-", String.Empty);
                    _logger.LogDebug("Receive => " + str);
                    ReceivedEvent?.Invoke(this, new ISerialClient.SerialReceiveEventArgs(str));
                }
                else
                {
                    if (_idx >= BufferSize)
                    {
                        _logger.LogWarning("Packet is too large, discard.");
                        _idx = 0;
                    }
                    else if(_idx != 0) _buf[_idx++] = b;
                }
                
            }
        }

        public void SendAsync(string content)
        {
            byte[] buf = Util.Helper.HexToByte(content);
            _port.Write(buf, 0, buf.Length);
        }
        public event EventHandler<ISerialClient.SerialReceiveEventArgs>? ReceivedEvent;
    }
}
