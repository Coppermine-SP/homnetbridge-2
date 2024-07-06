using PacketDotNet;
using System.Text.RegularExpressions;

namespace CloudInteractive.HomNetBridge.Apps.HBMLog
{
    [NetDaemonApp]
    public class HBMLog : HomNetAppBase
    {
        public HBMLog(ILogger<HBMLog> logger, IEthernetCapture netCapture) : base(logger, ethernetCapture: netCapture)
        {

        }

        [Protocol(ProtocolType.Udp)]
        [StartsWith("[HBM]")]
        public void OnReceiveHBMLog(IEthernetCapture.EthernetReceiveEventArgs e)
        {
            const string pattern = @"[\p{C}]|\t|[ ]{5,}";
            string message = Regex.Replace(e.Content.PayloadToString(true), pattern, " ").Trim();

            Logger.LogInformation(message);
        }
    }
}
