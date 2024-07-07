using PacketDotNet;

namespace CloudInteractive.HomNetBridge.Apps.LobbyAccess
{
    [NetDaemonApp]
    public class LobbyAccess : HomNetAppBase
    {
        private IHaContext _context;

        public LobbyAccess(IHaContext context, ILogger<LobbyAccess> logger, IEthernetCapture netCapture) : base(logger, ethernetCapture: netCapture) => _context = context;

        [Protocol(ProtocolType.Udp)]
        [Contains("CallType=TYPE_LOBBY, MsgType=reqOpenDoor")]
        public void LobbyOpenRequested(IEthernetCapture.EthernetReceiveEventArgs e)
        {
            Logger.LogInformation($"Lobby reqOpenDoor event detected in HBM REPORT!");
            Notification.SendNotification(_context,"공동현관문", $"공동현관문 출입 요청을 승인했습니다.", Notification.NotifyLevel.TimeSensitive, tag: "lobbyAccess");
        }

        [Protocol(ProtocolType.Udp)]
        [Contains("CallType=TYPE_LOBBY, MsgType=evtRing")]
        public void LobbyRingEvent(IEthernetCapture.EthernetReceiveEventArgs e)
        {
            Logger.LogInformation($"Lobby evtRing event detected in HBM REPORT!");
            Notification.SendNotification(_context, "공동현관문", $"공동현관문 출입 요청이 있습니다.", Notification.NotifyLevel.TimeSensitive, tag:"lobbyAccess");
        }

    }
}
