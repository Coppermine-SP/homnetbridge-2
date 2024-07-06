using Microsoft.Extensions.Configuration;
using PacketDotNet;

namespace CloudInteractive.HomNetBridge.Apps.ElevatorControl
{
    [NetDaemonApp]
    public class ElevatorControl : HomNetAppBase
    {
        public enum ElevatorDirection { Stop = 0, Down = 1, Up = 2 }
        public static readonly string[] DirectionString =
        {
            "층에 있습니다",
            "층에서 내려가고 있습니다",
            "층에서 올라가고 있습니다"
        };
        private static int FloorStringToInt(string floor) => (Int32.TryParse(floor, out int result)) ? result : -Int32.Parse(floor.Replace("B", ""));
        private static string FloorIntToString(int floor) => (floor > 0) ? floor.ToString() : $"B{-floor}";

        private const string NotifyTitle = "엘리베이터 호출";
        private const string NotifyTag = "elevator-service";

        private readonly int ReferenceFloor;
        private readonly int NotifyThreshold;

        private IHaContext _context;
        private bool _isCalled = false;
        private bool _isFirstUpdate = true;
        private bool _isHeadingDest = false;
        private bool _isNearCalled = false;

        private int _currentFloor = 0;
        private ElevatorDirection _currentDirection = 0;
        private int _lastNotifyFloor = 0;

        private string GetCurrentFloorString => FloorIntToString(_currentFloor);
        private int GetDistance => Math.Abs(_currentFloor - ReferenceFloor);

        public ElevatorControl(IConfiguration config, IHaContext context, ILogger<ElevatorControl> logger, ISerialClient serialClient, IEthernetCapture netCapture) : base(logger, serialClient, netCapture)
        {
            _context = context;

            var section = config.GetSection("ElevatorControl");
            ReferenceFloor = section.GetValue<int>("ReferenceFloor");
            NotifyThreshold = section.GetValue<int>("NotifyThreshold");
            Logger.LogInformation($"ReferenceFloor={ReferenceFloor}, NotifyThreshold={NotifyThreshold}");

            var entity = context.Entity("input_button.homnet_evcall");
            entity.StateAllChanges().Subscribe(e => ElevatorCallRequest());
        }

        private void ElevatorCallRequest()
        {
            Logger.LogInformation("Calling elevator");
            SerialClient.SendAsync("021C410A00D401012903021C410A40D40101E903");
            SerialClient.SendAsync("021C410800D22D03021C410A40D20103E903");
        }

        [Protocol(ProtocolType.Tcp)]
        [StartsWith("SLB&8&lbs&20&cmd=2")]
        [UrlParameter("floor")]
        public void ElevatorFloorUpdate(IEthernetCapture.EthernetReceiveEventArgs e)
        {
            string content = e.Content.PayloadToString();
            int floor = FloorStringToInt(Helper.GetUrlParameter("floor", content));
            ElevatorDirection direction = (ElevatorDirection)Convert.ToInt32(Helper.GetUrlParameter("direction", content));

            if (!_isCalled || floor == 0) return;

            lock (this)
            {
                _currentDirection = direction;
                if (floor != _currentFloor)
                {
                    Logger.LogInformation($"Floor update: ({floor}, direction={_currentDirection}).");
                    _currentFloor = floor;

                    //엘리베이터가 목적지로 향하는 방향인지 체크
                    if ((ReferenceFloor >= _currentFloor) && direction == ElevatorDirection.Up) _isHeadingDest = true;
                    else if ((ReferenceFloor < _currentFloor) && direction == ElevatorDirection.Down) _isHeadingDest = true;
                    else _isHeadingDest = false;

                    //최초 업데이트
                    if (_isFirstUpdate)
                    {
                        Logger.LogInformation("FirstUpdate push.");
                        Notification.SendNotification(_context, NotifyTitle, $"엘리베이터를 호출하였습니다. 현재 {GetCurrentFloorString}{DirectionString[(int)_currentDirection]}.", Notification.NotifyLevel.TimeSensitive, NotifyTag);
                        _lastNotifyFloor = floor;
                        _isFirstUpdate = false;
                    }
                    else
                    {
                        if (_isHeadingDest && GetDistance <= 3)
                        {
                            if (_isNearCalled) return; 
                            Logger.LogInformation($"Elevator expected arrival. arrival expected notify push.");

                            if (_currentDirection == ElevatorDirection.Down)
                                Notification.SendNotification(_context, NotifyTitle, $"엘리베이터가 곧 도착합니다.", Notification.NotifyLevel.TimeSensitive, NotifyTag);
                            else
                                Notification.SendNotification(_context, NotifyTitle, $"엘리베이터가 도착지 근처에 있습니다.", Notification.NotifyLevel.TimeSensitive, NotifyTag);

                            _isNearCalled = true;
                            return;
                        }
                        else _isNearCalled = false;
                        
                        if (Math.Abs(_currentFloor - _lastNotifyFloor) >= NotifyThreshold)
                        {
                            Logger.LogInformation($"notifyThreshold reached. floor notify push.(lastNotifyFloor={_lastNotifyFloor})");
                            Notification.SendNotification(_context, NotifyTitle, $"현재 {GetCurrentFloorString}{DirectionString[(int)_currentDirection]}.", Notification.NotifyLevel.TimeSensitive, NotifyTag);
                            _lastNotifyFloor = floor;
                        }
                    }
                }
            }

        }

        [Protocol(ProtocolType.Tcp)]
        [StartsWith("SLB&5&lbs&20")]
        [UrlParameter("arrival")]
        public void ElevatorArrival(IEthernetCapture.EthernetReceiveEventArgs e)
        {
            if (!_isCalled) return;

            lock (this)
            {
                _isCalled = false;
                _isFirstUpdate = true;
                _isHeadingDest = false;
                _isNearCalled = false;
                Logger.LogInformation("Elevator arrived. reset all states.");
            }

            Notification.SendNotification(_context, NotifyTitle, "엘리베이터가 도착했습니다.", Notification.NotifyLevel.TimeSensitive, NotifyTag);
        }

        [Protocol(ProtocolType.Tcp)]
        [StartsWith("SLB&5&lbs&120")]
        [UrlParameter("res")]
        public void ElevatorCall(IEthernetCapture.EthernetReceiveEventArgs e)
        {
            if (_isCalled)
            {
                Logger.LogWarning("Previous request have not been properly cleaned up. force cleanup.");
                lock (this)
                {
                    _isCalled = false;
                    _isFirstUpdate = true;
                    _isHeadingDest = false;
                    _isNearCalled = false;
                }
            }

            lock (this)
            {
                Logger.LogInformation("Elevator Called.");
                _isCalled = true;
            }
        }
    }
}
