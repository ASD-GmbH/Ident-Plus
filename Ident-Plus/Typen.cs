namespace Ident_PLUS
{
    public class Typen
    {
        public struct Benutzer
        {
            public string ChipID { get; set; }
            public string Name { get; set; }
            public string RDPAddr { get; set; }
            public string RDPUser { get; set; }
        };

        public enum ChipStatus
        {
            EinChip,
            KeinChip,
            MehrereChips
        }

        public enum Sichtbarkeit
        {
            unsichtbar = 0,
            sichtbar = 5
        };
    }
}