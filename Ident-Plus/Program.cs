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
                var dllName = "Ident";
                if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "//" + dllName + ".dll"))
                {
                    var domain = AppDomain.CreateDomain(dllName + "Domain");
                    domain.DomainUnload += (sender, args) => Console.WriteLine("Domain Unloaded");

                    var komponente = domain.CreateInstanceAndUnwrap(dllName, "Ident.UpdateablePlus") as IUpdateable;
                    komponente?.Run();

                    AppDomain.Unload(domain);

                    if (UpdateVerfuegbar()) FuehreUpdateDurch();

                    Thread.Sleep(500);
                }
            }
        }

        private static void FuehreUpdateDurch()
        {
            Console.WriteLine("Update wird durchgeführt!");
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            File.Delete(path + "//Ident.dll");
            File.Move(path + "//Update//Ident.dll", path + "//Ident.dll");
        }

        private static bool UpdateVerfuegbar()
        {
            return File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "//Update//Ident.dll");
        }
    }
}
