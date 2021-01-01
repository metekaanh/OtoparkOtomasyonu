using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.IO.Ports;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using openalprnet;

namespace OtoparkOtomasyonu
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Helper.Initialize();
            Helper.vcd.NewFrame += Vcd_NewFrame;
           
            string[] ports = SerialPort.GetPortNames();  //Seri portları diziye ekleme
            foreach (string port in ports) comboBox1.Items.Add(port); //Seri portları comboBox1'e ekleme
            comboBox1.SelectedIndex = 0;
            label3.Visible = false;
            label1.Visible = false;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                Helper.ArduinoBaslat(comboBox1.Text); //txtPort içerisindeki değeri seri port ismimize atıyoruz. İkiside string, convert'e gerek yok.
                if (Helper.arduino.IsOpen) //şart sağlanıyorsa seri port açılarak aşağıdaki işlemler gerçekleşiyor
                {
                    lblSonuc.Visible = true;
                    lblSonuc.Text = "Bağlantı Kuruldu";
                    lblSonuc.ForeColor = Color.Green;
                    tmrMesafe.Start(); //timerı başlatıyoruz
                    label3.Visible = false;
                    label1.Visible = false;


                }
            }
            catch (Exception) //herhangi bir hataya karşı try/catch bloğunu kullanıyoruz
            {
                lblSonuc.Visible = false;
                lblSonuc.Text = "Bağlantı Kurulamadı";
                lblSonuc.ForeColor = Color.Red;
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            tmrMesafe.Stop();
            Helper.arduino.Close();
            lblSonuc.Visible = true;
            lblSonuc.Text = "Bağlantı Sonlandırıldı";
            lblSonuc.ForeColor = Color.Red;
            txtMesafe.Text = ""; //durdurma işleminden sonra port ismi, mesafe değeri ve bar oranı sıfırlanıyor

            label3.Visible = false;
            label1.Visible = false;
            if (picTaken.Image != null)
            {
                picTaken.Image.Dispose();
                picTaken.Image = null;
            }
        }
        private DateTime openTime= DateTime.Now;
        private void tmrMesafe_Tick(object sender, EventArgs e)
        {
            //tmrMesafe.Stop();
            if ((DateTime.Now - openTime).TotalSeconds > 10)
            {
                Helper.KapiKapat();
            }
            int mesafe = Helper.MesafeOku();   
            txtMesafe.Text = mesafe.ToString(); //elde edilen bilgi değeri mesafe textbox'ının içerisine string'e çevrilerek yazılıyor.
            if (mesafe < 10)
            {
                label1.Text = "";
                label1.Visible = false;
                picTaken.Image = null;
                Bitmap resim = new Bitmap(picCam.Image);
                picTaken.Image = (Bitmap)resim.Clone();
                var plaka = Helper.ProcessImageFile(resim, out Bitmap yeniResim);
                txtPlaka.Text = plaka;
                picPlakaResmi.Image = yeniResim;
                Helper.KapiAc(plaka);
                openTime = DateTime.Now;
                label3.Text = "Fotoğraf Çekildi.";
                label3.Visible = true;

            }
            else if (mesafe > 10)
            {
                label1.Text = "YAKINDA NESNE YOK.";
                label1.Visible = true;
            }
            //tmrMesafe.Start();
        }

        private void Vcd_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            picCam.Image = (Bitmap)eventArgs.Frame.Clone();
        }
       
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
      
    }
}
