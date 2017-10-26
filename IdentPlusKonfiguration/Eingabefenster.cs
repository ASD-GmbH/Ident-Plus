using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ident_PLUS;

namespace IdentPlusKonfiguration
{
    public partial class Eingabefenster : Form
    {
        public Eingabefenster()
        {
            InitializeComponent();
        }

        private string _chipnummer;
        private object _lock = new object();

        private void btn_chipnummer_einlesen_Click(object sender, EventArgs e)
        {
            lock (_lock)
            {
                text_chipnummer.Text = _chipnummer;
            }
        }

        private void Eingabefenster_Load(object sender, EventArgs e)
        {
            Datafox_TSHRW38_SerialPort.Initialisiere_Chipleser(
                chipnummer =>
                {
                    lock (_lock)
                    {
                        _chipnummer = chipnummer;
                    }
                }, 
                () => {}, 
                () => {}, 
                () => {}).Item1.Open();
        }
    }
}
