using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Http;
using System.Data.SqlTypes;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        string bed = "";
        List<room> hospital = new List<room>();
        class room
        {
            patient pt1 = new patient();
            patient pt2 = new patient();
            public patient getPt1()
            {
                return pt1;
            }
            public patient getPt2()
            {
                return pt2;
            }
            public bool inDanger()
            {
                if(pt1.inDanger() || pt2.inDanger()) { return true; }
                return false;
            }
        }
        class patient{
            private int sugar = 110;
            private int beat = 80;
            public void setSugar(int x)
            {
                sugar = x;
            }
            public void setBeat(int x)
            {
                beat = x;
            }
            public int getSugar()
            {
                return sugar;
            }
            public int getBeat()
            {
                return beat;
            }
            public bool inDanger()
            {
                if(sugar > 140 || beat < 60) { return true; }
                return false;
            }
        }
        public Form1()
        {
            InitializeComponent();
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            await post();
        }
        async Task post()
        {
            // 创建HttpClient实例
            using (HttpClient httpClient = new HttpClient())
            {
                // 准备要发送的数据
                var postData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("sugar", textBox1.Text),
                    new KeyValuePair<string, string>("beat", textBox2.Text)
                });
                try
                {
                    // 发送POST请求并获取响应
                    HttpResponseMessage response = await httpClient.PostAsync("http://192.168.125.134:1880/"+bed, postData);

                    // 检查响应是否成功
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        //label3.Text = responseContent;
                    }
                    else
                    {
                        Console.WriteLine($"失敗。{response.StatusCode}");
                        //label3.Text = response.StatusCode.ToString();
                    }
                }
                catch (Exception ex)
                {
                    //label3.Text = ex.Message;
                    Console.WriteLine($"發生異常：{ex.Message}");
                }
            }

        }
        async Task listenToInput()
        {
            string url = "http://192.168.1.21:6000/";
            using (HttpListener listener = new HttpListener())
            {
                listener.Prefixes.Add(url);
                listener.Start();
                Console.WriteLine($"Server started at {url}");
                label3.Text = "start listening...";
                while (true)
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    HandleRequest(context);
                }
            }
        }
        void HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            System.IO.Stream body = request.InputStream;
            System.IO.StreamReader reader = new System.IO.StreamReader(body, request.ContentEncoding);
            string text = reader.ReadToEnd();
            JObject jsonObj = JObject.Parse(text);
            string sugarValue = jsonObj["sugar"].ToString();
            string beatValue = jsonObj["beat"].ToString();
            string bedValue = jsonObj["bed"].ToString();
            textBox3.Text = sugarValue + "\t";
            textBox3.Text += beatValue + "\t";
            textBox3.Text += bedValue;
            modifyGraph(bedValue, sugarValue, beatValue);
            uploadThingSpeak(bedValue, int.Parse(sugarValue), int.Parse(beatValue));

            string responseString = "收到!!";  // 這是一個簡單的示例回應

            // 設定回應內容類型和長度
            HttpListenerResponse response = context.Response;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentType = "text/plain";
            response.ContentLength64 = buffer.Length;

            // 寫入回應到 OutputStream
            System.IO.Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();

        }
        private async void uploadThingSpeak(string bed, int sugar, int beat)
        {
            string ThingSpeakWriteApiKey = "VKODHS9F6S4EV3EF"; // 替換成你的 Write API Key
            string channel1, channel2;
            if(bed == "1-1")
            {
                channel1 = "&field1=";
                channel2 = "&field2=";
            }
            else if(bed == "1-2")
            {
                channel1 = "&field3=";
                channel2 = "&field4=";
            }
            else if(bed == "2-1")
            {
                channel1 = "&field5=";
                channel2 = "&field6=";
            }
            else
            {
                channel1 = "&field7=";
                channel2 = "&field8=";
            }
            using (HttpClient httpClient = new HttpClient())
            {
                string apiUrl = "https://api.thingspeak.com/update?api_key="+ThingSpeakWriteApiKey+channel1+sugar+channel2+beat;

                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Data uploaded successfully. Response: {responseContent}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to upload data. Status code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading data: {ex.Message}");
                }
            }
        }
        private void modifyGraph(string bed, string sugarValue, string beatValue)
        {
            Bitmap temp;
            if (int.Parse(sugarValue) <= 140 && int.Parse(beatValue) >= 60)
            {
               temp = new Bitmap("..\\picture\\0-0.PNG");
            }
            else if(int.Parse(sugarValue) > 140 && int.Parse(beatValue) >= 60)
            {
                temp = new Bitmap("..\\picture\\1-0.PNG");
            }
            else if (int.Parse(sugarValue) <= 140 && int.Parse(beatValue) < 60)
            {
                temp = new Bitmap("..\\picture\\0-1.PNG");
            }
            else
            {
                temp = new Bitmap("..\\picture\\1-1.PNG");
            }

            if (bed == "1-1")
            {
                hospital[0].getPt1().setSugar(int.Parse(sugarValue));
                hospital[0].getPt1().setBeat(int.Parse(beatValue));
                pictureBox2.Image = temp;
                if (hospital[0].inDanger())
                {
                    pictureBox6.Image = new Bitmap("..\\picture\\roomOn.PNG");
                }
                else
                {
                    pictureBox6.Image = new Bitmap("..\\picture\\roomOff.PNG");
                }
            }
            else if(bed == "1-2")
            {
                hospital[0].getPt2().setSugar(int.Parse(sugarValue));
                hospital[0].getPt2().setBeat(int.Parse(beatValue));
                pictureBox3.Image = temp;
                if (hospital[0].inDanger())
                {
                    pictureBox6.Image = new Bitmap("..\\picture\\roomOn.PNG");
                }
                else
                {
                    pictureBox6.Image = new Bitmap("..\\picture\\roomOff.PNG");
                }
            }
            else if(bed == "2-1")
            {
                hospital[1].getPt1().setSugar(int.Parse(sugarValue));
                hospital[1].getPt1().setBeat(int.Parse(beatValue));
                pictureBox4.Image = temp;
                if (hospital[1].inDanger())
                {
                    pictureBox7.Image = new Bitmap("..\\picture\\roomOn.PNG");
                }
                else
                {
                    pictureBox7.Image = new Bitmap("..\\picture\\roomOff.PNG");
                }
            }
            else
            {
                hospital[1].getPt2().setSugar(int.Parse(sugarValue));
                hospital[1].getPt2().setBeat(int.Parse(beatValue));
                pictureBox5.Image = temp;
                if (hospital[1].inDanger())
                {
                    pictureBox7.Image = new Bitmap("..\\picture\\roomOn.PNG");
                }
                else
                {
                    pictureBox7.Image = new Bitmap("..\\picture\\roomOff.PNG");
                }
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            bed = "bed1-1";
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            bed = "bed1-2";
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            bed = "bed2-1";
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            bed = "bed2-2";
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await listenToInput();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox6.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox7.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.Image = new Bitmap("..\\picture\\0-0.PNG");
            pictureBox3.Image = new Bitmap("..\\picture\\0-0.PNG");
            pictureBox4.Image = new Bitmap("..\\picture\\0-0.PNG");
            pictureBox5.Image = new Bitmap("..\\picture\\0-0.PNG");
            pictureBox6.Image = new Bitmap("..\\picture\\roomOff.PNG");
            pictureBox7.Image = new Bitmap("..\\picture\\roomOff.PNG");
            hospital.Add(new room());
            hospital.Add(new room());
        }


    }
}
