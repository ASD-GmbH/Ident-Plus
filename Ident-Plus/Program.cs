using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Interface;

namespace ASD.Ident_Plus
{
    class Program
    {
        /// <summary>
        /// Update der Nutzlast via Appdomain (un)loading
        /// Updater über doppel-Exe-Ping-Pong, ermöglicht Icon Austausch zu späterem Zeitpunkt
        /// On-the-fly Update
        /// </summary>



        private static void Main()
        {
            while (true)
            {
                var dllfile = "Ident";
                if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "//" + dllfile + ".dll"))
                {
                    var domain = AppDomain.CreateDomain("UpdaterDomain");
                    domain.DomainUnload += (sender, args) => Console.WriteLine("Unloaded");

                    var komponente = domain.CreateInstanceAndUnwrap(dllfile, "Ident.UpdateablePlus") as IUpdateable;
                    komponente?.Run();

                    AppDomain.Unload(domain);
                    Thread.Sleep(500);
                }
            }
        }
    }
}
