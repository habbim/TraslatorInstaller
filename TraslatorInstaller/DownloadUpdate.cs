using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;

namespace TraslatorInstaller
{
    public partial class DownloadUpdate : Form
    {
        public DownloadUpdate()
        {
            InitializeComponent();
            this.Shown += new System.EventHandler(this.DownloadUpdate_Shown);

        }
        private void DownloadUpdate_Shown(object sender, EventArgs e)
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadComplete);
                wc.DownloadFileAsync(new System.Uri("https://subtitulos.eu/lifeisstrange/update/"+Form1.DataVer[4]),
                Program.dirDownload+@"\update.exe");

            }
        }
        void wc_DownloadComplete(object sender, AsyncCompletedEventArgs c)
        {
            MessageBox.Show("Descarga completada correctamente");
            Process.Start(Program.dirDownload + @"\update.exe");
            Application.Exit();
        }
        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;



        }
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("¿Estás seguro de que quieres salir?", "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("¿Estás seguro de que quieres cancelar la actualización?", "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}
