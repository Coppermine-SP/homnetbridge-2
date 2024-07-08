using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CloudInteractive.HomNetBridge.Services
{
    public class RemoteSerialClient : ISerialClient
    {
        public event EventHandler<ISerialClient.SerialReceiveEventArgs>? ReceivedEvent;

        private const int BufferSize = 512;
        private const int ReconnectWaitTime = 10;

        private readonly ILogger _logger;
        private readonly string _host;
        private readonly int _port;
        private Socket? _socket;
        private readonly byte[] _recvBuffer;
        private readonly byte[] _outBuffer;
        private int _idx = 0;

        public RemoteSerialClient(ILogger<RemoteSerialClient> logger, IConfiguration config)
        {
            _logger = logger;
            var section = config.GetSection("RemoteSerialClient");
            _host = section.GetValue<string>("Hostname") ?? "localhost";
            _port = section.GetValue<int>("Port");
            _logger.LogInformation($"Init.. ({_host}:{_port})");

            _recvBuffer = new byte[BufferSize];
            _outBuffer = new byte[BufferSize];
            Connect();
        }

        ~RemoteSerialClient() => Disconnect();
        
        private void Connect()
        {
            _logger.LogInformation($"Connecting to {_host}:{_port} via TCP...");
            if ( _socket is not null) _socket.Close();

            try
            {
                IPAddress ip = Dns.GetHostAddresses(_host)[0];
                IPEndPoint endpoint = new IPEndPoint(ip, _port);

                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
                _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 2);
                _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 2);
                _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                _socket.Connect(endpoint);
                _socket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, new AsyncCallback(ReceivePacket),
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

                for (int i = 0; i < recv; i++)
                {
                    byte b = _recvBuffer[i];

                    if (b == 0x02)
                    {
                        _idx = 0;
                        _outBuffer[_idx++] = b;
                    }
                    else if (b == 0x03 && _idx != 0)
                    {
                        _outBuffer[_idx++] = b;
                        byte[] tmp = new byte[_idx];
                        Array.Copy(_outBuffer, tmp, _idx);

                        string str = BitConverter.ToString(tmp).Replace("-", String.Empty);
                        _logger.LogDebug("Receive => " + str);
                        ReceivedEvent?.Invoke(this, new ISerialClient.SerialReceiveEventArgs(str));
                    }
                    else
                    {
                        if (_idx >= BufferSize)
                        {
                            _logger.LogWarning("Packet is too large, discard.");
                            _idx = 0;
                        }
                        else if(_idx != 0) _outBuffer[_idx++] = b;
                    }

                }
                asyncSocket.BeginReceive(_recvBuffer, 0, _recvBuffer.Length, SocketFlags.None, new AsyncCallback(ReceivePacket), asyncSocket);
            }
            catch (Exception e)
            {
                _logger.LogError("Exception occurred in receive packet: " + e);
                Disconnect();
            }
        }

        public void SendAsync(string hex)
        {
            if (_socket is null) return;
            try
            {
                byte[] message = Helper.HexToByte(hex);
                _socket.Send(message, 0, message.Length, SocketFlags.None);
                _logger.LogInformation("Send => " + hex);
            }
            catch (Exception e)
            {
                _logger.LogError("Exception occurred in send packet: " + e);
                Disconnect();
            }

        }

    }
}
