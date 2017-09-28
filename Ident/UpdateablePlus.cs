using System;
using Interface;

namespace Ident
{

    public class UpdateablePlus : MarshalByRefObject, IUpdateable
    {
        public void Run()
        {
           Console.WriteLine("Run Version 1");
        }

        public void Terminate()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }
    }
}
