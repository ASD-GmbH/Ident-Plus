using System.Threading.Tasks;

namespace IdentPlusLib
{
    public static class DemoData
    {
        public const string Token1 = "1";
        public const string Token2 = "2";

        public static Task<Reply> Abfrage(Query query)
        {
            if (query.Token == Token1) return Task.FromResult((Reply)new RDPInfos("Martina Musterfrau", "einRechner", "mm"));
            if (query.Token == Token2) return Task.FromResult((Reply)new RDPInfos("Jens Mustermann", "einandererRechner", "jm"));
            return Task.FromResult(NotFound.Instance);
        }
    }
}