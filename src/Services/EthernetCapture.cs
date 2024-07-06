using System.Net.Mime;
using SharpPcap;
using System.Text;
using Microsoft.Extensions.Configuration;
using PacketDotNet;

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

    public class EthernetCapture : IEthernetCapture
    {
        public event EventHandler<IEthernetCapture.EthernetReceiveEventArgs>? ReceivedEvent;

        private readonly string _captureInterface;
        private readonly string _captureFilter;
        private readonly int _readTimeout;
        private readonly bool _verbose;

        private ILogger _logger;
        private static ICaptureDevice _captureDevice;

        public EthernetCapture(ILogger<RemoteSerialClient> logger, IConfiguration config)
        {
            _logger = logger;

            var section = config.GetSection("EthernetCapture");
            _captureInterface = section.GetValue<string>("CaptureInterface");
            _captureFilter = section.GetValue<string>("CaptureFilter");
            _readTimeout = section.GetValue<int>("ReadTimeout");
            _verbose = section.GetValue<bool>("PacketVerbose");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string interfaces = CaptureDeviceList.Instance.Count.ToString();
            var instances = CaptureDeviceList.Instance;

            try
            {
                _logger.LogInformation("LibPcap : " + Pcap.LibpcapVersion + " / Pcap: " + Pcap.Version);
                _logger.LogInformation($"Found {interfaces} interfaces.");
                foreach (var dev in instances)
                {
                    _logger.LogInformation($"interface: {dev.Name} ({dev.MacAddress})");
                    if (dev.Name == _captureInterface)
                    {
                        _logger.LogInformation($"Open interface {dev.Name} to capture...");
                        _captureDevice = dev;
                        _captureDevice.OnPacketArrival += new PacketArrivalEventHandler(OnPacketArrival);
                        _captureDevice.Open(DeviceModes.Promiscuous, _readTimeout);
                        _captureDevice.Filter = _captureFilter;
                        _captureDevice.StartCapture();
                        _logger.LogInformation($"interface {dev.Name} capture start.");
                        return;
                    }
                }
                _logger.LogWarning($"Can't find interface {_captureInterface}!");
                throw new ArgumentException($"interface {_captureInterface} was not found.");
            }
            catch (Exception e)
            {
                _logger.LogError("Cannot open capture device: " + e.ToString());
                throw new Exception();
            }
        }

        private void OnPacketArrival(object sender, PacketCapture e)
        {
            try
            {
                if (e.GetPacket().LinkLayerType != LinkLayers.Ethernet) return;
                var packet = Packet.ParsePacket(LinkLayers.Ethernet, e.GetPacket().Data).Extract<IPPacket>();

                if (packet is null) return;
                if (_verbose)
                {
                    var protocol = packet.Protocol;
                    var src = packet.SourceAddress;
                    var dst = packet.DestinationAddress;
                    var length = packet.TotalPacketLength;
                    var content = packet.PayloadToString() ?? "(not-readable)";

                    _logger.LogDebug($" {src} => {dst} ({protocol}, len={length}) :\n{content}");
                }

                ReceivedEvent?.Invoke(this, new IEthernetCapture.EthernetReceiveEventArgs(packet));
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception occurred in read packet: " + ex);
            }
        }
    }
}
