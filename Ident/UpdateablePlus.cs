using System;
using Interface;

namespace Ident
{

    public class UpdateablePlus : MarshalByRefObject, IUpdateable
    {
        public void Run()
        {
           Console.WriteLine("Run Version 2");
        }

        public void Terminate()
        {
            throw new System.NotImplementedException();
        }

        public bool AllowsUpdate()
        {
            // true, wenn keine RDP-Sitzung offen
            // wenn kein Update möglich ist, später per Callback darauf hinweisen, wenn es wieder möglich ist
            return true;
        }
    }
}
