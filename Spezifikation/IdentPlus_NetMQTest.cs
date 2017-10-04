using System;
using IdentPlusLib;
using NUnit.Framework;

namespace Spezifikation
{
    [TestFixture]
    public class IdentPlus_NetMQTest : IdentPlus_Testbase
    {
        private IDisposable _server;
        private IdentPlusClient _client;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _server = new NetMQServer("tcp://127.0.0.1:15289", new IdentPlusServer(DemoData.Abfrage));
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            _server.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            _client = new IdentPlusClient(new NetMQClient("tcp://127.0.0.1:15289"));
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