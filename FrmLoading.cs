using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace REG2Publisher
{
    public partial class FrmLoading : Form
    {
        private REG2Class Fungsi = new REG2Class();
        public FrmLoading()
        {
            InitializeComponent();
            lblversion.Text = "App Version " + Application.ProductVersion;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.Value += 2;
            if (progressBar1.Value == 100)
            {
                if (!Fungsi.CheckPing("192.168.190.37"))
                {
                    MessageBox.Show("KONEKSI KE SERVER BROKER TERPUTUS!","CheckPing",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    Fungsi.Log("CheckPing", "Koneksi ke server broker terputus");
                    this.Close();
                }
                else
                {
                    FrmLogin v = new FrmLogin();
                    v.Show();
                    this.Hide();

                }
                timer1.Stop();
            }

        }

        private void FrmLoading_Load(object sender, EventArgs e)
        {

        }
    }
}
