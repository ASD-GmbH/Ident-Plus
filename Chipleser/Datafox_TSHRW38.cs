using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
namespace Ident_PLUS
{
    public class Datafox_TSHRW38
    {
        public static Auswertung Werte_aus(string input, string vorherigerChip)
        {
            var signale = Signale_auslesen(input);
            Debug.WriteLine($"Anzahl der Chips: {signale.Length}");
            if (signale.Length == 0) return new Auswertung(Auswertungsart.Datenfehler);
            if (Regex.IsMatch(signale.Last(), @"_NOCHIP")) return new Auswertung(Auswertungsart.KeinChip);
            if (signale.Length > 1) return new Auswertung(Auswertungsart.UngenauerChip);
            return Signal_auswerten(signale.First(), vorherigerChip);
        }

        private static string[] Signale_auslesen(string input)
        {
            return input
                .Trim()
                .Split('\r')
                .Distinct()
                .Select(_ => Regex.Match(_, @"ASDR(.+)\+").Groups[1].Value)
                .ToArray();
        }

        private static Auswertung Signal_auswerten(string signal, string vorherigerChip)
        {
            if (Regex.IsMatch(signal, @"\d{10}"))
            {
                var chip = signal;
                if (vorherigerChip == "") return new Auswertung(chipId: chip, art: Auswertungsart.NeuerChip);
                if (vorherigerChip != chip) return new Auswertung(Auswertungsart.UngenauerChip);
                return new Auswertung(Auswertungsart.GleicherChip);
            }
            if (Regex.IsMatch(signal, @"_NOCHIP")) return new Auswertung(Auswertungsart.KeinChip);
            return new Auswertung(Auswertungsart.Datenfehler);
        }
    }
}