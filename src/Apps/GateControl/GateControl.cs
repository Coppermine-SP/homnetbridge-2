using System.Collections.Specialized;
using System.Web;
using PacketDotNet;

namespace CloudInteractive.HomNetBridge.Apps.GateControl;

[NetDaemonApp]
public class GateControl : HomNetAppBase
{
    private IHaContext _context;
    public GateControl(IHaContext context, ILogger<GateControl> logger, IEthernetCapture netCapture) : base(logger,
        ethernetCapture: netCapture)
    {
        _context = context;
    }
    
    private static string GetUrlParameter(string key, string value)
    {
        NameValueCollection parameters = HttpUtility.ParseQueryString(value);
        return parameters[key] ?? String.Empty;
    }

    [Protocol(ProtocolType.Tcp)]
    [StartsWith("SLB&5&lbs&17")]
    [UrlParameter("info")]
    public void OnGateEntry(IEthernetCapture.EthernetReceiveEventArgs e)
    {
        string content = e.Content.PayloadToString(true) ?? String.Empty;
        string licenseNo = GetUrlParameter("info", content);
        
        Logger.LogInformation("GateEntry: " + licenseNo);
        Notification.SendNotification(_context, "차량 입차", $"{licenseNo} 차량이 입차하였습니다.", Notification.NotifyLevel.Active);
    }
}