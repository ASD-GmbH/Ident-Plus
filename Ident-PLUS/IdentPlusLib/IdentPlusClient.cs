using System;
using System.Threading.Tasks;

namespace IdentPlusLib
{
    public class IdentPlusClient : IDisposable
    {
        private readonly MessagingClient _client;

        public IdentPlusClient(MessagingClient netMqClient)
        {
            _client = netMqClient;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public Task<Reply> IdentDatenAbrufen(Query query)
        {
            var token = System.Text.Encoding.UTF8.GetBytes(query.Token);
            var buffer = new byte[4+4+token.Length];
            Array.Copy(BitConverter.GetBytes(Constants.VERSION_1), 0, buffer, 0, 4);
            Array.Copy(BitConverter.GetBytes(token.Length), 0, buffer, 4, 4);
            Array.Copy(token, 0, buffer, 8, token.Length);
            return _client.RequestResponse(buffer)
                .ContinueWith(task =>
                {
                    var messagecode = BitConverter.ToInt32(task.Result, 0);

                    if (messagecode == Constants.REPLY_NOTFOUND) return NotFound.Instance;
                    if (messagecode == Constants.REPLY_ERROR) return Parse_ERROR_Datagram(task);
                    if (messagecode == Constants.REPLY_RDPINFO) return Parse_RDPINFO_Datagram(task);
                    return new InternalError($"Unbekannter MessageCode: {messagecode}!");
                });

        }

        private static RDPInfos Parse_RDPINFO_Datagram(Task<byte[]> task)
        {
            var namelength = BitConverter.ToInt32(task.Result, 4);
            var addresslength = BitConverter.ToInt32(task.Result, 8);
            var usernamelength = BitConverter.ToInt32(task.Result, 12);

            var name = System.Text.Encoding.UTF8.GetString(task.Result, 16, namelength);
            var address = System.Text.Encoding.UTF8.GetString(task.Result, 16 + namelength, addresslength);
            var username = System.Text.Encoding.UTF8.GetString(task.Result, 16 + namelength + addresslength, usernamelength);

            return new RDPInfos(name, address, username);
        }

        private static InternalError Parse_ERROR_Datagram(Task<byte[]> task)
        {
            var infolength = BitConverter.ToInt32(task.Result, 4);
            var info = System.Text.Encoding.UTF8.GetString(task.Result, 8, infolength);
            return new InternalError(info);
        }
    }
}