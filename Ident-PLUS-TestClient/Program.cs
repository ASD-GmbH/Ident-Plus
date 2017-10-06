using System;
using IdentPlusLib;

namespace Ident_PLUS_TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Out.WriteLine("Ident-PLUS command line client");
            if (args.Length != 2)
            {
                Console.Out.WriteLine("Usage: ident-PLUS-TestClient.exe <tcp://host:port> <token>");
            }
            else
            {
                var hostport = args[0];
                var token = args[1];

                using (var client = new IdentPlusLib.IdentPlusClient(new NetMQClient(hostport)))
                {
                    var task = client.IdentDatenAbrufen(new Query(token));
                    if (task.Wait(TimeSpan.FromMilliseconds(2000)))
                    {
                        var reply = task.Result;
                        if (reply is RDPInfos)
                        {
                            var rdp = (RDPInfos) reply;
                            Console.Out.WriteLine($"RDP: '{rdp.Name} ({rdp.RDPUserName})' - {rdp.RDPAdresse}");
                        }
                        else if (reply is NotFound)
                        {
                            Console.Out.WriteLine("Token not found!");
                        }
                        else if (reply is InternalError)
                        {
                            var error = (InternalError) reply;
                            Console.Out.WriteLine("Error: " + error.ErrorInfo);
                        }
                        else
                        {
                            Console.Out.WriteLine("Unexpected reply type: " + (reply.GetType().Name));
                        }
                    }
                    else
                    {
                        Console.Out.WriteLine("Timeout!");
                    }
                }
            }
        }
    }
}
