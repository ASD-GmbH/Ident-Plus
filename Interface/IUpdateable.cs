using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Interface
{
    public interface IUpdateable
    {
        void Run();
        void Terminate();
        void Update();
    }
}
