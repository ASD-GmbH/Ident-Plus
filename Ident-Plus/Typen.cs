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

        public enum Auswertungsart
        {
            NeuerChip,
            GleicherChip,
            KeinChip,
            UngenauerChip,
            Datenfehler
        }

        public struct Auswertung
        {
            public readonly string ChipID;
            public readonly Auswertungsart Art;

            public Auswertung(Auswertungsart art, string chipId = "")
            {
                Art = art;
                ChipID = chipId;
            }
        }
    }
}