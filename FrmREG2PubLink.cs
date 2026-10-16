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
using System.Diagnostics;
namespace REG2Publisher
{
    public partial class FrmREGPubLink : Form
    {
        
        MqttClient mqttClient;
        public string clientid;
        public string cabang;
        public string ipEDP;
        public string inisaha;
        public string inisaha2;
        
        public string ipcab;
        public string usercab;
        public string passcab;
        string iprdp;
        string urdp;
        string prdp;
        REG2Class Fungsi = new REG2Class();
        public string NikLogin;
        public string c_id;
        public string ServerBroker;
        private List<string> commandHistory = new List<string>();
        public FrmREGPubLink()
        {
            InitializeComponent();
            textBox1.KeyDown += textBox1_KeyDown;
           
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            listBox1.Visible = false;
            ServerBroker = Fungsi.Server;
            lbl_rdp.Visible = false;
            txt_rdp.Visible = false;
            BTN_RECONNECT.Visible = false;
            textBox1.Enabled = false;
            c_id = NikLogin + "BC";

            lblinit.Text = $"{Application.ProductName} v{Application.ProductVersion}";
            getip();
            getDatacabang();
            lbl_client.Text = "";

            FrmDTO childForm = new FrmDTO();
            childForm.TopLevel = false;
            tabPage2.Controls.Add(childForm);
            childForm.NikLogin = NikLogin;
            childForm.Show();

            lbltampung.Text = "";
            button5.Enabled = false;
            BTN_RECONNECT.Enabled = false;
            cb_capture.Checked = false;
        }


        private void cb_client_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            PublishMqtt(TopicCommandSS(clientid), "SS");
        }


