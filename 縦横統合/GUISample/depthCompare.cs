using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using LabImg;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Collections;

namespace GUISample
{
    public partial class depthCompare : Form
    {
        IplImage depthHorizontal, depthVertical, baseImage;
        int threshold = 100;//誤差100メートルが初期値
        Hashtable xyHash = new Hashtable();
        Point mouseStartPoint;
        Point mouseEndPoint;
        Point horizontalMouseStart, horizontalMouseEnd;
        IplImage horizontalBlock, verticalBlock;
        Boolean drawFlag = false;


        public depthCompare(IplImage dH, IplImage dV)
        {
            InitializeComponent();
            this.depthHorizontal = dH;
            this.depthVertical = dV;
            this.baseImage = new IplImage(dH.Size, BitDepth.U8, 3);
        }

        public depthCompare(IplImage dH, IplImage dV, IplImage bI)
        {
            InitializeComponent();
            this.depthHorizontal = dH;
            //if (dV.Width < dV.Height)
            //{
                this.depthVertical = kaneUtility.imageRotation270(dV);
            //}
            //else
            //{
              //  this.depthVertical = dV;
            //}
            this.baseImage = bI;
        }

        private void depthCompare_Load(object sender, EventArgs e)
        {
            trackBar1.Minimum = 0;
            trackBar1.Maximum = 300;

            // 初期値を設定
            trackBar1.Value = threshold;

            // 描画される目盛りの刻みを設定
            trackBar1.TickFrequency = 10;

            // スライダーをキーボードやマウス、
            // PageUp,Downキーで動かした場合の移動量設定
            trackBar1.SmallChange = 10;
            trackBar1.LargeChange = 50;

            float minValue = kaneUtility.minIplImage(depthHorizontal);
            float maxValue = kaneUtility.maxIplImage(depthHorizontal);
           

            //pictureBox1.Image = ResizeImage((Disparity.normalizeIplDepth(depthHorizontal,0,200)).ToBitmap(), pictureBox1.Width, pictureBox1.Height);
            //pictureBox2.Image = ResizeImage((Disparity.normalizeIplDepth(depthVertical,0,200)).ToBitmap(), pictureBox2.Width, pictureBox2.Height);
            
            kaneUtility.SetDisparity(depthHorizontal, kaneUtility.minIplImage(depthHorizontal), kaneUtility.maxIplImage(depthHorizontal), pictureBox1, pictureBox4, textBox2, textBox3);
            kaneUtility.SetDisparity(depthVertical, kaneUtility.minIplImage(depthVertical), kaneUtility.maxIplImage(depthVertical), pictureBox2, pictureBox5, textBox4, textBox5);
            
            pictureBox3.Image = ResizeImage(baseImage.ToBitmap(), pictureBox3.Width, pictureBox3.Height);
            PlotChartXY(chart1, depthHorizontal, depthVertical, minValue, maxValue, minValue, maxValue, xyHash);
            paintDepthCompare();
        
        }


        private void paintDepthCompare()
        {
            pictureBox3.Image = ResizeImage(baseImage.ToBitmap(), pictureBox3.Width, pictureBox3.Height);

            unsafe
            {
                Bitmap bmp = baseImage.ToBitmap();
                byte* depthHPtr = (byte*)depthHorizontal.ImageData;
                byte* depthVPtr = (byte*)depthVertical.ImageData;

                for (int y = 0; y < depthHorizontal.Height; y++)
                {
                    for (int x = 0; x < depthHorizontal.Width; x++)
                    {
                        int offsetImage = y * depthHorizontal.WidthStep + x;
                        int horizontalPix = (int)depthHPtr[offsetImage];
                        int verticalPix = (int)depthVPtr[offsetImage];

                        //距離がある一定値を超えたら
                        if (horizontalPix - verticalPix > threshold)
                        {
                            Color c = Color.Red;
                            paintPoint(x, y, bmp,c);
                        }
                        else if (horizontalPix - verticalPix < -threshold)
                        {
                            Color c = Color.Blue;
                            paintPoint(x, y, bmp, c);
                        }

                    }
                }
                //pictureBox3.Image = ResizeImage(bmp, pictureBox3.Width, pictureBox3.Height);
                CommonUtility.FillPicBox(bmp, pictureBox3);
            }



        }

