using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentPlusLib;

namespace Ident_PLUS
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new IdentPlusClient(new NetMQClient(System.Configuration.ConfigurationManager.ConnectionStrings["IdentPlusServer"].ConnectionString));
            var info = client.IdentDatenAbrufen(new Query("")).Result;
        }
    }
}
