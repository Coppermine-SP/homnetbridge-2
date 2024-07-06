using System.Text;
using PacketDotNet;

namespace CloudInteractive.HomNetBridge.Util
{
    public static class IPPacketExtension
    {
        public static string? PayloadToString(this IPPacket packet, bool useEucKr=false)
        {
            Packet extractedPacket;
            if (packet.Protocol == ProtocolType.Tcp) 
                extractedPacket = packet.Extract<TcpPacket>();
            else if (packet.Protocol == ProtocolType.Udp)
                extractedPacket = packet.Extract<UdpPacket>();
            else return null;

            var payload = extractedPacket.PayloadData;
            if (payload is not null)
            {
                if (useEucKr)
                {
                    return Encoding.GetEncoding(51949).GetString(payload).Trim();
                }
                return Encoding.UTF8.GetString(payload).Trim();
            }
            return null;
        }
    }
}