        private void getip()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ipAddress in localIPs)
            {

                ipEDP = ipAddress.ToString();
            }
        }


        private void REG2Commander_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mqttClient != null && mqttClient.IsConnected)
            {
                mqttClient.Disconnect();
            }
        }
        // Tambahkan sebagai field kelas
        private Dictionary<string, (string server, string user, string pass)> dataCabang
            = new Dictionary<string, (string, string, string)>();

        private void getDatacabang()
        {
            string connectionString = Fungsi.connectionString;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    cb_client.Items.Clear();
                    dataCabang.Clear();
                    connection.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "SELECT KDCAB, NAMA, SERVER, RDP_USER, RDP_PASS FROM master_cabang " +
                        "where recid='*' and kdcab not in('G218','G219','G257','G259') order by kdcab",
                        connection);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string kdcab = reader.GetString("KDCAB");
                            string namacab = reader.GetString("NAMA");
                            string server = reader.GetString("SERVER");
                            string user = reader.GetString("RDP_USER");
                            string pass = reader.GetString("RDP_PASS");

                            dataCabang[kdcab] = (server, user, pass); // disimpan per kode cabang
                            cb_client.Items.Add(kdcab + "-" + namacab);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Fungsi.Log("getDatacabang", ex.Message);
                    MessageBox.Show("getDatacabang error : " + ex.Message);
                    this.Close();
                }
            } // connection otomatis Dispose & Close di sini
        }
        private string currentBcTopic = "";
        private string currentSsTopic = "";
        private void konekbroker(string sclinet)
        {
            Task.Run(() =>
            {
                try
                {
                    string bcTopic = TopicResponsBC(sclinet);
                    string screenshotTopic = TopicResponsSS(sclinet);

                    if (mqttClient == null || !mqttClient.IsConnected)
                    {
                        if (mqttClient != null && mqttClient.IsConnected)
                        {
                            mqttClient.Disconnect();
                        }

                        this.Invoke((MethodInvoker)delegate
                        {
                            mqttClient = new MqttClient(ServerBroker);
                            mqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;

                            try
                            {
                                mqttClient.Connect(c_id);

                                if (cb_capture.Checked)
                                {
                                    mqttClient.Subscribe(
                                        new[] { bcTopic, screenshotTopic },
                                        new[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                                }
                                else
                                {
                                    mqttClient.Subscribe(
                                        new[] { bcTopic },
                                        new[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                                }

                                currentBcTopic = bcTopic;
                                currentSsTopic = screenshotTopic;
                                inisaha = bcTopic;
                            }
                            catch (Exception ex)
                            {
                                Fungsi.Log("konekbroker", "Error while connecting or subscribing: " + ex.Message);
                                MessageBox.Show($"Error while connecting or subscribing: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        });
                    }
                    else
                    {
                        if (bcTopic != currentBcTopic)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                txt_respons.Clear();
                            });

                            try
                            {
                                var oldTopics = new List<string>();
                                if (!string.IsNullOrEmpty(currentBcTopic)) oldTopics.Add(currentBcTopic);
                                if (cb_capture.Checked && !string.IsNullOrEmpty(currentSsTopic)) oldTopics.Add(currentSsTopic);

                                if (oldTopics.Count > 0)
                                {
                                    mqttClient.Unsubscribe(oldTopics.ToArray());
                                }
                            }
                            catch (Exception ex)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    Fungsi.Log("konekbroker", "Error unsubscribing from MQTT topics: " + ex.Message);
                                    MessageBox.Show($"Error unsubscribing from MQTT topics: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                });
                            }
                        }

                        try
                        {
                            if (cb_capture.Checked)
                            {
                                mqttClient.Subscribe(
                                    new[] { bcTopic, screenshotTopic },
                                    new[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                            }
                            else
                            {
                                mqttClient.Subscribe(
                                    new[] { bcTopic },
                                    new[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                            }

                            currentBcTopic = bcTopic;
                            currentSsTopic = screenshotTopic;
                            inisaha = bcTopic;
                        }
                        catch (Exception ex)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                Fungsi.Log("konekbroker", "Error subscribing to MQTT topics: " + ex.Message);
                                MessageBox.Show($"Error subscribing to MQTT topics: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show($"Error connecting to MQTT broker: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
            });
        }

        private void MqttClient_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            try
            {
                var topic = e.Topic;
                var message = Encoding.UTF8.GetString(e.Message);

                if (topic == currentBcTopic)
                {
                    txt_respons.Invoke((MethodInvoker)(() =>
                    {
                        txt_respons.Text += Environment.NewLine + message;
                        txt_respons.SelectionStart = txt_respons.Text.Length;
                        txt_respons.ScrollToCaret();
                    }));
                }
                else if (topic == currentSsTopic)
                {
                    byte[] data = e.Message;
                    using (MemoryStream stream = new MemoryStream(data))
                    {
                        Bitmap screenshot = new Bitmap(stream);

                        pictureBox1.Invoke(new Action(() =>
                        {
                            if (pictureBox1.Image != null)
                                pictureBox1.Image.Dispose();

                            pictureBox1.Image = (Bitmap)screenshot.Clone();
                        }));

                        screenshot.Dispose();
                        Console.WriteLine("Tangkapan layar berhasil ditampilkan.");
                    }
                }
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    Fungsi.Log("MqttClient", "Error processing received MQTT message:" + ex.Message);
                    MessageBox.Show($"Error processing received MQTT message: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private string TopicResponsBC(string client) => $"RESPONS_{client}/BC/{NikLogin}";
        private string TopicResponsSS(string client) => $"RESPONS_{client}/SS/{NikLogin}";
        private string TopicCommandBC(string client) => $"COMMAND_{client}/BC/{NikLogin}";
        private string TopicCommandSS(string client) => $"COMMAND_{client}/SS/{NikLogin}";

        private void PublishMqtt(string topic, string payload)
        {
            if (mqttClient == null || !mqttClient.IsConnected) return;

            Task.Run(() =>
            {
                try
                {
                    if (!this.IsDisposed && !this.Disposing)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            mqttClient.Publish(topic, Encoding.UTF8.GetBytes(payload));
                        });
                    }
                }
                catch (Exception ex)
                {
                    Fungsi.Log("PublishMqtt", ex.Message);
                    if (!this.IsDisposed)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            MessageBox.Show($"Error publishing MQTT message: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        });
                    }
                }
            });
        }
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F9)
            {
                try
                {
                    Task.Run(() =>
                    {
                        if (mqttClient != null && mqttClient.IsConnected)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                if (string.Equals(textBox1.Text, "CLS", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(textBox1.Text, "CLEAR", StringComparison.OrdinalIgnoreCase))
                                {
                                    textBox1.Clear();
                                }
                                else
                                {
                                    string commandTopic = TopicCommandBC(clientid);
                                    mqttClient.Publish(commandTopic, Encoding.UTF8.GetBytes(textBox1.Text));

                                    DateTime currentTime = DateTime.Now;
                                    txt_respons.Text += Environment.NewLine + clientid + "@ " + currentTime;
                                    txt_respons.Text += Environment.NewLine + textBox1.Text;

                                    commandHistory.Add(textBox1.Text);
                                    textBox1.Clear();

                                    if (cb_capture.Checked)
                                    {
                                        string commandTopic2 = TopicCommandSS(clientid);
                                        mqttClient.Publish(commandTopic2, Encoding.UTF8.GetBytes("SS"));
                                    }
                                }
                            });
                        }
                    });
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show($"Error connecting to MQTT broker: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
            }

            if (e.KeyCode == Keys.F7)
            {
                listBox1.Items.Clear();
                if (commandHistory.Count > 0)
                {
                    foreach (var command in commandHistory)
                    {
                        listBox1.Items.Add(command);
                    }

                    listBox1.Visible = true;
                    listBox1.Focus();
                }
                else
                {
                    MessageBox.Show("No command history available.", "History", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void txt_respons_TextChanged(object sender, EventArgs e)
        {

        }

        private void cb_client_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            try
            {
                lbltampung.Text = "";
                button5.Enabled = false;
                textBox1.Enabled = false;
                BTN_RECONNECT.Visible = false;

                cabang = cb_client.Text.Substring(0, Math.Min(4, cb_client.Text.Length));
                clientid = "TAMPUNG_" + cabang;

                iprdp = "";
                urdp = "";
                prdp = "";

                if (dataCabang.TryGetValue(cabang, out var info))
                {
                    ipcab = info.server;
                    usercab = info.user;
                    passcab = info.pass;
                }

                lbl_load.Text = "Ping ke :" + ipcab;

                var result = Fungsi.cekrdp(cabang);
                if (result != null && result.Contains("|"))
                {
                    var parts = result.Split(new char[] { '|' });
                    iprdp = parts[0];
                    urdp = parts[1];
                    prdp = parts[2];
                }

                if (Fungsi.CheckPing(iprdp))
                {
                    lbl_load.Text = "Ping sukses : " + ipcab;
                    lbl_load.Text = "MENUNGGU RESPON : " + clientid;

                    FrmCek cek = new FrmCek();
                    Fungsi.Nametmp = clientid;
                    cek.ShowDialog();

                    if (cek.hasil)
                    {
                        konekbroker(clientid);
                        textBox1.Enabled = true;
                        button5.Enabled = true;
                        lbltampung.Text = cb_client.Text;
                        lbl_load.Text = "TERKONEKSI KE : " + clientid;
                    }
                    else
                    {
                        lbl_client.Text = "";
                        lbl_load.Text = "LISTENERS OFFLINE : " + clientid;
                        textBox1.Enabled = false;
                        BTN_RECONNECT.Visible = true;
                        BTN_RECONNECT.Enabled = true;
                    }

                    lbl_rdp.Visible = false;
                    txt_rdp.Visible = false;
                }
                else
                {
                    lbl_load.Text = "Ping Gagal :" + ipcab;
                    textBox1.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void ListBox1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("OK!");
            if (listBox1.SelectedItem != null)
            {
                MessageBox.Show("OK");
                string selectedCommand = listBox1.SelectedItem.ToString();
                textBox1.Text = selectedCommand;
                listBox1.Visible = false;
            }
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (listBox1.SelectedItem != null)
            {
                string selectedCommand = listBox1.SelectedItem.ToString();
                textBox1.Text = selectedCommand;
                listBox1.Visible = false;
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.Arguments = "/c taskkill /f /im reg2*";
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.Start();

            string output = cmd.StandardOutput.ReadToEnd();
            cmd.WaitForExit();

            Console.WriteLine("Output:");
            Console.WriteLine(output);

            if (mqttClient != null && mqttClient.IsConnected)
            {
                mqttClient.Disconnect();
            }
            mqttClient = null;

            List<Form> openForms = new List<Form>(Application.OpenForms.Cast<Form>());
            foreach (Form frm in openForms)
            {
                frm.Close();
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!(cb_client.Text == ""))
            {
                FrmCapture ss = new FrmCapture();
                ss.NikLogin = NikLogin;
                ss.clientid = clientid;

                ss.ShowDialog();
            }
            
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (cb_client.Text == "") return;

            string lok = Path.Combine(Application.StartupPath, "set.rdp");
            if (File.Exists(lok))
            {
                File.Delete(lok);
            }
            Fungsi.CreateRdpFile(lok, iprdp, urdp, prdp);

            Process.Start(lok);

            lbl_rdp.Visible = true;
            txt_rdp.Visible = true;
            txt_rdp.UseSystemPasswordChar = true;
            txt_rdp.Text = prdp;
        }

        private void lbl_rdp_Click(object sender, EventArgs e)
        {

        }

        private void BTN_RECONNECT_Click(object sender, EventArgs e)
        {
            FrmCek cek = new FrmCek();
            Fungsi.Nametmp = clientid;
            cek.ShowDialog();

            if (cek.hasil)
            {
                konekbroker(clientid);
                textBox1.Enabled = true;
                lbl_load.Text = "TERKONEKSI KE : " + clientid;
                BTN_RECONNECT.Visible = false;
            }
            else
            {
                lbl_client.Text = "";
                lbl_load.Text = "LISTENERS OFFLINE : " + clientid;
                textBox1.Enabled = false;
                BTN_RECONNECT.Visible = true;
            }
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

     

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            try
            {
                string command = @"for %%D in (C D E) do (
        if exist ""%%D:\REGUPD\REGUPD.exe"" (
            ""%%D:\REGUPD\REGUPD.exe"" -auto
            echo Ditemukan di %%D: dan dijalankan.
            goto :EOF
        ) else (
            echo Tidak ditemukan di %%D:
        )
    )";

                PublishMqtt(TopicCommandBC(clientid), command);
            }
            catch (Exception ex)
            {
                Fungsi.Log("RUN REGUPD", ex.Message);
            }
        }

        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                string command = $"powershell -Command \"$tomorrow=(Get-Date).AddDays(1).ToString('dd'); $found=$false; foreach($d in Get-PSDrive -PSProvider FileSystem){{ $path=Join-Path ($d.Name+':\\') ('DTO\\{cabang}\\'+$tomorrow); if(Test-Path $path){{ Write-Host ('Folder ditemukan di drive '+$d.Name+':'); cmd /c dir $path; $found=$true; break }} }} if(-not $found){{ Write-Host 'Folder tidak ditemukan di semua drive.' }}\"";

                Fungsi.Log("DEBUG_COMMAND", command); // <-- tambahkan log ini sementara
                PublishMqtt(TopicCommandBC(clientid), command);
            }
            catch (Exception ex)
            {
                Fungsi.Log("RUN REGUPD", ex.Message);
            }
        }
        private void tabPage3_Click(object sender, EventArgs e)
        {

        }
    }
}
