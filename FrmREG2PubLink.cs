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
        public FrmREGPubLink()
        {
            InitializeComponent();
            textBox1.KeyDown += textBox1_KeyDown;
           
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            ServerBroker = Fungsi.Server;
            lbl_rdp.Visible = false;
            txt_rdp.Visible = false;
            BTN_RECONNECT.Visible = false;
            textBox1.Enabled = false;
            c_id = NikLogin + "BC";
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            lblinit.Text = $"{Application.ProductName} v{Application.ProductVersion}";
            getip();
            getDatacabang();
            lbl_client.Text = "";
            button4.Enabled = false;
            FrmDTO childForm = new FrmDTO();

            childForm.TopLevel = false;
            tabPage2.Controls.Add(childForm);
            childForm.NikLogin = NikLogin;
            childForm.Show();
        }
        private void MqttClient_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            try
            {
                
                var topic = e.Topic;
                var message = Encoding.UTF8.GetString(e.Message);
                if (topic == "RESPONS_" + clientid + "/BC/" + NikLogin)
                {
                    txt_respons.Invoke((MethodInvoker)(() =>
                    {                  
                        txt_respons.Text += Environment.NewLine + message;
                        txt_respons.SelectionStart = txt_respons.Text.Length;
                        txt_respons.ScrollToCaret();
                    }));
                }
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show($"Error connecting to MQTT broker: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
           
        }

        private void cb_client_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            
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
        private void getDatacabang()
        {
            string connectionString = Fungsi.connectionString ;
            MySqlConnection connection = new MySqlConnection(connectionString);
            try
            {
                cb_client.Items.Clear();
                connection.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM m_cabang where recid='*' and kdcab not in('G218','G219','G257','G259') order by kdcab", connection);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string kdcab = reader.GetString("KDCAB");
                        string namacab = reader.GetString("NAMA");
                        ipcab = reader.GetString("SERVER");
                        usercab = reader.GetString("RDP_USER");
                        passcab = reader.GetString("RDP_PASS");
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
            finally
            {
                connection.Close();
            }
        }

        private void konekbroker(string sclinet)
        {
            Task.Run(() =>
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


                        this.Invoke((MethodInvoker)delegate
                        {
                            commandTopic = "RESPONS_" + sclinet + "/BC/" + NikLogin;
                        });

                        mqttClient = new MqttClient(ServerBroker);
                        mqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;

                        this.Invoke((MethodInvoker)delegate
                        {

                            mqttClient.Subscribe(new string[] { commandTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                            mqttClient.Connect(c_id);
                            inisaha = commandTopic;
                        });
                    }
                    else
                    {
                        if ("RESPONS_" + clientid + "/BC/" + NikLogin != inisaha)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                txt_respons.Clear();
                            });
                        }
                        
                        if (mqttClient != null && mqttClient.IsConnected)
                        {
                            try
                            {
                                mqttClient.Unsubscribe(new string[] { "RESPONS_" + sclinet + "/BC/" + NikLogin });
                            }
                            catch (Exception ex)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    MessageBox.Show($"Error Unsubscribe to MQTT topic: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                });
                            }
                        }

                        try
                        {
                            string newCommandTopic = $"RESPONS_{sclinet}/BC/{NikLogin}";
                            mqttClient.Subscribe(new string[] { newCommandTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                            inisaha = newCommandTopic;
                        }
                        catch (Exception ex)
                        {
                            this.Invoke((MethodInvoker)delegate
                                {
                                    MessageBox.Show($"Error Subscribe to MQTT topic: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.KeyCode == Keys.F9)
            {
                //konekbroker();
                try
                {
                    Task.Run(() =>
                    {
                        if (mqttClient != null && mqttClient.IsConnected)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {

                                string commandTopic = "COMMAND_" + clientid + "/BC/" + NikLogin;
                                mqttClient.Publish(commandTopic, Encoding.UTF8.GetBytes(textBox1.Text));
                                DateTime currentTime = DateTime.Now;
                                txt_respons.Text += Environment.NewLine + clientid + "@ " + currentTime;
                                txt_respons.Text += Environment.NewLine + textBox1.Text;

                                textBox1.Clear();
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
        }

        private void txt_respons_TextChanged(object sender, EventArgs e)
        {

        }

        private void cb_client_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            try
            {
                textBox1.Enabled = false;
                lbl_load.Text = "Ping ke :" + ipcab;
                BTN_RECONNECT.Visible = false;
                if (Fungsi.CheckPing(ipcab))
                {
                    lbl_load.Text = "Ping sukses : " + ipcab;
                    
                    cabang = cb_client.Text.Substring(0, Math.Min(4, cb_client.Text.Length));
                    clientid = "TAMPUNG_" + cabang;
                    var result = Fungsi.cekrdp(cabang);
                    iprdp = "";
                    urdp = "";
                    prdp = "";
                    if (result != null && result.Contains("|"))
                    {
                        iprdp = result.Split(new char[] { '|' })[0];
                        urdp = result.Split(new char[] { '|' })[1];
                        prdp = result.Split(new char[] { '|' })[2];

                    }
                    lbl_load.Text = "MENUNGGU RESPON : " + clientid;
                    FrmCek cek = new FrmCek();
                    Fungsi.Nametmp = clientid;
                    cek.ShowDialog();
                    if (cek.hasil)
                    {
                        
                        konekbroker(clientid);
                        textBox1.Enabled = true;
                        lbl_load.Text = "TERKONEKSI KE : " + clientid;
                        
                    }
                    else
                    {
                        lbl_client.Text = "";
                        lbl_load.Text = "LISTENERS OFFLINE : " + clientid;
                        textBox1.Enabled = false;
                        BTN_RECONNECT.Visible = true;
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

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (mqttClient != null && mqttClient.IsConnected)
            {
                mqttClient.Disconnect();
            }

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
            if (!(cb_client.Text == ""))
            {
                string lok = Application.StartupPath + "\\set.rdp";
                if (File.Exists(lok))
                {
                    File.Delete(lok);
                    Fungsi.CreateRdpFile(lok, iprdp, urdp, prdp);
                }
                else
                {
                    Fungsi.CreateRdpFile(lok, iprdp, urdp, prdp);
                }
                Process.Start(lok);
                //File.Delete(lok);
                lbl_rdp.Visible = true;
                txt_rdp.Visible = true;
                txt_rdp.Text = prdp;
            }
            

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
    }
}
