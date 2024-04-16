using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using MySql.Data.MySqlClient;
using System.Net;
using System.Runtime.InteropServices;
namespace REG2Publisher
{
    public partial class FrmDTO : Form
    {
        MqttClient mqttClient;
        public string NikLogin;
        string script = "";
        public string ipcab;
        public string usercab;
        public string passcab;
        public string clientid;
        public string inisaha2;
        public string inisaha;
        public string c_id;
        public string ServerBroker;
        REG2Class Fungsi = new REG2Class();
        public FrmDTO()
        {
            InitializeComponent();
        }
        private void MqttClient_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            try
            {
                var topic = e.Topic;
                var message = Encoding.UTF8.GetString(e.Message);
                if (topic == "RESPONS_" + Fungsi.Nametmp + "/DTO/" + NikLogin)
                {
                   txt_respons2.Text += Environment.NewLine + message;
                   txt_respons2.SelectionStart = txt_respons2.Text.Length;
                   txt_respons2.ScrollToCaret();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to MQTT broker: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void getDatacabang()
        {
            string connectionString = Fungsi.connectionString;
            MySqlConnection connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM m_cabang where recid='*' and kdcab not in('G219') order by kdcab", connection);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string kdcab = reader.GetString("KDCAB");
                        string namacab = reader.GetString("NAMA");
                        ipcab = reader.GetString("SERVER");
                        usercab = reader.GetString("RDP_USER");
                        passcab = reader.GetString("RDP_PASS");
                        ck_cabang.Items.Add(kdcab + " - " + namacab, CheckState.Unchecked);
                        ck_cabang.Tag = kdcab;
                    }
                }

            }
            catch (Exception ex)
            {
                Fungsi.Log("getDatacabang", ex.Message);
                Console.WriteLine("Error: " + ex.Message);
                MessageBox.Show("getDatacabang error : " + ex.Message);
                this.Close();
                throw;
            }
            finally
            {
                connection.Close();
            }
        }
        private void GetListZip()
        {
            string connectionString = Fungsi.connectionString;
            MySqlConnection connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT nama_file FROM m_file WHERE recid='*' ORDER BY nama_file", connection);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string files = reader.GetString("nama_file");
                        ck_files.Items.Add(files, CheckState.Unchecked);
                        ck_files.Tag = files;
                    }
                }

            }
            catch (Exception ex)
            {
                Fungsi.Log("GetListZip", ex.Message);
                Console.WriteLine("Error: " + ex.Message);
                this.Close();
                throw;
            }
            finally
            {
                connection.Close();
            }
        }
        private void konekbroker2(string sclinet)
        {
            try
            {
                if (mqttClient == null || !mqttClient.IsConnected)
                {
                    if (mqttClient != null && mqttClient.IsConnected)
                    {
                        mqttClient.Disconnect();
                    }
                    string commandTopic = string.Empty;
                    commandTopic = "RESPONS_" + sclinet + "/DTO/" + NikLogin;
                    mqttClient = new MqttClient(ServerBroker);
                    mqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;
                    mqttClient.Subscribe(new string[] { commandTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                    mqttClient.Connect(c_id);
                    inisaha2 = commandTopic;

                }
                else
                {
                    if (mqttClient != null && mqttClient.IsConnected)
                    {
                        try
                        {
                            mqttClient.Unsubscribe(new string[] { "RESPONS_" + sclinet + "/DTO/" + NikLogin });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error Unsubscribe to MQTT topic: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    try
                    {
                        string newCommandTopic = $"RESPONS_{sclinet}/DTO/{NikLogin}";
                        mqttClient.Subscribe(new string[] { newCommandTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                        inisaha = newCommandTopic;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error Subscribe to MQTT topic: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to MQTT broker: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {
           

        }
        private void SendCOmmand(string tampung)
        {

            try
            {
                string perintah = "";
                if (tampung == "TAMPUNG_G001")
                {
                    perintah = "Taskkill /f /im ZipDTO.exe";
                    perintah += Environment.NewLine;
                    perintah += "start D:\\ZipDTO\\ZipDTO.exe";
                    perintah += Environment.NewLine;
                    perintah += "exit";

                }
                else
                {
                    perintah = "Taskkill /f /im ZipDTO.exe";
                    perintah += Environment.NewLine;
                    perintah += "start E:\\ZipDTO\\ZipDTO.exe";
                    perintah += Environment.NewLine;
                    perintah += "exit";
                }
                try
                {
                    konekbroker2(tampung);
                    if (mqttClient != null && mqttClient.IsConnected)
                    {
                        string commandTopic2 = "COMMAND_" + tampung + "/DTO/" + NikLogin;
                        mqttClient.Publish(commandTopic2, Encoding.UTF8.GetBytes(perintah));
                        DateTime currentTime = DateTime.Now;
                        txt_respons2.Text += Environment.NewLine + tampung + "@ " + currentTime;
                        txt_respons2.Text += Environment.NewLine + perintah;
                        txt_respons2.Text += Environment.NewLine;
                    }
                }
                catch (Exception ex1)
                {
                    MessageBox.Show($"Error Send to MQTT broker: {ex1.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex2)
            {

                MessageBox.Show(ex2.Message);
            }

        }
        private void KirimTugas()
        {
            button1.Text = "Loading";
            string listcab = "";
            StringBuilder result = new StringBuilder();
            foreach (var itemChecked in ck_cabang.CheckedItems)
            {
                string text = itemChecked.ToString();
                string tagValue = text.Substring(0, Math.Min(text.Length, 4));
                string formattedTag = $"'{tagValue}'";
                result.Append(formattedTag);
                result.Append(",");
            }
            if (result.Length > 0)
                result.Remove(result.Length - 1, 1);

            listcab = result.ToString();

            string listfiles = "";
            StringBuilder result2 = new StringBuilder();
            foreach (var itemChecked2 in ck_files.CheckedItems)
            {
                string text2 = itemChecked2.ToString();
                string formattedTag2 = $"{text2}";
                result2.Append(formattedTag2);
                result2.Append("|");
            }
            if (result2.Length > 0)
                result2.Remove(result2.Length - 1, 1);

            listfiles = result2.ToString();
            string tgl = dateTimePicker1.Text;
            int m = ck_cabang.CheckedItems.Count;

            string connectionString = Fungsi.connectionString;
            MySqlConnection connection = new MySqlConnection(connectionString);
            try
            {
                bool ismdn = false;
                connection.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT KDCAB,`SERVER`,`USER`,PASS,DB FROM m_cabang WHERE RECID='*' AND KDCAB IN(" + listcab + ")", connection);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string serv = "";
                        string pass = "";
                        string user = "";
                        string db = "";
                        string cb = "";
                        string inittampung = "";
                        cb = reader.GetString("KDCAB");
                        serv = reader.GetString("SERVER");
                        user = reader.GetString("USER");
                        pass = reader.GetString("PASS");
                        db = reader.GetString("DB");
                        inittampung = "TAMPUNG_" + cb;
                        string connectiontmp = "";
                        connectiontmp = "server=" + serv + ";user=" + user + ";password=" + pass + ";database=" + db + ";";
                        MySqlConnection con2 = new MySqlConnection(connectiontmp);
                        if (Fungsi.CheckPing(serv))
                        {
                            if (!(cb == "G009") && !(cb == "G257") && !(cb == "G259") && !(cb == "G218"))
                            {
                                FrmCek cek = new FrmCek();
                                Fungsi.Nametmp = inittampung;
                                cek.ShowDialog();
                                if (cek.hasil)
                                {
                                    try
                                    {
                                        con2.Open();
                                        MySqlCommand cmd1 = new MySqlCommand("CREATE TABLE if not exists `command` (`id` INT(11) NOT NULL AUTO_INCREMENT,`jenis_command` VARCHAR(20) DEFAULT '',`Tanggal` VARCHAR(20) DEFAULT NULL,`isi_command` TEXT,`stat_command` CHAR(1) DEFAULT '*',PRIMARY KEY (`id`)) ENGINE=INNODB AUTO_INCREMENT=3 DEFAULT CHARSET=latin1", con2);
                                        cmd1.ExecuteNonQuery();

                                        MySqlCommand cmd2 = new MySqlCommand("UPDATE COMMAND SET STAT_COMMAND='1' WHERE STAT_COMMAND='*'", con2);
                                        cmd2.ExecuteNonQuery();

                                        MySqlCommand cmd3 = new MySqlCommand("INSERT INTO COMMAND (JENIS_COMMAND, TANGGAL, ISI_COMMAND) VALUES (@JenisCommand, @Tanggal, @IsiCommand)", con2);
                                        cmd3.Parameters.AddWithValue("@JenisCommand", "A");
                                        cmd3.Parameters.AddWithValue("@Tanggal", tgl);
                                        cmd3.Parameters.AddWithValue("@IsiCommand", listfiles);
                                        cmd3.ExecuteNonQuery();

                                        SendCOmmand(inittampung);

                                        button1.Text = "Proses ke : " + cb + " - " + serv;
                                    }
                                    catch (Exception ex1)
                                    {
                                        Fungsi.Log("KirimTUgas", ex1.Message);
                                        MessageBox.Show("KirimTUgas", ex1.Message);

                                    }
                                }
                                else
                                {
                                    txt_respons2.Text += Environment.NewLine;
                                    txt_respons2.Text += "GAGAL KIRIM KE " + inittampung + " KARENA LISTENERS OFFLINE";
                                }
                            }
                            else
                            {
                                if (!(ismdn))
                                {
                                    FrmCek cek = new FrmCek();
                                    Fungsi.Nametmp = inittampung;
                                    cek.ShowDialog();
                                    if (cek.hasil)
                                    {
                                        Fungsi.Log("Cek looping mdn ", cb);
                                        MySqlCommand cmda = new MySqlCommand("UPDATE COMMAND SET STAT_COMMAND='1' WHERE STAT_COMMAND='*'", con2);
                                        cmda.ExecuteNonQuery();

                                        MySqlCommand cmd3a = new MySqlCommand("INSERT INTO COMMAND (JENIS_COMMAND, TANGGAL, ISI_COMMAND,CAB) VALUES (@JenisCommand, @Tanggal, @IsiCommand,@IsiCab)", con2);
                                        cmd3a.Parameters.AddWithValue("@JenisCommand", "A");
                                        cmd3a.Parameters.AddWithValue("@Tanggal", tgl);
                                        cmd3a.Parameters.AddWithValue("@IsiCommand", listfiles);
                                        cmd3a.Parameters.AddWithValue("@IsiCab", cb);
                                        cmd3a.ExecuteNonQuery();

                                        SendCOmmand("TAMPUNG_G009");
                                        ismdn = true;
                                    }
                                    else
                                    {
                                        txt_respons2.Text += Environment.NewLine;
                                        txt_respons2.Text += "GAGAL KIRIM KE " + inittampung + " KARENA LISTENERS OFFLINE";
                                    }
                                }
                                
                                
                            }

                        }
                        else
                        {
                            Fungsi.Log("KirimTUgas", "Tidak terkoneksi ke " + serv);
                        }
                
                    }
                     
                    }
                   
            }
            catch (Exception ex)
            {

                Fungsi.Log("KirimTugas", ex.Message);
                MessageBox.Show("Error: " + ex.Message);
                this.Close();
            }
            finally
            {
                button1.Text = "SEND COMMAND ZIP";
                connection.Close();
            }
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
           

        }

        private void Download()
        {
            try
            {
                StringBuilder res = new StringBuilder();
                string parcab;
                string parfile;
                string prd;
                foreach (var i in ck_cabang.CheckedItems)
                {
                    string a = i.ToString();
                    string b = $"{a}";
                    res.Append(b);
                    res.Append(",");
                }
                if (res.Length > 0)
                    res.Remove(res.Length - 1, 1);

                parcab = res.ToString();

                StringBuilder res2 = new StringBuilder();
                foreach (var i2 in ck_files.CheckedItems)
                {
                    string a2 = i2.ToString();
                    string b2 = $"{a2}";
                    res2.Append(b2);
                    res2.Append(",");
                }
                if (res2.Length > 0)
                    res2.Remove(res2.Length - 1, 1);

                parfile = res2.ToString();
                prd = dateTimePicker1.Text;
                string pesan = "Akan download dengan data berikut ?\n\n" +
               "Period : " +prd+" \n"+
               "Cabang : "+parcab+" \n" +
               "Files : "+parfile+" \n" ;
                DialogResult result = MessageBox.Show(pesan, "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    string listfiles = "";
                    bool ismdn2 = false;
                    DateTime tanggal = dateTimePicker1.Value;
                    string yymmdd = tanggal.ToString("yyMMdd");
                    foreach (var itemChecked in ck_cabang.CheckedItems)
                    {
                        string text = itemChecked.ToString();
                        string tagValue = text.Substring(0, Math.Min(text.Length, 4));
                        StringBuilder result2 = new StringBuilder();
                        foreach (var itemChecked2 in ck_files.CheckedItems)
                        {
                            string text2 = itemChecked2.ToString();
                            string formattedTag2 = $"{text2}";
                            result2.Append(formattedTag2);
                            result2.Append(" ");
                        }
                        if (result2.Length > 0)
                            result2.Remove(result2.Length - 1, 1);

                        listfiles = result2.ToString();
                        if (tagValue == "G009" || tagValue == "G218" || tagValue == "G257" || tagValue == "G259")
                        {
                            ismdn2 = true;
                        }
                        else
                        {
                            FrmCek ah = new FrmCek();
                            Fungsi.Nametmp = "TAMPUNG_" + tagValue;
                            ah.ShowDialog();
                            if (ah.hasil)
                            {
                                konekbroker2("TAMPUNG_" + tagValue);
                                script = "dwn -download " + yymmdd + " " + listfiles;
                                script += Environment.NewLine;
                                script += "exit";
                                if (mqttClient != null && mqttClient.IsConnected)
                                {
                                    string commandTopic2 = "COMMAND_" + "TAMPUNG_" + tagValue + "/DTO/" + NikLogin;
                                    mqttClient.Publish(commandTopic2, Encoding.UTF8.GetBytes(script));
                                    DateTime currentTime = DateTime.Now;
                                    txt_respons2.Text += Environment.NewLine + "TAMPUNG_" + tagValue + "@ " + currentTime;
                                    txt_respons2.Text += Environment.NewLine + script;
                                    txt_respons2.Text += Environment.NewLine;
                                }
                            }
                            else
                            {
                                txt_respons2.Text += Environment.NewLine;
                                txt_respons2.Text += "DOWNLOAD FILE GAGAL KARENA LISTENER OFFLINE : " + "TAMPUNG_" + tagValue;

                            }

                        }


                    }
                    if (ismdn2)
                    {
                        FrmCek ah2 = new FrmCek();
                        Fungsi.Nametmp = "TAMPUNG_G009";
                        ah2.ShowDialog();
                        if (ah2.hasil)
                        {
                            konekbroker2("TAMPUNG_G009");
                            script = "dwn -download " + yymmdd + " " + listfiles;
                            script += Environment.NewLine;
                            script += "exit";
                            if (mqttClient != null && mqttClient.IsConnected)
                            {
                                string commandTopic2 = "COMMAND_" + "TAMPUNG_G009/DTO/" + NikLogin;
                                mqttClient.Publish(commandTopic2, Encoding.UTF8.GetBytes(script));
                                DateTime currentTime = DateTime.Now;
                                txt_respons2.Text += Environment.NewLine + "TAMPUNG_G009@ " + currentTime;
                                txt_respons2.Text += Environment.NewLine + script;
                                txt_respons2.Text += Environment.NewLine;
                            }
                        }
                        else
                        {
                            txt_respons2.Text += Environment.NewLine;
                            txt_respons2.Text += "DOWNLOAD FILE GAGAL KARENA LISTENER OFFLINE : " + "TAMPUNG_G009";

                        }

                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Fungsi.Log("Download", ex.Message);
            }

            btnDown.Text = "Selesai";
            btnDown.Text = "DOWNLOAD FILES";

        }


        private void ck_all1_CheckedChanged(object sender, EventArgs e)
        {
           


        }


        private void ck_download_CheckedChanged(object sender, EventArgs e)
        {
            
        }
        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void FrmDTO_Load(object sender, EventArgs e)
        {
            ServerBroker = Fungsi.Server;
            c_id= NikLogin + "DTO";
            getDatacabang();
            GetListZip();
        }

        private void check2_CheckedChanged(object sender, EventArgs e)
        {
            bool checked2 = check2.Checked;
            if (checked2)
            {
                for (int i = 0; i < ck_files.Items.Count; i++)
                {
                    ck_files.SetItemChecked(i, true);
                }
            }
            else
            {
                for (int i = 0; i < ck_files.Items.Count; i++)
                {
                    ck_files.SetItemChecked(i, false);
                }
            }
        }

        private void check1_CheckedChanged(object sender, EventArgs e)
        {
            bool checked1 = check1.Checked;
            if (checked1)
            {
                for (int i = 0; i < ck_cabang.Items.Count; i++)
                {
                    ck_cabang.SetItemChecked(i, true);
                }
            }
            else
            {
                for (int i = 0; i < ck_cabang.Items.Count; i++)
                {
                    ck_cabang.SetItemChecked(i, false);
                }
            }
        }

        private void checkd_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            if (ck_cabang.CheckedItems.Count == 0 || ck_files.CheckedItems.Count == 0)
            {
                MessageBox.Show("Harap pilih list cabang atau files");
            }
            else
            {
                disable();
                Download();
                enable();
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (ck_cabang.CheckedItems.Count == 0 || ck_files.CheckedItems.Count == 0)
            {
                MessageBox.Show("Harap pilih list cabang atau files");
            }
            else
            {
                StringBuilder res = new StringBuilder();
                string parcab;
                string parfile;
                string prd;
                foreach (var i in ck_cabang.CheckedItems)
                {
                    string a = i.ToString();
                    string b = $"{a}";
                    res.Append(b);
                    res.Append(",");
                }
                if (res.Length > 0)
                    res.Remove(res.Length - 1, 1);

                parcab = res.ToString();

                StringBuilder res2 = new StringBuilder();
                foreach (var i2 in ck_files.CheckedItems)
                {
                    string a2 = i2.ToString();
                    string b2 = $"{a2}";
                    res2.Append(b2);
                    res2.Append(",");
                }
                if (res2.Length > 0)
                    res2.Remove(res2.Length - 1, 1);

                parfile = res2.ToString();
                prd = dateTimePicker1.Text;
                string pesan = "Akan ZIP dengan data berikut ?\n\n" +
               "Period : " + prd + " \n" +
               "Cabang : " + parcab + " \n" +
               "Files : " + parfile + " \n";
                DialogResult result = MessageBox.Show(pesan, "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    disable();
                    backgroundWorker1.RunWorkerAsync();
                    enable();
                }
 
                   
            }
        }

        private void disable()
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            btnDown.Enabled = false;
            groupBox2.Enabled = false;
            

        }
        private void enable()
        {
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            btnDown.Enabled = true;
            groupBox2.Enabled = true;
            

        }

        private void backgroundWorker1_DoWork_1(object sender, DoWorkEventArgs e)
        {
            KirimTugas();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (ck_cabang.CheckedItems.Count == 0)
            {
                MessageBox.Show("Harap pilih list cabang");
            }
            else
            {
                StringBuilder res = new StringBuilder();
                string parcab;
                string prd;
                foreach (var i in ck_cabang.CheckedItems)
                {
                    string a = i.ToString();
                    string b = $"{a}";
                    res.Append(b);
                    res.Append(",");
                }
                if (res.Length > 0)
                    res.Remove(res.Length - 1, 1);

                parcab = res.ToString();

                prd = dateTimePicker1.Text;
                string pesan = "Akan unzip dengan data berikut ?\n\n" +
               "Period : " + prd + " \n" +
               "Cabang : " + parcab + " \n";
                DialogResult result = MessageBox.Show(pesan, "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    disable();
                    Unzipdt();
                    enable();
                }


            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (ck_cabang.CheckedItems.Count == 0)
            {
                MessageBox.Show("Harap pilih list cabang");
            }
            else
            {
                StringBuilder res = new StringBuilder();
                string parcab;
                string prd;
                foreach (var i in ck_cabang.CheckedItems)
                {
                    string a = i.ToString();
                    string b = $"{a}";
                    res.Append(b);
                    res.Append(",");
                }
                if (res.Length > 0)
                    res.Remove(res.Length - 1, 1);

                parcab = res.ToString();

                prd = dateTimePicker1.Text;
                string pesan = "Akan cek dengan data berikut ?\n\n" +
               "Period : " + prd + " \n" +
               "Cabang : " + parcab + " \n";
                DialogResult result = MessageBox.Show(pesan, "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    disable();
                    dir();
                    enable();
                }


            }

        }
        private void dir()
        {
            try
            {
                foreach (var itemChecked in ck_cabang.CheckedItems)
                {
                    string text = itemChecked.ToString();
                    string tagValue = text.Substring(0, Math.Min(text.Length, 4));
                    if (!(tagValue == "G009") && !(tagValue == "G257") && !(tagValue == "G259") && !(tagValue == "G218"))
                    {
                        FrmCek ah = new FrmCek();
                        Fungsi.Nametmp = "TAMPUNG_" + tagValue;
                        ah.ShowDialog();
                        if (ah.hasil)
                        {
                            konekbroker2("TAMPUNG_" + tagValue);
                            if (mqttClient != null && mqttClient.IsConnected)
                            {
                                string prd1 = dateTimePicker1.Text;
                                string[] tanggalSplit = prd1.Split('-');
                                string dd = tanggalSplit[0];
                                string script1;
                                if (tagValue == "G001")
                                {
                                    script1 = @"
                                    $path = 'D:\\DTO\\" + tagValue + "\\" + dd + @"\\'
                                    $file = Get-ChildItem -Path $path | Select-Object -First 1
                                    $totalFiles = (Get-ChildItem -Path $path | Measure-Object).Count
                                    if ($file -ne $null) {
                                        $shortName = $file.Name.Substring(0, [Math]::Min($file.Name.Length, 7))
                                        Write-Host ""NAMA ZIP : $shortName""
                                        Write-Host ""TOTAL ZIP : $totalFiles""
                                    } else {
                                        Write-Host ""TIDAK ADA ZIP""
                                    }
                                    ";
                                }
                                else
                                {
                                    script1 = @"
                                    $path = 'E:\\DTO\\" + tagValue + "\\" + dd + @"\\'
                                    $file = Get-ChildItem -Path $path | Select-Object -First 1
                                    $totalFiles = (Get-ChildItem -Path $path | Measure-Object).Count
                                    if ($file -ne $null) {
                                        $shortName = $file.Name.Substring(0, [Math]::Min($file.Name.Length, 7))
                                        Write-Host ""NAMA ZIP : $shortName""
                                        Write-Host ""TOTAL ZIP : $totalFiles""
                                    } else {
                                        Write-Host ""TIDAK ADA ZIP""
                                    }
                                    ";
                                    }

                                string commandTopic2 = "COMMAND_" + "TAMPUNG_" + tagValue + "/DTO/" + NikLogin;
                                mqttClient.Publish(commandTopic2, Encoding.UTF8.GetBytes(script1));
                                DateTime currentTime = DateTime.Now;
                                txt_respons2.Text += Environment.NewLine + NikLogin + "@ " + currentTime;
                                txt_respons2.Text += Environment.NewLine + "Cek ZIP " + tagValue;
                                txt_respons2.Text += Environment.NewLine;
                            }
                        }
                        else
                        {
                            txt_respons2.Text += Environment.NewLine;
                            txt_respons2.Text += "CEK FILE FILE GAGAL KARENA LISTENER OFFLINE : " + "TAMPUNG_" + tagValue;

                        }
                    }
                    else
                    {
                        FrmCek ah = new FrmCek();
                        Fungsi.Nametmp = "TAMPUNG_G009";
                        ah.ShowDialog();
                        if (ah.hasil)
                        {
                            konekbroker2("TAMPUNG_G009");
                            if (mqttClient != null && mqttClient.IsConnected)
                            {
                                string prd1 = dateTimePicker1.Text;
                                string[] tanggalSplit = prd1.Split('-');
                                string dd = tanggalSplit[0];
                                string script1;
                                script1 = @"
                                $path = 'E:\\DTO\\" + tagValue + "\\" + dd + @"\\'
                                $file = Get-ChildItem -Path $path | Select-Object -First 1
                                $totalFiles = (Get-ChildItem -Path $path | Measure-Object).Count
                                if ($file -ne $null) {
                                    $shortName = $file.Name.Substring(0, [Math]::Min($file.Name.Length, 7))
                                    Write-Host ""NAMA ZIP : $shortName""
                                    Write-Host ""TOTAL ZIP : $totalFiles""
                                } else {
                                    Write-Host ""TIDAK ADA ZIP""
                                }";

                                string commandTopic2 = "COMMAND_" + "TAMPUNG_G009/DTO/" + NikLogin;
                                mqttClient.Publish(commandTopic2, Encoding.UTF8.GetBytes(script1));
                                DateTime currentTime = DateTime.Now;
                                txt_respons2.Text += Environment.NewLine + NikLogin + "@ " + currentTime;
                                txt_respons2.Text += Environment.NewLine + "Cek ZIP " + tagValue;
                                txt_respons2.Text += Environment.NewLine;
                            }
                        }
                        else
                        {
                            txt_respons2.Text += Environment.NewLine;
                            txt_respons2.Text += "CEK FILE FILE GAGAL KARENA LISTENER OFFLINE : " + "TAMPUNG_" + tagValue;

                        }
                    }
                    System.Threading.Thread.Sleep(5000);
                }
                
               
            }
            catch (Exception ex)
            {
                Fungsi.Log("DIR", ex.Message);
                MessageBox.Show("DIR Error : " + ex.Message);
                this.Close();
            }
            


            }

        private void Unzipdt()
        {
            try
            {
                string bln;
                bln = dateTimePicker1.Text.Substring(3, 2);
                if (bln == "10")
                {
                    bln = "A";
                }
                else if (bln == "11")
                {
                    bln = "B";
                }
                else if (bln == "12")
                {
                    bln = "C";
                }
                else
                {
                    bln = dateTimePicker1.Text.Substring(4, 1);
                }

                string thn;
                thn = dateTimePicker1.Text.Substring(dateTimePicker1.Text.Length - 1);
                string tgl;
                tgl = dateTimePicker1.Text.Substring(0, 2);
                string namadto = "DTO" + thn + bln + tgl;
                foreach (var itemChecked in ck_cabang.CheckedItems)
                {
                    string text = itemChecked.ToString();
                    string tagValue = text.Substring(0, Math.Min(text.Length, 4));
                    if (!(tagValue == "G009") && !(tagValue == "G257") && !(tagValue == "G259") && !(tagValue == "G218"))
                    {
                        
                        FrmCek ah = new FrmCek();
                        Fungsi.Nametmp = "TAMPUNG_" + tagValue;
                        ah.ShowDialog();
                        if (ah.hasil)
                        {
                            konekbroker2("TAMPUNG_" + tagValue);
                            if (mqttClient != null && mqttClient.IsConnected)
                            {
                                string prd1 = dateTimePicker1.Text;
                                string[] tanggalSplit = prd1.Split('-');
                                string dd = tanggalSplit[0];
                                string script1;
                                if (tagValue == "G001")
                                {
                                    script1 = "unzip -v D:\\DTO\\G001\\"+dd+"\\"+namadto+"T.001";
                                }
                                else if(tagValue =="G137")
                                {
                                    script1 = "unzip -v E:\\DTO\\G137\\" + dd + "\\" + namadto + "T.004";
                                }
                                else if (tagValue == "G105")
                                {
                                    script1 = "unzip -v E:\\DTO\\G105\\" + dd + "\\" + namadto + "T.105";
                                }
                                else if (tagValue == "G080")
                                {
                                    script1 = "unzip -v E:\\DTO\\G080\\" + dd + "\\" + namadto + "T.42B";
                                }
                                else if (tagValue == "G027")
                                {
                                    script1 = "unzip -v E:\\DTO\\G027\\" + dd + "\\" + namadto + "T.41H";
                                }
                                else if (tagValue == "G116")
                                {
                                    script1 = "unzip -v E:\\DTO\\G116\\" + dd + "\\" + namadto + "T.DCN";
                                }
                                else
                                {
                                    script1 = "unzip -v E:\\DTO\\G049\\" + dd + "\\" + namadto + "T.4L6";
                                }

                                string commandTopic2 = "COMMAND_" + "TAMPUNG_" + tagValue + "/DTO/" + NikLogin;
                                mqttClient.Publish(commandTopic2, Encoding.UTF8.GetBytes(script1));
                                DateTime currentTime = DateTime.Now;
                                txt_respons2.Text += Environment.NewLine + NikLogin + "@ " + currentTime;
                                txt_respons2.Text += Environment.NewLine + "UNZIP -V DTO " + tagValue;
                                txt_respons2.Text += Environment.NewLine;
                            }
                        }
                        else
                        {
                            txt_respons2.Text += Environment.NewLine;
                            txt_respons2.Text += "VIEW UNZIP GAGAL KARENA LISTENER OFFLINE : " + "TAMPUNG_" + tagValue;

                        }
                    }
                    else
                    {
                        FrmCek ah = new FrmCek();
                        Fungsi.Nametmp = "TAMPUNG_G009";
                        ah.ShowDialog();
                        if (ah.hasil)
                        {
                            konekbroker2("TAMPUNG_G009");
                            if (mqttClient != null && mqttClient.IsConnected)
                            {
                                string prd1 = dateTimePicker1.Text;
                                string[] tanggalSplit = prd1.Split('-');
                                string dd = tanggalSplit[0];
                                string script1;

                                if (tagValue == "G009")
                                {
                                    script1 = "unzip -v E:\\DTO\\G009\\" + dd + "\\" + namadto + "T.3FG";
                                }
                                else if (tagValue == "G257")
                                {
                                    script1 = "unzip -v E:\\DTO\\G257\\" + dd + "\\" + namadto + "F.0JB";
                                }
                                else if (tagValue == "G259")
                                {
                                    script1 = "unzip -v E:\\DTO\\G259\\" + dd + "\\" + namadto + "F.0KQ";
                                }
                                else
                                {
                                    script1 = "unzip -v E:\\DTO\\G218\\" + dd + "\\" + namadto + "T.2M5";
                                }

                                string commandTopic2 = "COMMAND_" + "TAMPUNG_G009/DTO/" + NikLogin;
                                mqttClient.Publish(commandTopic2, Encoding.UTF8.GetBytes(script1));
                                DateTime currentTime = DateTime.Now;
                                txt_respons2.Text += Environment.NewLine + NikLogin + "@ " + currentTime;
                                txt_respons2.Text += Environment.NewLine + "Cek ZIP " + tagValue;
                                txt_respons2.Text += Environment.NewLine;
                            }
                        }
                        else
                        {
                            txt_respons2.Text += Environment.NewLine;
                            txt_respons2.Text += "UNZIP FILE GAGAL KARENA LISTENER OFFLINE : " + "TAMPUNG_" + tagValue;

                        }
                    }
                    
                    System.Threading.Thread.Sleep(5000);
                }


            }
            catch (Exception ex)
            {
                Fungsi.Log("DIR", ex.Message);
                MessageBox.Show("DIR Error : " + ex.Message);
                this.Close();
            }



        }

        private void txt_respons2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
