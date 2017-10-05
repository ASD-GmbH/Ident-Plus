using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows.Forms;

namespace Ident_PLUS
{
    public class RemoteSitzung
    {
        private static int TscPid { set; get; }
        private static string Folder { set; get; } = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);


        public static void Start(string rdp_username, string rdp_adresse, string rdp_basis_pfad)
        {
            if (TscPid != 0) Stop();
            if (rdp_adresse != "")
            {
                {
                    var RDPGrundeinstellungen = $"{(rdp_basis_pfad != "" ? rdp_basis_pfad : Folder)}\\basis.rdp"; // Fallback auf RDP-Basis im App-Verzeichnis
                    if (File.Exists(RDPGrundeinstellungen))
                    {
                        string text = File.ReadAllText(RDPGrundeinstellungen);
                        text = text + $"username:s:{rdp_username}";
                        File.WriteAllText($"{Folder}\\ASDRDP.rdp", text);
                        var p = new Process();
                        p.StartInfo.FileName = "c:\\windows\\system32\\mstsc.exe";
                        p.StartInfo.Arguments = $"{Folder}\\ASDRDP.rdp /v:{rdp_adresse}";
                        p.Start();
                        TscPid = p.Id;
                    }
                    else
                    {
                        MessageBox.Show($@"Die Datei mit den RDP-Grundeinstellungen ({RDPGrundeinstellungen}) wurde nicht gefunden! Das Programm wird beendet.",
                            @"Quelle fehlt",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1);
                        Program.Beenden();
                    }
                }
            }
        }

        public static void Stop()
        {
            if (TscPid != 0)
            {
                foreach (Process process in Process.GetProcessesByName("mstsc"))
                {
                    if (process.Id == TscPid && !process.HasExited) Loesche_Prozess(process); // Wenn der Prozess selbst noch vorhanden wird, wird er hier gekillt
                    // Nach kurzer Zeit startet der RDP-Client einen oder mehrere Childprozesse und der ursprüngliche Prozess wird beendet.
                    // Daher werden im folgenden alle mstsc-Prozesse abgefragt, ob der Ursprungsprozess ihr Parent war und, wenn ja, gekillt.
                    var query = $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {process.Id}";
                    var search = new ManagementObjectSearcher("root\\CIMV2", query);
                    foreach (var queryObj in search.Get())
                    {
                        var parentId = (uint)queryObj["ParentProcessId"];
                        if (TscPid == parentId)
                        {
                            Loesche_Prozess(process);
                        }
                    }
                }
                TscPid = 0;

                if (File.Exists($"{Folder}\\ASDRDP.rdp")) File.Delete($"{Folder}\\ASDRDP.rdp");
            }
        }

        private static void Loesche_Prozess(Process process)
        {
            Console.WriteLine($@"MSTSC-Prozess mit PID {process.Id} wird beendet.");
            process.Kill();
        }
    }
}