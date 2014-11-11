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

namespace GUISample
{
    public partial class DepthMap : Form
    {
        Bitmap depthImg;
        IplImage _img;

        public DepthMap(IplImage img)
        {
            InitializeComponent();
            _img = img;
            depthImg = img.ToBitmap();
        
        }

        private void DepthMap_Load(object sender, EventArgs e)
        {
            IplImage img = Adjust(_img);
            int[] hist = new int[256];

            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    int offset = (int)img[y, x].Val0;
                    hist[offset]++;
                }
            }


            string legend = "Histogram";

            chart1.Series.Clear();
            chart1.Series.Add(legend);

            chart1.Series[legend].ChartType = SeriesChartType.Column;
            chart1.Series[legend].LegendText = legend;

            for (int i = 0; i < hist.Length; i++)
            {
                DataPoint dp = new DataPoint();

                dp.SetValueXY(i, hist[i]);
                dp.IsValueShownAsLabel = false;
                chart1.Series[legend].Points.Add(dp);
            }

                pictureBox1.Image = depthImg;

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 0~255へ変換
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private IplImage Adjust(IplImage img)
        {
            IplImage resultImg = new IplImage(img.Width, img.Height,BitDepth.U8, 1);
            float dmin = kaneUtility.minIplImage(img);
            float dmax = kaneUtility.maxIplImage(img);

            for (int y = 0; y < img.Height; y++)
            {
                for(int x = 0; x < img.Width; x++)
                {
                    resultImg[y, x] = (byte)(255 * (img[y, x].Val0 - dmin) / (dmax - dmin));
                }
            }


                return resultImg;
        }

    }
}
