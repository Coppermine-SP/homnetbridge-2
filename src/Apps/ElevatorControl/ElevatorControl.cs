using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudInteractive.HomNetBridge.Apps.ElevatorControl
{
    [NetDaemonApp]
    public class ElevatorControl : HomNetAppBase
    {
        public ElevatorControl(ILogger<ElevatorControl> logger, ISerialClient serialClient, IEthernetCapture netCapture) : base(logger, serialClient, netCapture)
        {

        }
    }
}