        private void paintPoint(int x, int y, Bitmap canvas, Color c)
        {
            Graphics g = Graphics.FromImage(canvas);

            Bitmap p = new Bitmap(1, 1);
            p.SetPixel(0, 0, c);
            g.DrawImageUnscaled(p, x, y);
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

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void ValueChangeEvent(object sender, EventArgs e)
        {
            textBox1.Text = trackBar1.Value.ToString();
            threshold = trackBar1.Value;
            paintDepthCompare();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            threshold = int.Parse(textBox1.Text);
            trackBar1.Value = threshold;
            paintDepthCompare();
        }



        /// <summary>
        /// 散布図表示
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="plot1"></param>
        /// <param name="plot2"></param>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="minY"></param>
        /// <param name="maxY"></param>
        public static void PlotChartXY(Chart chart, IplImage plot1, IplImage plot2, float minX, float maxX, float minY, float maxY, Hashtable ht)
        {
            //init series
            Series series1 = new Series("plot1");

            //setType
            series1.ChartType = SeriesChartType.Point;

            //int L = plot1.Width * plot1.Height;

            for (int i = 0; i < plot1.Height; i++)
                for (int j = 0; j < plot1.Width; j++)
                {
                    int index = series1.Points.AddXY(plot1[i, j].Val0, plot2[i, j].Val0);
                    DataPoint point = series1.Points[index];
                    
                    ht[index] = new Point(j, i);
                }

            //setValueLabel
            series1.IsValueShownAsLabel = false;

            //setPointsize
            series1.MarkerSize = 5;

            //setMarkerType
            series1.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;

            //setMaxAndMin
            ChartArea ca = new ChartArea();
            ca.AxisX.Maximum = maxX;
            ca.AxisX.Minimum = minX;
            ca.AxisY.Maximum = maxY;
            ca.AxisY.Minimum = minY;
            //ca.AxisX.Title.


            //add series in Chart
            chart.Series.Clear();
            chart.Series.Add(series1);

            //add chartArea in Chart
            chart.ChartAreas.Clear();
            chart.ChartAreas.Add(ca);
        }

        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseStartPoint = new Point(e.X, e.Y);
        }

        private void chart1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseEndPoint = new Point(e.X, e.Y);
            HitTestResult result;
            Bitmap bmp = baseImage.ToBitmap();
            for (int i = mouseStartPoint.Y; i < mouseEndPoint.Y; i++)
            {
                for (int j = mouseStartPoint.X; j < mouseEndPoint.X; j++)
                {
                    result = chart1.HitTest(j,i);
                    if (result.ChartElementType == ChartElementType.DataPoint)
                    {
                        int index = result.PointIndex;
                        Point p = (Point)xyHash[index];

                        paintPoint(p.X, p.Y, bmp, Color.Green);
                    }
                }
            }

            pictureBox3.Image = ResizeImage(bmp, pictureBox3.Width, pictureBox3.Height);
        }

        private void Horizontal_MouseDown(object sender, MouseEventArgs e)
        {
            if (drawFlag == false)
            {
                horizontalMouseStart = e.Location;
                drawFlag = true;
            }
            else
            {

            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            kaneUtility.SetDisparity(depthHorizontal, float.Parse(textBox3.Text), float.Parse(textBox2.Text), pictureBox1, pictureBox4, textBox2, textBox3);
            kaneUtility.SetDisparity(depthVertical, float.Parse(textBox3.Text), float.Parse(textBox2.Text), pictureBox2, pictureBox5, textBox4, textBox5);

            PlotChartXY(chart1, depthHorizontal, depthVertical, float.Parse(textBox3.Text), float.Parse(textBox2.Text), float.Parse(textBox3.Text), float.Parse(textBox2.Text), xyHash);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }



    }
}
