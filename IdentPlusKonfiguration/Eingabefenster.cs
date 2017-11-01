using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
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

        private Datafox_TSHRW38_SerialPort chipleser;

        private void btn_chipnummer_einlesen_Click(object sender, EventArgs e)
        {
                text_chipnummer.Text = chipleser.Chipnummer();
        }

        private void Eingabefenster_Load(object sender, EventArgs e)
        {
            chipleser =
            Datafox_TSHRW38_SerialPort.Initialisiere_Chipleser(
                chipnummer => {},
                () => {}, 
                () => {}, 
                () => {}).Item1;
            chipleser.Open();
        }
    }
}
