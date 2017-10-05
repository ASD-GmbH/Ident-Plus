using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IdentPlusLib;
using Ident_PLUS.Properties;
using static Ident_PLUS.Typen;

namespace Ident_PLUS
{
    class Program
    {

        private static NotifyIcon _trayIcon;
        private static IHardwareInterface _chipleser;
        private static ChipStatus _chipStatus = ChipStatus.KeinChip;
        private static IdentPlusClient _identplusclient;
        private static String _rdpBasisPfad;


        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);





        static void Main(string[] args)
        {


            var konsolensichtbarkeit = args.Contains("/k") ? (int)Sichtbarkeit.sichtbar : (int)Sichtbarkeit.unsichtbar;
            var handle = GetConsoleWindow();
            ShowWindow(handle, konsolensichtbarkeit);
            Console.WriteLine(@"### Ident-PLUS Servicekonsole ###");

            var trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Beenden", OnExit);
            _trayIcon = new NotifyIcon
            {
                Text = @"RDP-Watcher",
                Icon = Resources.p_plus_icon,
                ContextMenu = trayMenu,
                Visible = true
            };

            _rdpBasisPfad = System.Configuration.ConfigurationManager.AppSettings["RDPBasisPfad"];
            var serveradresse = System.Configuration.ConfigurationManager.ConnectionStrings["IdentPlusServer"].ConnectionString;
            if (serveradresse == "DEMO")
            {
                serveradresse = "tcp://127.0.0.1:15289";
                var server = new NetMQServer(serveradresse, new IdentPlusServer(DemoData.Abfrage));
            }

            Console.WriteLine($"Nutze Datenserver unter {serveradresse}");
            _identplusclient = new IdentPlusClient(new NetMQClient(serveradresse));


            Chipleser_verbinden();

            Application.Run();
        }



        private static void OnExit(object sender, EventArgs e)
        {
            Beenden();
        }

        public static void Beenden()
        {
            _trayIcon.Dispose();
            if (Application.MessageLoop) Application.Exit(); // Schließen einer WinForms app
            else Environment.Exit(1); // Schließen einer Console app
        }


        private static void Chipleser_verbinden()
        {
            var ergebnis = Datafox_TSHRW38_SerialPort.Initialisiere_Chipleser(OnChipAufgelegt, OnChipEntfernt, OnReaderGetrennt, OnMehrereChips);
            _chipleser = ergebnis.Item1;
            var verbindungsinfo = ergebnis.Item2;

            if (_chipleser == null)
            {
                var dialogResult = Kein_Reader_gefunden_Dialog_ausgeben();
                if (dialogResult == DialogResult.Retry) Chipleser_verbinden();
                else Beenden();
            }
            else
            {
                var meldung = $"Verbunden: {verbindungsinfo}";
                Console.WriteLine(meldung);
                Balloninfo("Com-Ports", meldung, 2000);
                _chipleser.Open();
            }
        }

        private static void Balloninfo(string title, string text, int duration)
        {
            _trayIcon.BalloonTipTitle = title;
            _trayIcon.BalloonTipText = text;
            _trayIcon.ShowBalloonTip(duration);
        }

        private static void OnChipAufgelegt(string chipID)
        {
            if (_chipStatus == ChipStatus.KeinChip)
            {
                _chipStatus = ChipStatus.EinChip;
                Console.WriteLine(@"Chip aufgelegt, frage Nutzerdaten ab...");
                var userdaten = Benutzer_nachschlagen(chipID);
                Daten_anzeigen(userdaten);
                RemoteSitzung.Start(userdaten.RDPUser, userdaten.RDPAddr, _rdpBasisPfad);
            }
        }

        private static void OnChipEntfernt()
        {
            RemoteSitzung.Stop();
            KeinChip_Meldung_ausgeben();
            _chipStatus = ChipStatus.KeinChip;
        }

        private static void OnReaderGetrennt()
        {
            RemoteSitzung.Stop();
            _chipleser.Close();
            _chipStatus = ChipStatus.KeinChip;
            var dialogResult = Reader_getrennt_Dialog_ausgeben();
            if (dialogResult == DialogResult.Retry) Chipleser_verbinden();
            else Beenden();
        }

        private static void OnMehrereChips()
        {
            if (_chipStatus == ChipStatus.MehrereChips) return;
            _chipStatus = ChipStatus.MehrereChips;
            RemoteSitzung.Stop();
            Mehrere_Chips_Dialog_ausgeben();
        }

        private static Benutzer Benutzer_nachschlagen(string chipID)
        {
            var reply = _identplusclient.IdentDatenAbrufen(new Query(chipID)).Result;

            if (reply is RDPInfos infos) return new Benutzer {ChipID = chipID, Name = infos.Name, RDPAddr = infos.RDPAdresse, RDPUser = infos.RDPUserName};
            if (reply is InternalError error)
            {
            }
            return new Benutzer { ChipID = chipID, Name = "???", RDPAddr = "", RDPUser = "" };
        }

        private static void Daten_anzeigen(Benutzer daten)
        {
            Console.WriteLine($@"Daten erhalten: ChipID:{daten.ChipID} - Name:{daten.Name} - RDP-Addr:{daten.RDPAddr} - RDP-User: {daten.RDPUser}");

            Balloninfo("Daten erhalten",
                        $"ChipID: {daten.ChipID} - {daten.Name}\nRDP: {daten.RDPUser} @ {daten.RDPAddr}\n",
                        7500);
        }


        private static void KeinChip_Meldung_ausgeben()
        {
            Console.WriteLine("Kein Chip aufgelegt");

            Balloninfo("Chip entfernt", "Der Chip wurde vom Lesegerät entfernt", 5000);
        }


        private static DialogResult Reader_getrennt_Dialog_ausgeben()
        {
            Console.WriteLine(@"Der verwendete Chipleser ist nicht mehr erreichbar. Wurde die Verbindung getrennt?");
            DialogResult result = MessageBox.Show(
                @"Verbindung zum Chipleser wurde getrennt! \nBitte neu verbinden.",
                @"IO-Fehler",
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);
            return result;
        }


        private static void Mehrere_Chips_Dialog_ausgeben()
        {
            Console.WriteLine(@"Mehrere Transponder sind aufgelegt.");
            Console.WriteLine(@"Bitte alle Transponder entfernen und anschließend nur einen auflegen.");
            MessageBox.Show(
                "Mehrere Transponder sind aufgelegt.\nBitte alle Transponder entfernen und anschließend nur einen auflegen.",
                @"Mehrere Transponder",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);
        }



        private static DialogResult Kein_Reader_gefunden_Dialog_ausgeben()
        {
            DialogResult result = MessageBox.Show(
                @"Es wurde kein Chipleser gefunden!\nBitte verbinden Sie das Gerät und versuchen Sie es erneut.",
                @"IO-Fehler",
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);
            return result;
        }

    }
}
