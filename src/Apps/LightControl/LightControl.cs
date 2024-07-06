using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Integration;

namespace CloudInteractive.HomNetBridge.apps.LightControl
{
    [NetDaemonApp]
    public class LightControl : HomNetAppBase
    {
        private static readonly Dictionary<char, bool[]> receiveCodeTable = new Dictionary<char, bool[]>()
        {
            {'0', new[] { false, false, false}},
            {'2', new[] { false, false, true }},
            {'4', new[] { false, true, false }},
            {'6', new[] { false, true, true }},
            {'8', new[] { true, false, false }},
            {'A', new[] { true, false, true }},
            {'C', new[] { true, true, false }},
            {'E', new[] { true, true, true }}
        };

        private static readonly Dictionary<char, string[]> sendCodeTable = new Dictionary<char, string[]>()
        {
            {'0', new[] { "4C", "000C" }},
            {'2', new[] { "2C", "20EC" }},
            {'4', new[] { "0C", "40CC" }},
            {'6', new[] { "EC", "60AC" }},
            {'8', new[] { "CC", "808C" }},
            {'A', new[] { "AC", "A06C" }},
            {'C', new[] { "8C", "C04C" }},
            {'E', new[] { "6C", "E02C" }}
        };

        private static Dictionary<int, Entity> _lightStatusBooleanHelpers;
        private bool[] _lightStatus = { false, false, false };
        private bool _isUpdated = false;
        private bool _isDisposing = false;
        private readonly Task _requestTask;
        private readonly ConcurrentQueue<LightCallbackData> _requestQueue = new ConcurrentQueue<LightCallbackData>();
        public record LightCallbackData(int idx, bool state);
        public record LightCallbackParameter(int idx, JsonElement state);

        public LightControl(IHaContext context, ILogger<LightControl> logger, ISerialClient serialClient) : base(logger, serialClient)
        {
            
            _lightStatusBooleanHelpers = new Dictionary<int, Entity>()
            {
                { 0, context.Entity("input_boolean.homnet_light_0_state") },
                { 1, context.Entity("input_boolean.homnet_light_1_state") },
                { 2, context.Entity("input_boolean.homnet_light_2_state") }
            };
            context.RegisterServiceCallBack<LightCallbackParameter>("callback_light", LightCallbackEvent);
            _requestTask = Task.Run(RequestTask);
        }

        ~LightControl()
        {
            _isDisposing = true;
            _requestTask.Wait();
        }

        private void LightCallbackEvent(LightCallbackParameter e)
        {
            //ISSUE: Exception in subscription -> System.InvalidOperationException: Cannot get the value of a token type 'String' as a boolean.
            bool x;
            try
            {
                x = e.state.GetBoolean();
            }
            catch
            {
                Logger.LogWarning("Invalid LightCallbackData paramater.");
                return;
            }

            Logger.LogInformation($"LightCallback from HA: {e.idx} => {x}");
            _requestQueue.Enqueue(new LightCallbackData(e.idx, x));
        }

        private void RequestTask()
        {
            while (true)
            {
                LightCallbackData x;
                if (_requestQueue.TryDequeue(out x))
                {
                    while (!_isUpdated) Thread.Sleep(100);

                    lock (this)
                    {
                        _isUpdated = false;
                        _lightStatus[x.idx] = x.state;
                        char key = receiveCodeTable.FirstOrDefault(y =>
                            y.Value[0] == _lightStatus[0] && y.Value[1] == _lightStatus[1] &&
                            y.Value[2] == _lightStatus[2]).Key;
                        var codes = sendCodeTable[key];

                        Logger.LogInformation(
                            $"Light status change request => {x.idx}:{x.state} states:({_lightStatus[0]}, {_lightStatus[1]}, {_lightStatus[2]})");
                        SerialClient?.SendAsync($"0219010A00B003{key}0{codes[0]}030219010B40B00003{codes[1]}03");
                    }
                }
                Thread.Sleep(100);
                if (_isDisposing) return;
            }

        }

        [StartsWith("0219010800C23D030219010D40C2000003")]
        private void LightStatusUpdate(ISerialClient.SerialReceiveEventArgs e)
        {
            lock (this)
            {
                if (!receiveCodeTable.TryGetValue(e.Content[34], out var value))
                {
                    Logger.LogWarning("Invalid light status code :" + e.Content[34]);
                    return;
                }

                if (!_isUpdated || !Helper.CompareBoolArray(value, _lightStatus))
                {
                    Logger.LogInformation(
                        $"Light status update => {e.Content[34]} ({value[0]}, {value[1]}, {value[2]})");

                    _lightStatus = (bool[])value.Clone();

                    for (int i = 0; i < 3; i++)
                    {
                        if (_lightStatus[i]) _lightStatusBooleanHelpers[i].CallService("turn_on");
                        else _lightStatusBooleanHelpers[i].CallService("turn_off");
                    }

                    _isUpdated = true;
                }
            }
        }
    }
}
