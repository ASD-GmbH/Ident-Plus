namespace Ident_PLUS
{
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

    public enum Ergebnis
    {
        Erfolg,
        Fehler
    }

    public struct Antwort
    {
        public readonly Ergebnis Ergebnis;
        public readonly string Meldung;

        public Antwort(Ergebnis ergebnis, string meldung = "")
        {
            Ergebnis = ergebnis;
            Meldung = meldung;
        }
    }
}