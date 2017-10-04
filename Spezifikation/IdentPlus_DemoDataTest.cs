using IdentPlusLib;
using NUnit.Framework;

namespace Spezifikation
{
    [TestFixture]
    public class IdentPlus_DemoDataTest : IdentPlus_Testbase
    {
        protected override IdentAbfrage SUT()
        {
            return DemoData.Abfrage;
        }
    }
}