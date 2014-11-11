using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using LabImg;


namespace GUISample
{
    public partial class HVCompare : Form
    {
        IplImage horizontal, vertical;

        public HVCompare(IplImage horizontal, IplImage vertical)
        {
            InitializeComponent();
            this.horizontal = horizontal;
            this.vertical = vertical;
        }

        private void HVCompare_Load(object sender, EventArgs e)
        {
            IplImage grayH = new IplImage(horizontal.Width, horizontal.Height, BitDepth.U8, 1);
            IplImage grayV = new IplImage(vertical.Width, vertical.Height, BitDepth.U8, 1);

            Cv.CvtColor(horizontal, grayH, ColorConversion.BgrToGray);
            Cv.CvtColor(vertical, grayV, ColorConversion.BgrToGray);

            imageProcessing(grayH, grayV);
        }


        private void imageProcessing(IplImage img1, IplImage img2)
        {

            pictureBox1.Image = ResizeImage(img1.ToBitmap(), pictureBox1.Width, pictureBox1.Height);
            pictureBox2.Image = ResizeImage(img2.ToBitmap(), pictureBox2.Width, pictureBox2.Height);

            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;

            float[] gHFloat = new float[img1.Width * img1.Height];
            float[] gVFloat = new float[img2.Width * img2.Height];

            unsafe
            {
                byte* gHptr = (byte*)img1.ImageData;
                byte* gVptr = (byte*)img2.ImageData;
                for (int y = 0; y < img2.Height; y++)
                {
                    for (int x = 0; x < img2.Width; x++)
                    {
                        int offsetH = (img1.WidthStep * y) + x;
                        int offsetV = (img2.WidthStep * y) + x;
                        gHFloat[x + img1.Width * y] = (float)gHptr[offsetH];
                        gVFloat[x + img2.Width * +y] = (float)gVptr[offsetV];
                    }
                }
            }

            //散布図表示
            ChartUtility.PlotChartXY(chart1, img1, img2, gHFloat.Min(), gHFloat.Max(), gVFloat.Min(), gVFloat.Max());

        }

        public static Bitmap ResizeImage(Bitmap image, double dw, double dh)
        {
            double hi;
            double imagew = image.Width;
            double imageh = image.Height;

            if ((dh / dw) <= (imageh / imagew))
            {
                hi = dh / imageh;
            }
            else
            {
                hi = dw / imagew;
            }
            int w = (int)(imagew * hi);
            int h = (int)(imageh * hi);

            Bitmap result = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(result);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, 0, 0, result.Width, result.Height);

            return result;
        }

    }
}
