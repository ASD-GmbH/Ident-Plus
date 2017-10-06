﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using IdentPlusLib;
using Ident_PLUS.Properties;
using static Ident_PLUS.Typen;

namespace Ident_PLUS
{
    static class Program
    {

        private static NotifyIcon _trayIcon;
        private static IHardwareInterface _chipleser;
        private static ChipStatus _chipStatus = ChipStatus.KeinChip;
        private static IdentPlusClient _identplusclient;
        private static NetMQServer _demoNetMqServer;
        private static String _rdpBasis;
        private static int _konsolensichtbarkeit;


        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);



        static void Main(string[] args)
        {
            _konsolensichtbarkeit = args.Contains("/k") ? (int)Sichtbarkeit.sichtbar : (int)Sichtbarkeit.unsichtbar;
            var handle = GetConsoleWindow();
            ShowWindow(handle, _konsolensichtbarkeit);
            Console.WriteLine(@"### Ident-PLUS Servicekonsole ###");

            _trayIcon = Tray_einrichten();
            _rdpBasis = Lade_RDPBasis(System.Configuration.ConfigurationManager.AppSettings["RDPBasisDatei"]);

            var serveradresse = System.Configuration.ConfigurationManager.ConnectionStrings["IdentPlusServer"].ConnectionString;
            if (serveradresse == "DEMO")
            {
                const string demoadresse = "tcp://127.0.0.1:15289";
                Console.WriteLine($@"Nutze lokalen DEMO-Datenserver unter {demoadresse}");
                _demoNetMqServer = new NetMQServer(demoadresse, new IdentPlusServer(DemoData.Abfrage));
                _identplusclient = new IdentPlusClient(new NetMQClient(demoadresse));
            }
            else
            {
                Console.WriteLine($@"Nutze Datenserver unter {serveradresse}");
                _identplusclient = new IdentPlusClient(new NetMQClient(serveradresse));
            }

            Chipleser_verbinden();

            Application.Run();
        }



        private static NotifyIcon Tray_einrichten()
        {
            var trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Beenden", OnExit);
            trayMenu.MenuItems.Add("Konsole an-/ausschalten", ToggleKonsole);
            return new NotifyIcon
            {
                Text = @"ASD Ident-PLUS",
                Icon = Resources.ident_plus,
                ContextMenu = trayMenu,
                Visible = true
            };
        }


        private static string Lade_RDPBasis(string rdpBasisDatei)
        {
            var folder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var rdpGrundeinstellungen = (rdpBasisDatei != "" ? rdpBasisDatei : $"{folder}\\basis.rdp"); // Fallback auf RDP-Basis im App-Verzeichnis

            if (File.Exists(rdpGrundeinstellungen))
            {
                Console.WriteLine(@"Nutze RDP-Basiseinstellungen aus " + rdpGrundeinstellungen);
                return File.ReadAllText(rdpGrundeinstellungen);
            }
            Console.WriteLine($@"Nutze KEINE RDP-Grundeinstellungen. ({rdpGrundeinstellungen} wurde nicht gefunden)");
            return "";
        }


        private static void Chipleser_verbinden()
        {
            var ergebnis = Datafox_TSHRW38_SerialPort.Initialisiere_Chipleser(OnChipAufgelegt, OnChipEntfernt, OnReaderGetrennt, OnMehrereChips);
            if (ergebnis == null)
            {
                var dialogResult = Kein_Reader_gefunden_Dialog_ausgeben();
                if (dialogResult == DialogResult.Retry) Chipleser_verbinden();
                else Beenden();
            }
            else
            {
                _chipleser = ergebnis.Item1;
                var verbindungsinfo = ergebnis.Item2;

                var meldung = $"Verbunden: {verbindungsinfo}";
                Console.WriteLine(meldung);
                Balloninfo("Com-Ports", meldung, 2000);
                _chipleser.Open();
            }
        }


        private static void OnExit(object sender, EventArgs e)
        {
            Beenden();
        }

        private static void Beenden()
        {
            _trayIcon?.Dispose();
            _identplusclient?.Dispose();
            _demoNetMqServer?.Dispose();
            if (Application.MessageLoop) Application.Exit(); // Schließen einer WinForms app
            else Environment.Exit(0); // Schließen einer Console app
        }



        private static void OnChipAufgelegt(string chipID)
        {
            if (_chipStatus == ChipStatus.KeinChip)
            {
                _chipStatus = ChipStatus.EinChip;
                Console.WriteLine(@"Chip aufgelegt, frage Nutzerdaten ab...");
                var userdaten = Benutzer_nachschlagen(chipID);
                Daten_anzeigen(userdaten);
                RemoteSitzung.Start(userdaten.RDPUser, userdaten.RDPAddr, _rdpBasis);
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
                Datenfehler_Meldung_ausgeben(error);
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
                "Verbindung zum Chipleser wurde getrennt!\nBitte neu verbinden.",
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
                "Es wurde kein Chipleser gefunden!\nBitte verbinden Sie das Gerät und versuchen Sie es erneut.",
                @"IO-Fehler",
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);
            return result;
        }


        private static void Datenfehler_Meldung_ausgeben(InternalError error)
        {
            Console.WriteLine(@"Kommunikationsfehler: " + error);
            MessageBox.Show(
                @"Bei der Abfrage der Daten vom Server ist ein Fehler aufgetreten: " + error,
                @"Kommunikationsfehler",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button1);
        }


        private static void Balloninfo(string title, string text, int duration)
        {
            _trayIcon.BalloonTipTitle = title;
            _trayIcon.BalloonTipText = text;
            _trayIcon.ShowBalloonTip(duration);
        }

        private static void ToggleKonsole(object sender, EventArgs e)
        {
            _konsolensichtbarkeit = _konsolensichtbarkeit == (int) Sichtbarkeit.unsichtbar ? (int) Sichtbarkeit.sichtbar : (int) Sichtbarkeit.unsichtbar;
            var handle = GetConsoleWindow();
            ShowWindow(handle, _konsolensichtbarkeit);
        }
    }
}
