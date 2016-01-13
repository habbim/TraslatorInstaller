using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TraslatorInstaller
{
    public partial class SelectVersion : Form
    {
        public SelectVersion()
        {
            InitializeComponent();
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
            if (radioButton1.Checked)
            {
                Program.VerInstall = 0;
            }
            else if (radioButton2.Checked)
            {
                Program.VerInstall = 1;
            }
            else
            {
                Program.VerInstall = 2;
            }
            this.Close();
        }
    }
}
