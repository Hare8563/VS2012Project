using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenCvSharp;
using System.Windows.Forms.DataVisualization.Charting;
using LabImg;
using OpenCvSharp.Extensions;

namespace GUISample
{
    public partial class Form2 : Form
    {
        IplImage horizontal, vertical;

        public Form2(IplImage horizontal, IplImage vertical)
        {
            InitializeComponent();
            this.horizontal = horizontal;
            this.vertical = vertical;
            comboBox1.Items.Add(@"なし");
            comboBox1.Items.Add(@"Sobel");
            comboBox1.Items.Add(@"Laplacian");
            comboBox1.Items.Add(@"Canny");
            comboBox1.SelectedIndex = 0;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            IplImage grayH = new IplImage(horizontal.Width, horizontal.Height, BitDepth.U8, 1);
            IplImage grayV = new IplImage(vertical.Width, vertical.Height, BitDepth.U8, 1);

            Cv.CvtColor(horizontal, grayH, ColorConversion.BgrToGray);
            Cv.CvtColor(vertical, grayV, ColorConversion.BgrToGray);

            imageProcessing(grayH, grayV);
            
        }

        /// <summary>
        /// chart1に適切な処理を行いプロットする関数
        /// </summary>
        /// <param name="img1"></param>
        /// <param name="img2"></param>
        private void imageProcessing(IplImage img1, IplImage img2)
        {
            IplImage diff=new IplImage(img1.Width,img1.Height,BitDepth.U8,1);

            //エッジ抽出
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    Cv.Sobel(img1, img1, 1, 0);
                    Cv.ConvertScaleAbs(img1, img1);
                    Cv.Sobel(img2, img2, 1, 0);
                    Cv.ConvertScaleAbs(img2, img2);
                    break;
                case 2:
                    Cv.Laplace(img1, img1);
                    Cv.ConvertScaleAbs(img1, img1);
                    Cv.Laplace(img2, img2);
                    Cv.ConvertScaleAbs(img2, img2);
                    break;
                case 3:
                    Cv.ConvertScaleAbs(img1, img1);
                    Cv.ConvertScaleAbs(img2, img2);
                    Cv.Canny(img1, img1, 50, 200);
                    Cv.Canny(img2, img2, 50, 200);
                    break;
                default:
                    break;
            }



            pictureBox1.Image = ResizeImage(img1.ToBitmap(),pictureBox1.Width,pictureBox1.Height);
            pictureBox2.Image = ResizeImage(img2.ToBitmap(), pictureBox2.Width, pictureBox2.Height);

            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage;




            float[] gHFloat = new float[img1.Width * img1.Height];
            float[] gVFloat = new float[img2.Width * img2.Height];
            float[] imgDiff = new float[img1.Width * img1.Height];
            //float[,] gHFloat = new float[grayH.Width, grayH.Height];
            //float[,] gVFloat = new float[grayV.Width, grayV.Height];

            unsafe
            {
                byte* gHptr = (byte*)img1.ImageData;
                byte* gVptr = (byte*)img2.ImageData;
                byte* diffPtr = (byte*)diff.ImageData;
                for (int y = 0; y < img2.Height; y++)
                {
                    for (int x = 0; x < img2.Width; x++)
                    {
                        int offsetH = (img1.WidthStep * y) + x;
                        int offsetV = (img2.WidthStep * y) + x;
                        int offsetDiff = (diff.WidthStep * y) + x;
                        gHFloat[x + img1.Width * y] = (float)gHptr[offsetH];
                        gVFloat[x + img2.Width * +y] = (float)gVptr[offsetV];

                        //差分をとってみる
                        imgDiff[x + img1.Width * y] = (float)gHptr[offsetH] - (float)gVptr[offsetV];

                        if (imgDiff[x + img1.Width * y] >= 255) diffPtr[offsetDiff] = 255;
                        else if (imgDiff[x + img1.Width * y] <= 0) diffPtr[offsetDiff] = 0;
                        else diffPtr[offsetDiff] = (byte)imgDiff[x + img1.Width * y];
                    }
                }
            }

            pictureBox3.Image = ResizeImage(diff.ToBitmap(), pictureBox3.Width, pictureBox3.Height);
            //float maxX=gHFloat.Max();
            //float maxY=gVFloat.Max();

            //散布図表示
            ChartUtility.PlotChartXY(chart2, img1, img2, gHFloat.Min(), gHFloat.Max(), gVFloat.Min(), gVFloat.Max());
            //差分の折れ線表示
            ChartUtility.PlotChart(chart1, imgDiff, 0.0f, imgDiff.Length, imgDiff.Min(), imgDiff.Max());
            //ChartUtility.PlotChartXY(chart1,gHFloat,gVFloat,0.0f,maxX,0.0f,256f);


        }

        private void EdgeFilterIndexChanged(object sender, EventArgs e)
        {
            IplImage grayH = new IplImage(horizontal.Width, horizontal.Height, BitDepth.U8, 1);
            IplImage grayV = new IplImage(vertical.Width, vertical.Height, BitDepth.U8, 1);

            Cv.CvtColor(horizontal, grayH, ColorConversion.BgrToGray);
            Cv.CvtColor(vertical, grayV, ColorConversion.BgrToGray);

            imageProcessing(grayH, grayV);
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
