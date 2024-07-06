using System.Collections.Generic;

namespace CloudInteractive.HomNetBridge.Util
{
    public static class Notification
    {
        public enum NotifyLevel { Active, TimeSensitive, Critical }
        private static readonly string[] NotifyLevelString =
        {
            "active",
            "time-sensitive",
            "critical"
        };

        public static void SendNotification(IHaContext context, string title, string message, NotifyLevel level = NotifyLevel.Active, string? tag = null)
        {
            var dataFields = new Dictionary<string, object>();
            var pushFields = new Dictionary<string, string>();

            pushFields["interruption-level"] = NotifyLevelString[(int)level];
            dataFields["push"] = pushFields;
            if (tag is not null) dataFields["tag"] = tag;

            context.CallService("notify", "notify", data:new
            {
                title = title,
                message = message,
                data = dataFields
            });
        }

    }
}
