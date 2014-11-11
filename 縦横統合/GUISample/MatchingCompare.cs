using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LabImg;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Windows.Forms.DataVisualization.Charting;

namespace GUISample
{
    public partial class MatchingCompare : Form
    {
        private IplImage _horizontal, _vertical, _DPImg, _CDPImg, _horizontalBase, _verticalBase;
        private IplImage _horizontalLeft, _verticalLow;

        public MatchingCompare()
        {
            InitializeComponent();
        }

        public MatchingCompare(IplImage horizontal, IplImage vertical, IplImage DPImg, IplImage CDPImg, IplImage horiontalBase, IplImage verticalBase, IplImage horizontalLeft, IplImage verticalLow)
        {
            InitializeComponent();

            this._horizontal = horizontal;
            this._horizontalBase = horiontalBase;
            this._vertical = kaneUtility.imageRotation270(vertical);
            this._verticalBase = kaneUtility.imageRotation270(verticalBase);
            this._DPImg = DPImg;
            this._CDPImg = CDPImg;
            this._horizontalLeft = horizontalLeft;
            this._verticalLow = kaneUtility.imageRotation270(verticalLow);
        }

        private void MatchingCompare_Load(object sender, EventArgs e)
        {
            float minValue = 0.1F;
            float maxValue = 5000;

            CommonUtility.FillPicBox(_horizontalBase.ToBitmap(), pictureBox1);
            CommonUtility.FillPicBox(_verticalBase.ToBitmap(), pictureBox2);
            CommonUtility.FillPicBox(_horizontalLeft.ToBitmap(), pictureBox3);
            CommonUtility.FillPicBox(_verticalLow.ToBitmap(), pictureBox4);

            unchecked
            {
                kaneUtility.SetDisparity(_horizontal, minValue, maxValue, horizontalBox, BarBox1, textBox1, textBox2);
                kaneUtility.SetDisparity(_vertical, minValue, maxValue, VerticalBox, BarBox2, textBox3, textBox4);
                kaneUtility.SetDisparity(_DPImg, minValue, maxValue, DPBox, BarBox3, textBox5, textBox6);
                kaneUtility.SetDisparity(_CDPImg, minValue, maxValue, CDPBox, BarBox4, textBox7, textBox8);

                PlotChartXY(chart1, _horizontal, _vertical, minValue, maxValue, minValue, maxValue);

                PlotChartXY(chart2, _horizontal, _DPImg, minValue, maxValue, minValue, maxValue);

                PlotChartXY(chart3, _horizontal, _CDPImg, minValue, maxValue, minValue, maxValue);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
                kaneUtility.SetDisparity(_horizontal, float.Parse(textBox2.Text), float.Parse(textBox1.Text), horizontalBox, BarBox1, textBox1, textBox2);
                kaneUtility.SetDisparity(_vertical, float.Parse(textBox2.Text), float.Parse(textBox1.Text), VerticalBox, BarBox2, textBox3, textBox4);
                kaneUtility.SetDisparity(_DPImg, float.Parse(textBox2.Text), float.Parse(textBox1.Text), DPBox, BarBox3, textBox5, textBox6);
                kaneUtility.SetDisparity(_CDPImg, float.Parse(textBox2.Text), float.Parse(textBox1.Text), CDPBox, BarBox4, textBox7, textBox8);

                PlotChartXY(chart1, _horizontal, _vertical, float.Parse(textBox2.Text), float.Parse(textBox1.Text), float.Parse(textBox2.Text), float.Parse(textBox1.Text));
                PlotChartXY(chart2, _horizontal, _DPImg, float.Parse(textBox2.Text), float.Parse(textBox1.Text), float.Parse(textBox2.Text), float.Parse(textBox1.Text));
                PlotChartXY(chart3, _horizontal, _CDPImg, float.Parse(textBox2.Text), float.Parse(textBox1.Text), float.Parse(textBox2.Text), float.Parse(textBox1.Text));
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
        public void PlotChartXY(Chart chart, IplImage plot1, IplImage plot2, float minX, float maxX, float minY, float maxY)
        {
            //init series
            Series series1 = new Series("plot1");

            //setType
            series1.ChartType = SeriesChartType.Point;
            int i=0, j=0;
            int xValue=0, yValue=0;
            unchecked
             {
                    for (i = 0; i < plot1.Height; i++)
                        for (j = 0; j < plot1.Width; j++)
                        {
                            
                            try
                            {
                                
                                checked
                                {
                                    xValue = (int)plot1[i, j].Val0;
                                    yValue = (int)plot2[i, j].Val0;

                                    series1.Points.AddXY(xValue, yValue);
                                }
                            }
                            catch (OverflowException ex)
                            {
                                series1.Points.AddXY(0, 0);
                               Console.WriteLine("\""+ex.Message+"\"が"+i.ToString()+", "+j.ToString()+"で発生しました。0に置き換えます");
                            }
                            /*finally
                            {
                                Console.WriteLine(@"plot1:"+plot1[i, j].Val0.ToString());
                                Console.WriteLine(@"plot2:"+plot2[i, j].Val0.ToString());
                                Console.WriteLine(@"i:" + i.ToString() + @", j:" + j.ToString());
                            }*/
                        }

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
            ca.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            ca.AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
            //ca.AxisX.Title.


            //add series in Chart
            chart.Series.Clear();
            chart.Series.Add(series1);

            //add chartArea in Chart
            chart.ChartAreas.Clear();
            chart.ChartAreas.Add(ca);

        }


    }
}
