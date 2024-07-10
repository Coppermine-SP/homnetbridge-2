using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.Extensions.Logging;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.Extensions.Tts;
using NetDaemon.Runtime;

namespace CloudInteractive.HomNetBridge
{
    public static class Startup
    {
        enum SerialClientType
        {
            LocalSerialClient,
            RemoteSerialClient,
            NullSerialClient
        }

        enum EthernetCaptureType
        {
            LocalEthernetCapture,
            NullEthernetCapture
        }

        enum ConsoleOutType
        {
            Info,
            Warning
        }

        static void ConsoleOut(ConsoleOutType type, string content)
        {
            if (type is ConsoleOutType.Info)
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.Write("[i] ");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("[!] ");
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.WriteLine(content);

        }

        /// <summary>
        /// Home Assistant Integration of LG HomNet.
        /// </summary>
        /// <param name="serial"></param>
        /// <param name="ethernet"></param>
        static async Task Main(SerialClientType? serial, EthernetCaptureType? ethernet)
        {
            Type serialClient;
            Type ethernetCapture;

            //Docker Container Environment Variable
            string? envSerial = Environment.GetEnvironmentVariable("SERIAL_CLIENT");
            string? envEthernet = Environment.GetEnvironmentVariable("ETHERNET_CAPTURE");

            if (envSerial is not null)
            {
                envSerial = envSerial.ToUpper();
                if (envSerial == "LOCAL") serial = SerialClientType.LocalSerialClient;
                else if (envSerial == "REMOTE") serial = SerialClientType.RemoteSerialClient;
                else if (envSerial == "NULL") serial = SerialClientType.NullSerialClient;

            }

            if (envEthernet is not null)
            {
                envEthernet = envEthernet.ToUpper();
                if (envEthernet == "LOCAL") ethernet = EthernetCaptureType.LocalEthernetCapture;
                else if (envEthernet == "NULL") ethernet = EthernetCaptureType.NullEthernetCapture;
            }

            if (serial is null)
            {
                ConsoleOut(ConsoleOutType.Warning,"Parameter serial was not specified. Using LocalSerialClient as the default option.");
                serialClient = typeof(LocalSerialClient);
            }
            else
            {
                ConsoleOut(ConsoleOutType.Info,$"SerialClient: {serial}");
                if (serial is SerialClientType.LocalSerialClient) serialClient = typeof(LocalSerialClient);
                else if(serial is SerialClientType.RemoteSerialClient) serialClient = typeof(RemoteSerialClient);
                else serialClient = typeof(NullSerialClient);
            }

            if (ethernet is null)
            {
                ConsoleOut(ConsoleOutType.Warning,"Parameter ethernet was not specified. Using LocalEthernetCapture as the default option.");
                ethernetCapture = typeof(EthernetCapture);
            }
            else
            {
                ConsoleOut(ConsoleOutType.Info,$"EthernetCapture: {ethernet}");
                if (ethernet is EthernetCaptureType.LocalEthernetCapture) ethernetCapture = typeof(EthernetCapture);
                else ethernetCapture = typeof(NullEthernetCapture);
            }

            ConsoleOut(ConsoleOutType.Info, $"NetDaemon Startup..\n");

            try
            {
                await Host.CreateDefaultBuilder()
                    .UseNetDaemonAppSettings()
                    .UseNetDaemonRuntime()
                    .UseNetDaemonTextToSpeech()
                    .UseNetDaemonDefaultLogging()
                    .ConfigureServices((_, services) =>
                            services
                                .AddSingleton(typeof(ISerialClient), serialClient)
                                .AddSingleton(typeof(IEthernetCapture), ethernetCapture)
                                .AddAppsFromAssembly(Assembly.GetExecutingAssembly())
                                .AddNetDaemonStateManager()
                                .AddNetDaemonScheduler()
                        // Add next line if using code generator
                        // .AddHomeAssistantGenerated()
                    )
                    .Build()
                    .RunAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to start host... {e}");
                throw;
            }
        }
    }
}