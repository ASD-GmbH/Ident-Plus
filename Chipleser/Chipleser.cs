using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Management;
using System.Text.RegularExpressions;


namespace Ident_PLUS
{
    public class Chipleser : IHardwareInterface
    {
        private readonly SerialPort _port;
        private readonly ManagementEventWatcher _usbWatcher;
        private readonly Action<string> _chip_wurde_aufgelegt;
        private readonly Action _chip_wurde_entfernt;
        private readonly Action _reader_wurde_getrennt;
        private readonly Action _mehrere_Chips_erkannt;
        private string _chipnummer = "";

        private Chipleser(SerialPort comport, Action<string> chip_wurde_aufgelegt, Action chip_wurde_entfernt, Action reader_wurde_getrennt, Action mehrere_Chips_erkannt)
        {
            _port = comport;
            _chip_wurde_aufgelegt = chip_wurde_aufgelegt;
            _chip_wurde_entfernt = chip_wurde_entfernt;
            _reader_wurde_getrennt = reader_wurde_getrennt;
            _mehrere_Chips_erkannt = mehrere_Chips_erkannt;

            _usbWatcher = new ManagementEventWatcher {Query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3")};
            _usbWatcher.Start();
        }

        public static Tuple<Chipleser, string> Initialisiere_Chipleser(Action<string> chip_wurde_aufgelegt, Action chip_wurde_entfernt, Action reader_wurde_getrennt, Action mehrere_Chips_erkannt)
        {
            var readerinfos = Suche_nach_gisreader();
            if (readerinfos != null)
            {
                var geraeteinfo = readerinfos.Item1;
                var comportName = readerinfos.Item2;
                var port = Initialisiere_Seriellen_Port(comportName);
                return new Tuple<Chipleser, string>(
                    new Chipleser(port, chip_wurde_aufgelegt, chip_wurde_entfernt, reader_wurde_getrennt, mehrere_Chips_erkannt),
                    $"{geraeteinfo} an {comportName}");
            }

            return null;
        }

        private static SerialPort Initialisiere_Seriellen_Port(string comportName)
        {
            return new SerialPort
            {
                BaudRate = 19200,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Handshake = Handshake.None,
                PortName = comportName
            };
        }

        private void PortOnDataReceived(object sender, SerialDataReceivedEventArgs serialDataReceivedEventArgs)
        {
            var empfangeneDaten = ((SerialPort)sender).ReadExisting();
            var auswertung = Datafox_TSHRW38.Werte_aus(empfangeneDaten, _chipnummer);

            switch (auswertung.Art)
            {
                case Auswertungsart.NeuerChip:
                    _chip_wurde_aufgelegt(auswertung.ChipID);
                    _chipnummer = auswertung.ChipID;
                    break;
                case Auswertungsart.GleicherChip:
                    break;
                case Auswertungsart.KeinChip:
                    _chip_wurde_entfernt();
                    _chipnummer = "";
                    break;
                case Auswertungsart.UngenauerChip:
                    _mehrere_Chips_erkannt();
                    break;
                case Auswertungsart.Datenfehler:
                    break;
            }
        }

        private void OnUSBDisconnect(object sender, EventArrivedEventArgs eventArgs)
        {
            if (_port.IsOpen == false) _reader_wurde_getrennt();
        }

        public Antwort Open()
        {
            _usbWatcher.EventArrived += OnUSBDisconnect;
            _port.DataReceived += PortOnDataReceived;
            try
            {
                if (!_port.IsOpen) _port.Open();
                return new Antwort(Ergebnis.Erfolg);
            }
            catch (UnauthorizedAccessException e)
            {
                return new Antwort(Ergebnis.Fehler, e.Message);
            }
        }

        public string Chipnummer()
        {
            return _chipnummer;
        }

        public void Close()
        {
            _usbWatcher.EventArrived -= OnUSBDisconnect;
            _port.DataReceived -= PortOnDataReceived;
            _port.Close();
            Debug.WriteLine("Port geschlossen");
            _port.Dispose();
        }


        private static Tuple<string, string> Suche_nach_gisreader()
        {

            ManagementObjectSearcher mbs = new ManagementObjectSearcher("Select * From Win32_SerialPort");
            ManagementObjectCollection mbsList = mbs.Get();
            foreach (var mo in mbsList)
            {
                Match match = Regex.Match((string)mo["PNPDeviceID"], @"USB\\VID_(\w{4})&PID_(\w{4})\\([0-9-]*)");
                if (match.Success)
                {
                    var geraeteinfo = "";
                    var betrachteter_COMPort = mo["DeviceID"].ToString();
                    var vid = match.Groups[1].Value;
                    var pid = match.Groups[2].Value;
                    var seriennummer = match.Groups[3].Value;
                    if (vid == "1C40") // 1C40 für Datafox/GISmbH
                    {
                        if (pid == "05AC") geraeteinfo = "Gisreader TSHRW38 " + seriennummer;
                        //else if  // Hier koennen weitere Produkt-IDs für GisReader eingetragen werden

                    }
                    //else if  // Hier koennen weitere Hersteller-IDs eingetragen werden

                    if (geraeteinfo != "")
                    {
                        return new Tuple<string, string>(geraeteinfo, betrachteter_COMPort);
                    }
                }
            }
            return null;
        }
    }
}