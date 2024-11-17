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
using System.IO;

namespace REG2Publisher
{
    
    public partial class FrmCapture : Form
    {
        MqttClient mqttClient;
        public string NikLogin;
        public string clientid;
        public string c_id;
        public string inisaha;
        public string inisaha2;
        public string ServerBroker;
        REG2Class Fungsi = new REG2Class();
        public FrmCapture()
        {
            InitializeComponent();
        }
        private void MqttClient_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            try
            {

                var topic = e.Topic;
                var message = Encoding.UTF8.GetString(e.Message);
                if (topic == "RESPONS_" + clientid + "/SS/" + NikLogin)
                {
                    byte[] data = e.Message;
                    using (MemoryStream stream = new MemoryStream(data))
                    {
                        Bitmap screenshot = new Bitmap(stream);

                        pictureBox1.Invoke(new Action(() => pictureBox1.Image = screenshot));

                        Console.WriteLine("Tangkapan layar berhasil ditampilkan.");
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

        }
        public void ButtonClick()
        {
            ServerBroker = Fungsi.Server;
            c_id = NikLogin + "SS";
            button1.PerformClick();
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
                            commandTopic = "RESPONS_" + sclinet + "/SS/" + NikLogin;
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
                        if ("RESPONS_" + clientid + "/SS/" + NikLogin != inisaha)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                
                            });
                        }

                        if (mqttClient != null && mqttClient.IsConnected)
                        {
                            try
                            {
                                mqttClient.Unsubscribe(new string[] { "RESPONS_" + sclinet + "/SS/" + NikLogin });
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
                            string newCommandTopic = $"RESPONS_{sclinet}/SS/{NikLogin}";
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
        public void reqSS()
        {
           try
                {
                konekbroker(clientid);
                    Task.Run(() =>
                    {
                        if (mqttClient != null && mqttClient.IsConnected)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {

                                string commandTopic = "COMMAND_" + clientid + "/SS/" + NikLogin;
                                mqttClient.Publish(commandTopic, Encoding.UTF8.GetBytes("SS"));

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

        private void FrmCapture_Load(object sender, EventArgs e)
        {
            ServerBroker = Fungsi.Server;
            c_id = NikLogin + "SS";
            button1.PerformClick();

        }

        public void button1_Click(object sender, EventArgs e)
        {
            reqSS();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
