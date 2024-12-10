using System.Collections.Specialized;
using System.Linq;
using System.Web;
using CloudInteractive.HomNetBridge.Context;
using Microsoft.EntityFrameworkCore;
using PacketDotNet;

namespace CloudInteractive.HomNetBridge.Apps.GateControl;

[NetDaemonApp]
public class GateControl : HomNetAppBase
{
    private IHaContext _context;
    private ServerDbContext _dbContext;
    public GateControl(IHaContext context, ILogger<GateControl> logger, IEthernetCapture netCapture, ServerDbContext dbContext) : base(logger,
        ethernetCapture: netCapture)
    {
        _context = context;
        _dbContext = dbContext;
        
        Logger.LogInformation("Load previous car status..");
        foreach (var x in _dbContext.Cars)
        {
            if (x.EntryStatus) Logger.LogInformation($"{x.LicensePlate}(#{x.Id}) => Entry");
            else Logger.LogInformation($"{x.LicensePlate}(#{x.Id}) => Exit");
            _context.CallService("python_script", "set_state", null, new
            {
                entity_id = x.HaEntityName,
                state = x.EntryStatus ? "on" : "off"
            });
        }
    }
    
    private static string GetUrlParameter(string key, string value)
    {
        NameValueCollection parameters = HttpUtility.ParseQueryString(value);
        return parameters[key] ?? String.Empty;
    }

    [Protocol(ProtocolType.Tcp)]
    [StartsWith("SLB&5&lbs&17")]
    [UrlParameter("info")]
    public void OnVehicleDetected(IEthernetCapture.EthernetReceiveEventArgs e)
    {
        string content = e.Content.PayloadToString(true) ?? String.Empty;
        string licenseNo = GetUrlParameter("info", content);
        
        Logger.LogInformation("GateEntry: " + licenseNo);
        var model = _dbContext.Cars.FirstOrDefault(x => x.LicensePlate != null && x.LicensePlate.Equals(licenseNo));

        if (model is null)
        {
            Notification.SendNotification(_context, "차량 인식", $"{licenseNo} 차량이 인식되었습니다.", Notification.NotifyLevel.Active);
            Logger.LogWarning($"{licenseNo} was not found in database!");
            return;
        }

        if (model.EntryStatus)
        {
            Notification.SendNotification(_context, "차량 출차", $"{licenseNo} 차량이 출차하였습니다.", Notification.NotifyLevel.Active);
            Logger.LogInformation($"{licenseNo}(#{model.Id}) => Exit");
        }
        else
        {
            Notification.SendNotification(_context, "차량 입차", $"{licenseNo} 차량이 입차하였습니다.", Notification.NotifyLevel.Active);
            Logger.LogInformation($"{licenseNo}(#{model.Id}) => Entry");
        }
        
        model.EntryStatus = !model.EntryStatus;
        _dbContext.SaveChanges();
        
        _context.CallService("python_script", "set_state", null, new
        {
            entity_id = model.HaEntityName,
            state = model.EntryStatus ? "on" : "off"
        });
    }
}