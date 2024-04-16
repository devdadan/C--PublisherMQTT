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
                    string query = "SELECT COUNT(*) FROM userlogin WHERE nik = @Username AND pass =password(@Password) and recid=''";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Username", textBox1.Text);
                    cmd.Parameters.AddWithValue("@Password", textBox2.Text);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    string ver = Application.ProductVersion;
                    string query2 = "UPDATE USERLOGIN SET VERSI='" + ver + "' WHERE NIK='" + textBox1.Text + "'";
                    MySqlCommand cmdd = new MySqlCommand(query2, connection);
                    cmdd.ExecuteNonQuery();

                    string versi1;
                    string versi2;
                    if (count > 0)
                    {
                        string SQL = "SELECT REPLACE(a.versi,'.','') versi1,REPLACE(b.versi,'.','') versi2 FROM userlogin a,const b WHERE a.nik='"+textBox1.Text+"' AND b.apps=2 ";
                        MySqlCommand cmd0 = new MySqlCommand(SQL, connection);
                        using (MySqlDataReader reader = cmd0.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                versi1 = reader.GetString("versi1");
                                versi2 = reader.GetString("versi2");
                                
                                int hasilPerbandingan = string.Compare(versi1, versi2);
                                if (hasilPerbandingan < 0)
                                {
                                    string pesan = "Apakah akan update versi ?\n\n" +
                                     "Versi anda : " + versi1 + " \n" +
                                     "Versi server : " + versi2 + " \n";
                                    DialogResult result = MessageBox.Show(pesan, "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                    if (result == DialogResult.Yes)
                                    {

                                        string pathfull = Application.StartupPath;
                                        string batchFilePath = Path.Combine(pathfull, "updgrade.bat");
                                        string batchfile = @"cd /d """ + pathfull + @"""
                                        taskkill /f /im reg2*
                                        del /f /q Reg2Publink.exe
                                        del /f /q Reg2Publink.pdb
                                        wget -P """ + pathfull + @""" --user=Backoff --password=123456 ftp://192.168.190.37:21/Reg2PubLink.zip -N
                                        unzip -o Reg2PubLink.zip
                                        Reg2PubLink.exe
                                        exit";

                                        if (File.Exists(batchFilePath))
                                        {
                                            File.Delete(batchFilePath);
                                        }
                                        File.WriteAllText(batchFilePath, batchfile);
                                        if (File.Exists(batchFilePath))
                                        {
                                            Process.Start(batchFilePath);
                                        }


                                    }
                                    else
                                    {
                                        this.Close();
                                    }

                                }
                                reader.Close();  
                            }

                            try
                            {
                                string q2 = "INSERT INTO HIS_LOGIN (USERNAME,START_LOGIN,IPADDRESS) values ('" + textBox1.Text + "',now(),'" + ip + "')";
                                MySqlCommand cmd1 = new MySqlCommand(q2, connection);
                                cmd1.ExecuteNonQuery();

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

                                this.Hide();

                                FrmREGPubLink v = new FrmREGPubLink();
                                Fungsi.NikLogin = textBox1.Text;
                                v.NikLogin = textBox1.Text;
                                v.Show();

                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("ERROR SAAT CEK SERVER! : " + ex.Message, "PERINGATAN");
                                Fungsi.Log("his_login", ex.Message);
                                this.Close();
                            }

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
    }
}
