using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Interface;

namespace ASD.Ident_Plus
{
    [System.Runtime.InteropServices.Guid("7F0312D4-950A-423B-8BDB-08DC878021D1")]
    class Program
    {
        /// <summary>
        /// Update der Nutzlast via Appdomain (un)loading
        /// Updater über doppel-Exe-Ping-Pong, ermöglicht Icon Austausch zu späterem Zeitpunkt
        /// On-the-fly Update
        /// </summary>

        private static string _path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private const string _dateiname = "Ident-Plus";


        private static void Main()
        {
            var test = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            Console.WriteLine($"Ausgeführt wird: {test}");
            if (test == $"{_dateiname}.update.exe")
            {
                Console.WriteLine("Temporäre Exe wurde gestartet... Diese ersetzt nun das eigentliche Programm.");
                Thread.Sleep(1000); // erstmal zur Sicherheit, damit der alte Prozess beendet werden wurde, bevor die Datei überschrieben werden kann.
                try
                {
                    File.Copy($"{_dateiname}.update.exe", $"{_dateiname}.exe", overwrite: true);
                }
                catch (IOException)
                {
                    Console.WriteLine($"Die {_dateiname}-Programmdatei konnte nicht überschrieben werden. \n" +
                                      $"Bitte beenden Sie alle Instanzen und rufen {_dateiname}.update.exe erneut manuell auf.");
                    Console.ReadLine();
                    return;
                }
                System.Diagnostics.Process.Start($"{_dateiname}.exe");
                return;
            }
            else
            {
                Console.WriteLine("Ident-Plus läuft...");
                if (File.Exists($"{_dateiname}.update.exe"))
                {
                    Console.WriteLine("Update-Datei (alt) gefunden - wird gelöscht!");
                    File.Delete($"{_dateiname}.update.exe");
                }
            }



            while (true)
            {
                var dllName = "Ident";
                if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "//" + dllName + ".dll"))
                {
                    var domain = AppDomain.CreateDomain(dllName + "Domain");
                    domain.DomainUnload += (sender, args) => Console.WriteLine("Domain Unloaded");

                    var IdentAPP = domain.CreateInstanceAndUnwrap(dllName, "Ident.UpdateablePlus") as IUpdateable;
                    IdentAPP?.Run();

                    //if (App_Update_Verfuegbar() && (IdentAPP == null || IdentAPP.AllowsUpdate())) Update_App(domain);


                    Thread.Sleep(500);
                }
            }
        }




        private static void Update_App(AppDomain identdomain)
        {
            Console.WriteLine("APP_Update wird durchgeführt!");
            AppDomain.Unload(identdomain);
            File.Delete(_path + "//Ident.dll");
            File.Move(_path + "//Ident.update.dll", _path + "//Ident.dll");

            // App neu laden
        }

        private static void Update_Updater(AppDomain identdomain)
        {
            Console.WriteLine("Updater-update wird durchgeführt!");
            AppDomain.Unload(identdomain);

            System.Diagnostics.Process.Start($"{_dateiname}.update.exe");
            Environment.Exit(1);

        }


        private static bool App_Update_Verfuegbar()
        {
            return File.Exists(_path + "//Ident.update.dll");
        }

    }
}
