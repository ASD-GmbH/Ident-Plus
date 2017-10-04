using System;
using NetMQ;
using NetMQ.Sockets;

namespace IdentPlusLib
{
    public class NetMQServer : IDisposable
    {
        private readonly IdentPlusServer _backend;
        private readonly ResponseSocket _server;
        private readonly NetMQPoller _poller;

        public NetMQServer(string tcpaddress, IdentPlusServer backend)
        {
            _backend = backend;
            _server = new ResponseSocket();
            _server.ReceiveReady += OnReceiveReady;
            _server.Bind(tcpaddress);
            _poller = new NetMQPoller();
            _poller.Add(_server);
            _poller.RunAsync();
        }

        private void OnReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            var data = e.Socket.ReceiveFrameBytes();
            var reply = _backend.RequestResponse(data).Result;
            e.Socket.SendFrame(reply);
        }

        public void Dispose()
        {
            _poller.Dispose();
            _server.Dispose();
        }
    }
}