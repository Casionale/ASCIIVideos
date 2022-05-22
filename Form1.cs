using Emgu.CV;
using FFmpeg.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ASCIIVideos
{
    public partial class Form1 : Form
    {
        WebBrowser browser = new WebBrowser();
        Bitmap img;
        static string density = "N@#W$9876543210?!abc;:+=-,._        ";
        public Form1()
        {
            InitializeComponent();
            panel1.Controls.Add(browser);
            browser.Dock = DockStyle.Fill;
        }


        private string GetASCII(OpenFileDialog ofd, int width, int height)
        {
            img = new Bitmap(ofd.FileName);
            Bitmap newImg = ScaleGray(ResizeImage(img, new Size(width, height)));
            pictureBox1.Image = newImg;
            label1.Text = newImg.Width + " x " + newImg.Height;
            string sImg = ToASCII(newImg);
            return sImg;
        }

        private string GetASCII(Bitmap img, int width, int height)
        {
            Bitmap newImg = ScaleGray(ResizeImage(img, new Size(width, height)));

            pictureBox1.Image = new Bitmap(newImg);
            string msg = newImg.Width + " x " + newImg.Height;

            Invoke(new Action(() =>
            {
                label1.Text = msg;
            }));
            Bitmap buf = newImg;
            string sImg = ToASCII(buf);
            return sImg;
        }

        public static Bitmap ResizeImage(Bitmap imgToResize, Size size)
        {
            try
            {
                Bitmap b = new Bitmap(size.Width, size.Height);
                using (Graphics g = Graphics.FromImage((Image)b))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
                }
                return b;
            }
            catch
            {
                Console.WriteLine("Bitmap could not be resized");
                return imgToResize;
            }
        }

        public static Bitmap ScaleGray(Bitmap img)
        {
            for (int i = 0; i < img.Width; i++)
                for (int j = 0; j < img.Height; j++)
                {
                    Color c = img.GetPixel(i, j);
                    int color = (int)((0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B));
                    img.SetPixel(i, j, Color.FromArgb(color, color, color));
                }
            return img;
        }

        public static string ToASCII(Bitmap img)
        {

            string sImg = "";
            int max = density.Length;

            int height = img.Height;
            int width = img.Width;

            Bitmap buf = new Bitmap(img);



            for (int i = 0; i < height - 1; i++)
            {
                for (int j = 0; j < width - 1; j++)
                {
                    byte c;
                    //c = bbuf[j][i];
                    c = buf.GetPixel(j, i).R;
                    double relativeValue = c / 255.0;
                    int scaledValue = (int)(max * relativeValue);
                    sImg += density[scaledValue];
                }
                sImg += '\n';
            }

            return sImg;

        }



        public void TCamera(int width, int height)
        {
            VideoCapture capture = new VideoCapture();

            while (true)
            {

                Bitmap image = capture.QueryFrame().ToBitmap();
                //string sImg = GetASCII(image, 120, 100);
                string sImg = GetASCII(image, width, height);
                string msg = "<pre>" + sImg.Replace("\n", "<br>") + "</pre>";

                Invoke(new Action(() =>
                {
                    browser.DocumentText = msg;
                }));

                //Thread.Sleep(200);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();

            string sImg = GetASCII(ofd, 120, 100);

            browser.DocumentText = "<pre>" + sImg.Replace("\n", "<br>") + "</pre>";
            StreamWriter sw = new StreamWriter("out.txt");
            sw.Write(sImg);
            sw.Close();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Thread t = new Thread(() => TCamera(Int32.Parse(textBox1.Text), Int32.Parse(textBox2.Text)));
            t.Start();
            button2.Enabled = false;
        }

        public void VideoBroadcast()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Видео|*.mp4";

            Invoke(new Action(() =>
            {
                ofd.ShowDialog();
            }));

            DateTime now = DateTime.Now;

            while (true)
            {
                DateTime dTime = DateTime.Now;
                TimeSpan interval = dTime.Subtract(now);
                Bitmap img = new Bitmap(FromFile(ofd.FileName, interval));

                string sImg = GetASCII(img, 120, 100);
                string msg = "<pre>" + sImg.Replace("\n", "<br>") + "</pre>";

                Invoke(new Action(() =>
                {
                    pictureBox2.Image = img;
                    browser.DocumentText = msg;
                    label4.Text = Math.Round(interval.TotalMilliseconds).ToString();
                }));
            }
        }

        public static Bitmap FromFile(string fileName, TimeSpan time)
        {
            string path = @"C:\Users\Admin\source\repos\ASCIIVideos\bin\Debug\netcoreapp3.1\ffmpeg-2022-05-19-git-dd99d34d67-full_build\bin\ffmpeg.exe";

            var ffmpeg = new Engine(path);

            var options = new ConversionOptions { Seek = time };

            InputFile inputFile = new InputFile(fileName);

            OutputFile outputFile = new OutputFile("buf.jpg");

            ffmpeg.GetThumbnailAsync(inputFile, outputFile, options, new CancellationToken()).Wait();
            return new Bitmap("buf.jpg");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //new Thread(() => VideoBroadcast()).Start();
            Thread t = new Thread(() => abs(120, 100));
            t.Start();
        }


        public void abs(int width, int height)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Видео|*.mp4";

            Invoke(new Action(() =>
            {
                ofd.ShowDialog();
            }));
            Mat m = new Mat();

            VideoCapture capture = new VideoCapture(ofd.FileName);

            var fc = capture.Get(Emgu.CV.CvEnum.CapProp.FrameCount);
            
            Mat mm = new Mat();
            
            List<Bitmap> frames = new List<Bitmap>();
            for (int i = 0; i < fc; i+=2)
            {
                

                capture.Set(Emgu.CV.CvEnum.CapProp.PosFrames, i);
                capture.Read(mm);

                frames.Add(mm.ToBitmap());
                Invoke(new Action(() =>
                {
                    label4.Text = i + " / " + fc;
                }));
            }

            for(int i = 0; i < frames.Count; i++)
            {
                Bitmap b = new Bitmap(frames[i]);

                string sImg = GetASCII(b, 120, 100);
                string msg = "<pre>" + sImg.Replace("\n", "<br>") + "</pre>";

                Invoke(new Action(() =>
                {
                    browser.DocumentText = msg;
                    pictureBox2.Image = b;
                }));
                Thread.Sleep(1000 / 16);
            }

            /*
            int cuttentFrame = 0;
            while(cuttentFrame < fc)
            {
                NewFrame(m, capture, cuttentFrame);
                Thread.Sleep(1000 / 30);
                cuttentFrame++;
            }
            */

        }

        private void NewFrame(Mat m, VideoCapture capture, int cuttentFrame)
        {
            capture.Set(Emgu.CV.CvEnum.CapProp.PosFrames, cuttentFrame);
            capture.Read(m);


            Bitmap b = new Bitmap(m.ToBitmap());

            string sImg = GetASCII(b, 120, 100);
            string msg = "<pre>" + sImg.Replace("\n", "<br>") + "</pre>";

            Invoke(new Action(() =>
            {
                browser.DocumentText = msg;
                pictureBox2.Image = b;
            }));
        }
    }
}
