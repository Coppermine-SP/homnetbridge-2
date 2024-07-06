using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CloudInteractive.HomNetBridge.Services
{
    public class RemoteSerialClient : ISerialClient
    {
        public event EventHandler<ISerialClient.SerialReceiveEventArgs>? ReceivedEvent;

        private const int BufferSize = 1024;
        private const int ReconnectWaitTime = 10;

        private readonly ILogger _logger;
        private readonly string _host;
        private readonly int _port;
        private Socket? _socket;
        private byte[]? _buffer = null;

        public RemoteSerialClient(ILogger<RemoteSerialClient> logger, IConfiguration config)
        {
            _logger = logger;
            var section = config.GetSection("RemoteSerialClient");
            _host = section.GetValue<string>("Hostname") ?? "localhost";
            _port = section.GetValue<int>("Port");
            _logger.LogInformation($"Init.. ({_host}:{_port})");

            Connect();
        }

        ~RemoteSerialClient() => Disconnect();
        
        private void Connect()
        {
            _logger.LogInformation($"Connecting to {_host}:{_port} via TCP...");
            if ( _socket is not null) _socket.Close();

            try
            {
                _buffer = new byte[BufferSize];
                IPAddress ip = Dns.GetHostAddresses(_host)[0];
                IPEndPoint endpoint = new IPEndPoint(ip, _port);

                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
                _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 2);
                _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 2);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                _socket.Connect(endpoint);
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceivePacket),
                    _socket);
            }
            catch (Exception e)
            {
                _logger.LogError("Exception occurred in open socket: " + e);

                if (_socket is not null)
                {
                    _socket.Close();
                    _socket = null;
                }

                throw new Exception("Failed to open socket.", e);
            }

            _logger.LogInformation("Connected.");
        }

        private void Disconnect()
        {
            if (_socket is not null)
            {
                _socket.Close();
                _socket = null;
            }

            _buffer = null;

            _logger.LogInformation("Disconnected.");
            Task.Run(ReconnectAsync);
        }

        private async Task ReconnectAsync()
        {
            while (_socket is null)
            {
                try
                {
                    Connect();
                    return;
                }
                catch
                {
                    _logger.LogWarning($"Attempting to reconnect in {ReconnectWaitTime} seconds..");
                    Thread.Sleep(TimeSpan.FromSeconds(ReconnectWaitTime));
                }
            }
        }

        private void ReceivePacket(IAsyncResult result)
        {
            Socket asyncSocket = (Socket)result.AsyncState;
            try
            {
                int recv = asyncSocket.EndReceive(result);
                if (recv == 0)
                {
                    Disconnect();
                    return;
                }
                string receivedString = BitConverter.ToString(_buffer, 0, recv);
                _logger.LogDebug("Receive => " + receivedString);

                ReceivedEvent?.Invoke(this, new ISerialClient.SerialReceiveEventArgs(receivedString.Replace("-", string.Empty)));
                asyncSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceivePacket), asyncSocket);
            }
            catch (Exception e)
            {
                _logger.LogError("Exception occurred in receive packet: " + e);
                Disconnect();
            }
        }

    }
}
