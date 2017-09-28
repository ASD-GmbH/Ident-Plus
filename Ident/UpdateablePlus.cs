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

        public void Update()
        {
            Console.WriteLine("Update wird ausgeführt!");
        }
    }
}
