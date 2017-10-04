using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IdentPlusLib;
using Ident_PLUS.Properties;

namespace Ident_PLUS
{
    class Program
    {

        private static NotifyIcon _trayIcon;
        private static IHardwareInterface _chipleser;
        private static Typen.ChipStatus _chipStatus = Typen.ChipStatus.KeinChip;
        private static IdentPlusClient _identplusclient;


        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);





        static void Main(string[] args)
        {
            _identplusclient = new IdentPlusClient(new NetMQClient(System.Configuration.ConfigurationManager.ConnectionStrings["IdentPlusServer"].ConnectionString));
            //var info = client.IdentDatenAbrufen(new Query("")).Result;

            var konsolensichtbarkeit = args.Contains("/k") ? (int)Typen.Sichtbarkeit.sichtbar : (int)Typen.Sichtbarkeit.unsichtbar;
            var handle = GetConsoleWindow();
            ShowWindow(handle, konsolensichtbarkeit);
            Console.WriteLine(@"### Ident-PLUS Servicekonsole ###");


            Extended_Main();
            Application.Run();
        }

        private static void Extended_Main()
        {
            var trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Beenden", OnExit);
            _trayIcon = new NotifyIcon
            {
                Text = @"RDP-Watcher",
                Icon = Resources.p_plus_icon,
                ContextMenu = trayMenu,
                Visible = true
            };

            Chipleser_verbinden();

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
                Program.Balloninfo("Com-Ports", meldung, 2000);
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
            if (_chipStatus == Typen.ChipStatus.KeinChip)
            {
                _chipStatus = Typen.ChipStatus.EinChip;
                var reply = _identplusclient.IdentDatenAbrufen(new Query(chipID)).Result;
                var userdaten = reply
                Daten_anzeigen(userdaten);
                RemoteSitzung.Start(userdaten.RDPUser, userdaten.RDPAddr, _config.RDPBasisFile);
            }
        }

        private static void OnChipEntfernt()
        {
            RemoteSitzung.Stop();
            KeinChip_Meldung_ausgeben();
            _chipStatus = Typen.ChipStatus.KeinChip;
        }

        private static void OnReaderGetrennt()
        {
            RemoteSitzung.Stop();
            _chipleser.Close();
            _chipStatus = Typen.ChipStatus.KeinChip;
            var dialogResult = Reader_getrennt_Dialog_ausgeben();
            if (dialogResult == DialogResult.Retry) Chipleser_verbinden();
            else Beenden();
        }

        private static void OnMehrereChips()
        {
            if (_chipStatus == Typen.ChipStatus.MehrereChips) return;
            _chipStatus = Typen.ChipStatus.MehrereChips;
            RemoteSitzung.Stop();
            Mehrere_Chips_Dialog_ausgeben();
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
