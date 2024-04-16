using System;
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
namespace REG2Publisher
{
    public partial class FrmCek : Form
    {
        MqttClient mqttClient;
        REG2Class Fungsi = new REG2Class();
        string ServerBroker;
        string client;
        string NikLogin;
        string inisaha;
        string inisaha2;
        string c_id;
        public bool hasil;
        private int waktuTersisa;
        public FrmCek()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void FrmCek_Load(object sender, EventArgs e)
        {
            hasil = false;
            client = Fungsi.Nametmp;

            ServerBroker = Fungsi.Server;
            NikLogin = Fungsi.NikLogin;
            c_id = NikLogin + "CEK";
            Ceklisteners();
        }
        private void konekbroker(string sclinet)
        {
            text.Text = "Koneksi ke broker";
            try
            {
                if (mqttClient == null || !mqttClient.IsConnected)
                {
                    if (mqttClient != null && mqttClient.IsConnected)
                    {
                        mqttClient.Disconnect();
                    }
                    string commandTopic = string.Empty;
                    commandTopic = "RESPONS_" + sclinet + "/CEK/" + NikLogin;
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
                            mqttClient.Unsubscribe(new string[] { "RESPONS_" + sclinet + "/" + NikLogin });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error Unsubscribe to MQTT topic: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    try
                    {
                        string newCommandTopic = $"RESPONS_{sclinet}/{NikLogin}";
                        mqttClient.Subscribe(new string[] { newCommandTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                        inisaha = newCommandTopic;
                    }
                    catch (Exception ex)
                    {
                        text.Text = "Koneksi ke gagal";
                        MessageBox.Show($"Error Subscribe to MQTT topic: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to MQTT broker: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void MqttClient_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            try
            {

                var topic = e.Topic;
                var message = Encoding.UTF8.GetString(e.Message);
                if (topic == "RESPONS_" + client + "/CEK/" + NikLogin)
                {
                    if (message == "OK")
                    {
                        hasil = true;
                    }
                }
            }
            catch (Exception ex)
            {
               MessageBox.Show($"Error connecting to MQTT broker: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
              
            }

        }
        private void Ceklisteners()
        {
            try
            {
                konekbroker(client);
                if (mqttClient != null && mqttClient.IsConnected)
                {
                    text.Text = "Kirim test ke broker";
                    string commandTopic2 = "COMMAND_" + client + "/CEK/" + NikLogin;
                    mqttClient.Publish(commandTopic2, Encoding.UTF8.GetBytes("CEK"));
                    int batasWaktuMaksimum = 15000;
                    waktuTersisa = batasWaktuMaksimum / 1000;
                    timer1.Interval = 1000;
                    timer1.Start();
                    text.Text = "Tunggu respon listeners";
                }
            }
            catch (Exception ex)
            {
                Fungsi.Log("ceklisteners", ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            waktuTersisa--;
            lbl_tunggu.Text = $"{waktuTersisa} detik";
            if (hasil)
            {
                timer1.Stop();
                text.Text = "Respon listener OK";
                this.Close();
            }
            if (waktuTersisa <= 0)
            {
                timer1.Stop();
                text.Text = "Listeners tidak merespon";
                this.Close();
                
            }
        }

        private void lbl_tunggu_Click(object sender, EventArgs e)
        {

        }
    }
}
