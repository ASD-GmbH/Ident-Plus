using FluentAssertions;
using IdentPlusLib;
using NUnit.Framework;

namespace Spezifikation
{
    public abstract class IdentPlus_Testbase
    {
        protected abstract IdentAbfrage SUT();

        [Test]
        public void Ein_bekanntes_Token_wird_korrekt_aufgeloest()
        {
            SUT()(new Query(DemoData.Token1)).Result.Should().Be(DemoData.Abfrage(new Query(DemoData.Token1)).Result);
            SUT()(new Query(DemoData.Token2)).Result.Should().Be(DemoData.Abfrage(new Query(DemoData.Token2)).Result);
        }

        [Test]
        public void Ein_unbekanntes_Token_wird_als_nicht_gefunden_gemeldet()
        {
            SUT()(new Query("sagdhsa785d6aszuidsa78d6tzsajkldusa78tdzhasda")).Result.Should().Be(NotFound.Instance);
        }

    }
}
