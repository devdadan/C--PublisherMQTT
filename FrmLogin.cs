using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Diagnostics;
using BCrypt.Net;
namespace REG2Publisher
{
    public partial class FrmLogin : Form
    {
        REG2Class Fungsi = new REG2Class();
        string ip;
        public FrmLogin()
        {
            InitializeComponent();
        }

        private void Loader_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            textBox2.KeyDown += textBox2_KeyDown;

            try
            {
                string connectionString = Fungsi.connectionString;
                MySqlConnection connection = new MySqlConnection(connectionString);
                connection.Open();
                
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        private void getip()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            StringBuilder ipAddresses = new StringBuilder();
            foreach (IPAddress ipAddress in localIPs)
            {
                if (ipAddress.ToString().StartsWith("192.168"))
                {
                    ipAddresses.AppendLine(ipAddress.ToString());
                }
            }
            ip = ipAddresses.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text =="" && textBox2.Text =="")
            {
                MessageBox.Show("Harap input user dan password");
            }
            else
            {
                string connectionString = Fungsi.connectionString;
                MySqlConnection connection = new MySqlConnection(connectionString);

                try
                {
                    connection.Open();

                    if (Login(textBox1.Text, textBox2.Text))
                    {
                        string q2a = "SELECT COUNT(*) FROM BROKERINFO WHERE ISAKTIF='*'";
                        MySqlCommand cmd2a = new MySqlCommand(q2a, connection);
                        int count1 = Convert.ToInt32(cmd2a.ExecuteScalar());

                        if (count1 >= 1)
                        {
                            string q3 = "SELECT IP FROM BROKERINFO WHERE ISAKTIF='*'";
                            MySqlCommand cmd2 = new MySqlCommand(q3, connection);
                            Fungsi.Server = cmd2.ExecuteScalar().ToString();

                        }
                        else
                        {
                            MessageBox.Show("BELUM ADA SERVER BROKER AKTIF!", "PERINGATAN", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            Fungsi.Log("Cekserverbroker", "BELUM ADA SERVER BROKER");
                            this.Close();
                        }
                        if (Fungsi.CheckServer(Fungsi.Server))
                        {

                            this.Hide();

                            FrmREGPubLink v = new FrmREGPubLink();
                            Fungsi.NikLogin = textBox1.Text;
                            v.NikLogin = textBox1.Text;
                            v.Show();
                        }
                        else
                        {
                            MessageBox.Show("SERVER BROKER IS TURN OFF, TURN ON PLEASE!", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            this.Close();

                        }

                    }
                    else
                    {
                        MessageBox.Show("User atau Password salah!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Login :" + ex.Message);
                    this.Close();
                }
            }
            
        }

        private bool Login(string username, string password)
        {
            string connectionString = Fungsi.connectionString;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT password FROM table_user WHERE username = @Username ";

                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Username", username);

                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    string hashPassword = result.ToString();

                    // Verifikasi bcrypt Laravel
                    return BCrypt.Net.BCrypt.Verify(password, hashPassword);
                }
            }

            return false;
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            
        }
        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.KeyCode == Keys.Enter)
            {
                button1.Focus();
                //e.SuppressKeyPress = true;
                //button1.PerformClick();
            }
        }
    }
}
