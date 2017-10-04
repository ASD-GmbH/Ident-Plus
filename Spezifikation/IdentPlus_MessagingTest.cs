using IdentPlusLib;
using NUnit.Framework;

namespace Spezifikation
{
    [TestFixture]
    public class IdentPlus_MessagingTest : IdentPlus_Testbase
    {
        private IdentPlusServer _server;
        private IdentPlusClient _client;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _server = new IdentPlusServer(DemoData.Abfrage);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            _server.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            _client = new IdentPlusClient(_server);
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
        }

        protected override IdentAbfrage SUT()
        {
            return _client.IdentDatenAbrufen;
        }
    }
}