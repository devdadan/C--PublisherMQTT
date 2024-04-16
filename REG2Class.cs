using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
namespace REG2Publisher
{
    class REG2Class
    {
 
        private static string _server;
        private static string _nametmp;
        private static string _niklogin;
        public string connectionString = "server=192.168.190.100;user=root;password=15032012;database=publink;";
        public string Server
        {
            set { _server = value; }
            get { return _server; }
        }
        public string Nametmp
        {
            set { _nametmp = value; }
            get { return _nametmp; }
        }
        public string NikLogin
        {
            set { _niklogin = value; }
            get { return _niklogin; }
        }

        public bool CheckPing(string ip)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(ip);
                    if (reply.Status == IPStatus.Success)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (PingException)
            {
                return false;
            }
        }

        public string cekrdp(string cab)
        {
            try
            {
                string query = "SELECT CONCAT(SERVER,'|',RDP_USER,'|',RDP_PASS) AS RDP FROM m_cabang WHERE KDCAB = @kdcab";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@kdcab", cab);
                        connection.Open();
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            return result.ToString();
                        }
                        else
                        {

                            return "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Get RDP :", ex.Message);
                return "";
            }
        }
        public void CreateRdpFile(string fileName, string ipAddress, string username, string password)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.WriteLine("full address:s:" + ipAddress);
                sw.WriteLine("username:s:" + username);
                sw.WriteLine("password 51 b:s:" + password);
                sw.WriteLine("domain:s:");
                sw.WriteLine("screen mode id:i:2");
                sw.WriteLine("use multimon:i:0");
                sw.WriteLine("desktopwidth:i:800");
                sw.WriteLine("desktopheight:i:600");
                sw.WriteLine("session bpp:i:32");
                sw.WriteLine("winposstr:s:0,3,0,0,800,600");
            }
        }
        public void Log(string proses, string Message)
        {
            try
            {
                string currentDate = DateTime.Now.ToString("ddMMyy");
                string fileName = $"log_{currentDate}.txt";

                string filePath = Path.Combine(Application.StartupPath, fileName);

                using (StreamWriter sw = File.AppendText(filePath))
                {
                    sw.WriteLine($"{DateTime.Now} - Proses: {proses}, Pesan: {Message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing to log file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

       

    }
   
}
