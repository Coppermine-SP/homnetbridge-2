using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using CloudInteractive.HomNetBridge.Services;
using CloudInteractive.HomNetBridge.Util;

namespace CloudInteractive.HomNetBridge.Apps
{
    public class HomNetAppBase
    {
        internal ILogger Logger;
        internal ISerialClient? SerialClient;
        internal IEthernetCapture? EthernetCapture;
        internal readonly MethodInfo[] RuleMethods;

        public HomNetAppBase(ILogger logger, ISerialClient? serialClient = null, IEthernetCapture? ethernetCapture = null)
        {
            Logger = logger;
            SerialClient = serialClient;
            EthernetCapture = ethernetCapture;
            RuleMethods = this.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.GetCustomAttributes(typeof(Rule), true).Length > 0)
                .ToArray();


            Logger.LogInformation($"App Initialization. (useSerialClient={SerialClient is not null}, useEthernetCapture={EthernetCapture is not null}, rules={RuleMethods.Length})");

            if (SerialClient is not null) SerialClient.ReceivedEvent += OnSerialReceived;
        }

        public void OnSerialReceived(object? sender, ISerialClient.SerialReceiveEventArgs? e)
        {
            foreach (var rule in RuleMethods)
            {
                var attributes = rule.GetCustomAttributes();
                bool isCheckFailed = false;

                foreach (var x in attributes)
                {
                    if (x is EthernetRuleAttribute)
                    {
                        isCheckFailed = true;
                        break;
                    }

                    if (!((Rule)x).Check(e.Content))
                    {
                        isCheckFailed = true;
                        break;
                    }
                }

                if(isCheckFailed) continue;
                rule.Invoke(this, new object[] { e });
            }
        }
    }
}
