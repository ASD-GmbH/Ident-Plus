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

        private static string _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);


        private static void Main()
        {



            while (true)
            {
                var dllName = "Ident";
                if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "//" + dllName + ".dll"))
                {
                    var domain = AppDomain.CreateDomain(dllName + "Domain");
                    domain.DomainUnload += (sender, args) => Console.WriteLine("Domain Unloaded");

                    var IdentAPP = domain.CreateInstanceAndUnwrap(dllName, "Ident.UpdateablePlus") as IUpdateable;
                    IdentAPP?.Run();

                    if (App_Update_Verfuegbar() && (IdentAPP == null || IdentAPP.AllowsUpdate())) Update_App(domain);


                    Thread.Sleep(500);
                }
            }
        }




        private static void Update_App(AppDomain identdomain)
        {
            Console.WriteLine("APP_Update wird durchgeführt!");
            AppDomain.Unload(identdomain);
            File.Delete(_path + "//Ident.dll");
            File.Move(_path + "//Update//Ident.dll", _path + "//Ident.dll");

            // App neu laden
        }

        private static void Update_Updater(AppDomain identdomain)
        {
            Console.WriteLine("Updater-update wird durchgeführt!");
            AppDomain.Unload(identdomain);

            // sich selbst beenden und löschen
            // neuen Updater bewegen und starten

        }


        private static bool App_Update_Verfuegbar()
        {
            return File.Exists(_path + "//Update//Ident.dll");
        }

        private static bool Updater_Update_Verfuegbar()
        {
            return File.Exists(_path + "//Update//Ident-Plus.exe");
        }
    }
}
