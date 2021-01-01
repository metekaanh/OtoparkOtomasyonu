using AForge.Video.DirectShow;
using openalprnet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OtoparkOtomasyonu
{
    public class Helper
    {
        public static string cameraName = "Microsoft® LifeCam VX-2000";
        public static SerialPort arduino;

        public static void Initialize()
        {



            var fico = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo f in fico)
            {
                if (f.Name == cameraName)
                {
                    vcd = new VideoCaptureDevice(f.MonikerString);
                    vcd.Start();
                    break;
                }
            }

        }

        #region ImageProcess
        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        public static VideoCaptureDevice vcd;

        public static string config_file = Path.Combine(Helper.AssemblyDirectory, "openalpr.conf");
        public static string runtime_data_dir = Path.Combine(Helper.AssemblyDirectory, "runtime_data");
        public static AlprNet alprNet = new AlprNet("eu", config_file, runtime_data_dir);
        public static Bitmap CombineImages(List<Image> images)
        {
            Bitmap finalImage = null;

            try
            {
                var width = 0;
                var height = 0;

                foreach (var bmp in images)
                {
                    width += bmp.Width;
                    height = bmp.Height > height ? bmp.Height : height;
                }
                finalImage = new Bitmap(width, height);
                using (var g = Graphics.FromImage(finalImage))
                {
                    g.Clear(Color.Black);
                    var offset = 0;
                    foreach (Bitmap image in images)
                    {
                        g.DrawImage(image,
                                    new Rectangle(offset, 0, image.Width, image.Height));
                        offset += image.Width;
                    }
                }

                return finalImage;
            }
            catch (Exception ex)
            {
                if (finalImage != null)
                    finalImage.Dispose();

                throw ex;
            }
            finally
            {
                foreach (var image in images)
                {
                    image.Dispose();
                }
            }
        }
        public static Rectangle BoundingRectangle(List<Point> points)
        {
            // Add checks here, if necessary, to make sure that points is not null,
            // and that it contains at least one (or perhaps two?) elements

            var minX = points.Min(p => p.X);
            var minY = points.Min(p => p.Y);
            var maxX = points.Max(p => p.X);
            var maxY = points.Max(p => p.Y);

            return new Rectangle(new Point(minX, minY), new Size(maxX - minX, maxY - minY));
        }
        public static Image CropImage(Image img, Rectangle cropArea)
        {
            var bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }
        public static string ProcessImageFile(Bitmap image, out Bitmap output)
        {
            var alpr = Helper.alprNet;
            output = null;
            if (!alpr.IsLoaded())
            {
                return "Error initializing OpenALPR";
            }
            var results = alpr.Recognize(image);
            var images = new List<Image>(results.Plates.Count());
            foreach (var result in results.Plates)
            {
                var rect = Helper.BoundingRectangle(result.PlatePoints);
                var cropped = Helper.CropImage(image, rect);
                images.Add(cropped);
                return result.TopNPlates.OrderByDescending(s => s.OverallConfidence).First().Characters;
            }
            if (images.Any())
            {
                output = Helper.CombineImages(images);
            }
            return "";
        }
        #endregion

        #region ArduinoProcess
        public static void ArduinoBaslat(string portName)
        {
            arduino = new SerialPort
            {
                BaudRate = 9600,
                PortName = portName,
                DtrEnable = true
            };
            arduino.Open();
            Thread.Sleep(1000);
            arduino.DtrEnable = false;

        }
        private static bool kapiAcikMi= false;
        public static void KapiAc(string plaka)
        {
            var araba = Arac.ArabaGetir(plaka);
            if (araba != null)
            {
                arduino.Write("b");
                arduino.Write(araba.AracNo.ToString());
                kapiAcikMi = true;
            }
        }
        public static void KapiKapat()
        {
            if (kapiAcikMi)
            {
                arduino.Write("c");
                kapiAcikMi = false;
            }
        }

        public static int MesafeOku()
        {
            arduino.Write("e"); //seri port üzerinden veri yazıyoruz 
            bool okudum = false;
            while (!okudum)
            {
                var gelen = arduino.ReadExisting();
                okudum = int.TryParse(gelen, out int mesafe);
                if (okudum)
                {
                    return mesafe;
                }
            }

            return -1;
        }

        #endregion

    }

}
