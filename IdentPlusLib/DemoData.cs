using System.Threading.Tasks;

namespace IdentPlusLib
{
    public static class DemoData
    {
        public const string Token1 = "3084763134";
        public const string Token2 = "3084906750";
        public const string TestFehlermeldung = "Angeforderter Fehler zu Testzwecken.";

        public static Task<Reply> Abfrage(Query query)
        {
            if (query.Token == "DEMODATA_THROW") return Task.FromResult((Reply) new InternalError(TestFehlermeldung));
            if (query.Token == Token1) return Task.FromResult((Reply)new RDPInfos("Jan Dübbers", "web", "ASDOS\\jdübbers"));
            if (query.Token == Token2) return Task.FromResult((Reply)new RDPInfos("Hans Wurst", "", "hw"));
            return Task.FromResult(NotFound.Instance);
        }
    }
}