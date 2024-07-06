using PacketDotNet;

namespace CloudInteractive.HomNetBridge.Apps.DoorState
{
    [NetDaemonApp]
    public class DoorState : HomNetAppBase
    {
        private readonly IHaContext _context;

        public DoorState(IHaContext context, ILogger<DoorState> logger, IEthernetCapture netCapture) : base(logger, ethernetCapture: netCapture) => _context = context;
        
        private void SetDoorEntityState(bool state)
        {
            _context.CallService("python_script", "set_state", null, new
            {
                entity_id = "binary_sensor.homnet_front_door",
                state = state ? "on" : "off"
            });
        }

        [Protocol(ProtocolType.Udp)]
        [Contains("F3 09 82 02 01 01 89 F4")]
        public void DoorClosed(IEthernetCapture.EthernetReceiveEventArgs e)
        {
            Logger.LogInformation("Security sensor close event detected in HBM REPORT!");
            SetDoorEntityState(true);
        }

        [Protocol(ProtocolType.Udp)]
        [Contains("F3 09 82 02 00 01 88 F4")]
        public void DoorOpened(IEthernetCapture.EthernetReceiveEventArgs e)
        {
            Logger.LogInformation("Security sensor open event detected in HBM REPORT!");
            SetDoorEntityState(false);
        }
    }
}
