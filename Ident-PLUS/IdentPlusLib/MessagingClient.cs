using System;
using System.Threading.Tasks;

namespace IdentPlusLib
{
    public interface MessagingClient : IDisposable
    {
        Task<byte[]> RequestResponse(byte[] data);
    }
}