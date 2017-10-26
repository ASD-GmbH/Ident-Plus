namespace IdentPlusKonfiguration
{
    partial class Eingabefenster
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_chipnummer_einlesen = new System.Windows.Forms.Button();
            this.text_chipnummer = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btn_chipnummer_einlesen
            // 
            this.btn_chipnummer_einlesen.Location = new System.Drawing.Point(210, 12);
            this.btn_chipnummer_einlesen.Name = "btn_chipnummer_einlesen";
            this.btn_chipnummer_einlesen.Size = new System.Drawing.Size(134, 20);
            this.btn_chipnummer_einlesen.TabIndex = 0;
            this.btn_chipnummer_einlesen.Text = "Chipnummer einlesen";
            this.btn_chipnummer_einlesen.UseVisualStyleBackColor = true;
            this.btn_chipnummer_einlesen.Click += new System.EventHandler(this.btn_chipnummer_einlesen_Click);
            // 
            // text_chipnummer
            // 
            this.text_chipnummer.Location = new System.Drawing.Point(12, 12);
            this.text_chipnummer.Name = "text_chipnummer";
            this.text_chipnummer.Size = new System.Drawing.Size(192, 20);
            this.text_chipnummer.TabIndex = 1;
            // 
            // Eingabefenster
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(354, 42);
            this.Controls.Add(this.text_chipnummer);
            this.Controls.Add(this.btn_chipnummer_einlesen);
            this.Name = "Eingabefenster";
            this.Text = "Ident Plus Konfiguration";
            this.Load += new System.EventHandler(this.Eingabefenster_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_chipnummer_einlesen;
        private System.Windows.Forms.TextBox text_chipnummer;
    }
}

