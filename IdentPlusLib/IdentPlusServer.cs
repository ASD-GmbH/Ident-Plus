using System;
using System.Threading.Tasks;

namespace IdentPlusLib
{
    public class IdentPlusServer : MessagingClient
    {
        private readonly IdentAbfrage _callback;

        public IdentPlusServer(IdentAbfrage callback)
        {
            _callback = callback;
        }

        public void Dispose()
        {
        }

        public Task<byte[]> RequestResponse(byte[] data)
        {
            var version = BitConverter.ToInt32(data, 0);
            if (version != Constants.VERSION_1)
            {
                return Task.FromResult(EncodeError($"Version {version} nicht unterstützt!"));
            }
            else
            {
                var tokenlength = BitConverter.ToInt32(data, 4);
                var token = System.Text.Encoding.UTF8.GetString(data, 8, tokenlength);
                return _callback(new Query(token)).ContinueWith(task =>
                {
                    return (byte[])Serialize((dynamic)task.Result);
                });
            }
        }

        private byte[] EncodeError(string info)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(info);
            var buffer = new Byte[4 + 4 + bytes.Length];
            Array.Copy(BitConverter.GetBytes(Constants.REPLY_ERROR), 0, buffer, 0, 4);
            Array.Copy(BitConverter.GetBytes(bytes.Length), 0, buffer, 8, 4);
            Array.Copy(bytes, 0, buffer, 12, bytes.Length);
            return buffer;
        }

        private byte[] Serialize(NotFound instance)
        {
            return BitConverter.GetBytes(Constants.REPLY_NOTFOUND);
        }

        private byte[] Serialize(InternalError error)
        {
            return EncodeError(error.ErrorInfo);
        }

        private byte[] Serialize(RDPInfos infos)
        {
            var name = System.Text.Encoding.UTF8.GetBytes(infos.Name);
            var address = System.Text.Encoding.UTF8.GetBytes(infos.RDPAdresse);
            var user = System.Text.Encoding.UTF8.GetBytes(infos.RDPUserName);
            var buffer = new byte[4+4+4+4+name.Length+address.Length+user.Length];
            Array.Copy(BitConverter.GetBytes(Constants.REPLY_RDPINFO), 0, buffer, 0, 4);
            Array.Copy(BitConverter.GetBytes(name.Length), 0, buffer, 4, 4);
            Array.Copy(BitConverter.GetBytes(address.Length), 0, buffer, 8, 4);
            Array.Copy(BitConverter.GetBytes(user.Length), 0, buffer, 12, 4);
            Array.Copy(name, 0, buffer, 16, name.Length);
            Array.Copy(address, 0, buffer, 16+name.Length, address.Length);
            Array.Copy(user, 0, buffer, 16+name.Length+address.Length, user.Length);
            return buffer;
        }
    }
}