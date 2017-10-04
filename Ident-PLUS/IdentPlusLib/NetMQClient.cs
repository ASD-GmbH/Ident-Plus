using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace IdentPlusLib
{
    public class NetMQClient : MessagingClient
    {
        private readonly RequestSocket _client;
        private readonly object _socketlock = new object();

        public NetMQClient(string tcpaddress)
        {
            _client = new RequestSocket(tcpaddress);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public Task<byte[]> RequestResponse(byte[] data)
        {
            return
                Task<byte[]>.Factory.StartNew(() =>
                {
                    lock (_socketlock)
                    {
                        _client.SendFrame(data);
                        return _client.ReceiveFrameBytes();
                    }
                });
        }
    }
}