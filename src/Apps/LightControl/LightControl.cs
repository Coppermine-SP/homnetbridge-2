using CloudInteractive.HomNetBridge.Apps;
using CloudInteractive.HomNetBridge.Services;
using CloudInteractive.HomNetBridge.Util;

namespace CloudInteractive.HomNetBridge.apps.LightControl
{
    [NetDaemonApp]
    public class LightControl : HomNetAppBase
    {
        public LightControl(IHaContext context, ILogger<LightControl> logger, ISerialClient serialClient) : base(logger, serialClient)
        {
            logger.LogInformation("Hello, World!");
        }

        [StartsWith("0219010800C23D030219010D40C2000003")]
        private void LightStatusUpdate(ISerialClient.SerialReceiveEventArgs e)
        {
            Logger.LogInformation("LightStatusUpdate");
        }
    }
}
