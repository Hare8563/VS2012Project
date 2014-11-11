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
using System.Windows.Forms.DataVisualization.Charting;

namespace GUISample
{
    public partial class ChartForm : Form
    {
        private float minX, minY, maxX, maxY;
        private float[] plot1;
        private float[] plot2;
        private Bitmap bmp1,bmp2;
        private CvMat horizontalGX, horizontalGY, horizontalGN, horizontalPH;
        private CvMat verticalGX, verticalGY, verticalGN, verticalPH;

        public ChartForm(float[] plot1, float[] plot2, float minX, float maxX, float minY, float maxY,Bitmap bmp1, Bitmap bmp2)
        {
            InitializeComponent();
            this.plot1 = plot1;
            this.plot2 = plot2;
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
            this.bmp1 = bmp1;
            this.bmp2 = bmp2;

            
        }

        public ChartForm(float[] plot1,float[] plot2, float minX, float maxX, float minY, float maxY, IplImage horizontal, IplImage vertical)
        {
            InitializeComponent();
            this.plot1 = plot1;
            this.plot2 = plot2;
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.maxY = maxY;
            IplImage grayH = new IplImage(horizontal.Width, horizontal.Height, BitDepth.U8, 1);
            IplImage grayV = new IplImage(vertical.Width, vertical.Height, BitDepth.U8, 1);
            Cv.CvtColor(horizontal, grayH, ColorConversion.BgrToGray);
            Cv.CvtColor(vertical, grayV, ColorConversion.BgrToGray);

            Disparity.imageGradient(grayH,out horizontalGX,out horizontalGY,out horizontalGN,out horizontalPH);
            Disparity.imageGradient(grayV, out verticalGX, out verticalGY, out verticalGN, out verticalPH);

            bmp1 = horizontal.ToBitmap();
            bmp2 = vertical.ToBitmap();
        }

        private void ChartForm_Load(object sender, EventArgs e)
        {
            ChartUtility.PlotChart(chart1, plot1, plot2, minX, maxX, minY, maxY);
            IplImage HorizontalIplGN = Cv.GetImage(horizontalGN);
            IplImage VerticalIplGN = Cv.GetImage(verticalGN);

            IplImage HorizontalIplPH = Cv.GetImage(horizontalPH);
            IplImage VerticalIplPH = Cv.GetImage(verticalPH);
            if (bmp1 != null)
            {
                pictureBox1.Image = ResizeImage(HorizontalIplGN.ToBitmap(), pictureBox1.Size.Width, pictureBox1.Size.Height);
                pictureBox3.Image = ResizeImage(HorizontalIplPH.ToBitmap(), pictureBox3.Size.Width, pictureBox3.Size.Height);
            }
            if (bmp2 != null)
            {
                pictureBox2.Image = ResizeImage(VerticalIplGN.ToBitmap(), pictureBox2.Size.Width, pictureBox2.Size.Height);
                pictureBox4.Image = ResizeImage(VerticalIplPH.ToBitmap(), pictureBox4.Size.Width, pictureBox4.Size.Height);
            }

            label8.Text = histplot(chart2, horizontalPH, horizontalGN,25).ToString();
            label10.Text = histplot(chart3, verticalPH, verticalGN, 25).ToString(); 
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

        private void button1_Click(object sender, EventArgs e)
        {
            IplImage horizontal = (IplImage)BitmapConverter.ToIplImage(bmp1);
            IplImage vertical = (IplImage)BitmapConverter.ToIplImage(bmp2);

            Form2 f = new Form2(horizontal, vertical);

            f.ShowDialog();
        }

        /// <summary>
        /// 指定したチャートにCvMatのヒストグラムをプロットする
        /// </summary>
        /// <param name="chart">プロットするチャート</param>
        /// <param name="mat">対象の行列</param>
        /// <returns>ヒストグラムのもっとも高い値</returns>
        private int histplot(Chart chart,CvMat mat)
        {
            string legend = "Histogram";

            chart.Series.Clear();
            chart.Series.Add(legend);

            chart.Series[legend].ChartType = SeriesChartType.Column;
            chart.Series[legend].LegendText = legend;

            int[] histogram = new int[360];
            int histMax = 0;
            int histMaxValue=int.MinValue;

            for (int y = 0; y < mat.Height; y++)
            {
                for (int x = 0; x < mat.Width; x++)
                {
                    int offset = (int)mat[y,x];
                    offset += 360;
                    if (offset >= 360)
                    {
                        offset -= 360;
                    }

                    histogram[offset]++;
                    if (histMaxValue < histogram[offset])
                    {
                        histMax = offset;
                        histMaxValue = histogram[offset];
                    }
                }
            }

            for (int i = 0; i < histogram.Length; i++)
            {
                DataPoint dp = new DataPoint();

                dp.SetValueXY(i, histogram[i]);
                dp.IsValueShownAsLabel = false;
                chart.Series[legend].Points.Add(dp);
            }


            return histMax;

        }


        private int histplot(Chart chart, CvMat mat1, CvMat mat2, int threshold)
        {
            string legend = "Histogram";

            chart.Series.Clear();
            chart.Series.Add(legend);

            chart.Series[legend].ChartType = SeriesChartType.Column;
            chart.Series[legend].LegendText = legend;

            int[] histogram = new int[360];
            int histMax = 0;
            int histMaxValue = int.MinValue;

            for (int y = 0; y < mat1.Height; y++)
            {
                for (int x = 0; x < mat1.Width; x++)
                {
                    //もしある一定以上の値がmat2の(x,y)にあるのなら
                    if((int)mat2[y,x] > threshold){

                        int offset = (int)mat1[y, x];
                        offset += 360;
                        if (offset >= 360)
                        {
                            offset -= 360;
                        }

                        histogram[offset]++;
                        if (histMaxValue < histogram[offset])
                        {
                            histMax = offset;
                            histMaxValue = histogram[offset];
                        }
                    }
                }
            }

            for (int i = 0; i < histogram.Length; i++)
            {
                DataPoint dp = new DataPoint();

                dp.SetValueXY(i, histogram[i]);
                dp.IsValueShownAsLabel = false;
                chart.Series[legend].Points.Add(dp);
            }


            return histMax;
        }

    }
}
