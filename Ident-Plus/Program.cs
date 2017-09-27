using System;
using System.IO;
using System.Reflection;
using System.Threading;

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
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+"\\Updater.dll";
                if (File.Exists(path))
                {
                    var domain = AppDomain.CreateDomain("UpdaterDomain");
                    domain.DomainUnload += (sender, args) =>
                    {
                        Console.WriteLine("Unloaded");
                    };

                    Loader.Call(domain, path, "Updater.Updater", "Update", null);
                    Thread.Sleep(500);
                }
            }

        }

        public class Loader : MarshalByRefObject
        {
            object CallInternal(string dll, string typename, string method, object[] parameters)
            {
                Assembly a = Assembly.LoadFile(dll);
                object o = a.CreateInstance(typename);
                Type t = o.GetType();
                MethodInfo m = t.GetMethod(method);
                return m.Invoke(o, parameters);
            }

            public static object Call(AppDomain domain, string dll, string typename, string method, params object[] parameters)
            {
                Loader ld = (Loader)domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(Loader).FullName);
                object result = ld.CallInternal(dll, typename, method, parameters);
                AppDomain.Unload(domain);
                return result;
            }
        }
    }
}
