using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShoNS.Array;
using System.Threading.Tasks;
using System.Threading;
using OpenCvSharp;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace LabImg
{
	/// <summary>
	/// チャート表示の関数クラス
	/// </summary>
	public static class ChartUtility
	{
		/// <summary>
		/// XY-plotの散布図で点集合を表示
		/// </summary>
		/// <param name="chart"></param>
		/// <param name="plot1"></param>
		/// <param name="plot2"></param>
		/// <param name="minX"></param>
		/// <param name="minY"></param>
		/// <param name="maxX"></param>
		/// <param name="maxY"></param>
		public static void PlotChartXY(Chart chart, int[] plot1, int[] plot2, int minX, int maxX, int minY, int maxY)
		{
			//init series
			Series series1 = new Series("plot1");

			//setType
			series1.ChartType = SeriesChartType.Point;

			for (int i = 0; i < plot2.Length; i++)
				series1.Points.AddXY(plot1[i], plot2[i]);

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
		public static void PlotChartXY(Chart chart, FloatArray plot1, FloatArray plot2, float minX, float maxX, float minY, float maxY)
		{
			//init series
			Series series1 = new Series("plot1");

			//setType
			series1.ChartType = SeriesChartType.Point;

			for (int i = 0; i < plot2.Length; i++)
				series1.Points.AddXY(plot1[i], plot2[i]);

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
		public static void PlotChartXY(Chart chart, IplImage plot1, IplImage plot2, float minX, float maxX, float minY, float maxY)
		{
			//init series
			Series series1 = new Series("plot1");

			//setType
			series1.ChartType = SeriesChartType.Point;

			//int L = plot1.Width * plot1.Height;

			for (int i = 0; i < plot1.Height; i++)
				for (int j = 0; j < plot1.Width;j++ )
					series1.Points.AddXY(plot1[i,j].Val0, plot2[i,j].Val0);

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
		public static void PlotChartXY(Chart chart, float[,] plot1, float[,] plot2, float minX, float maxX, float minY, float maxY)
		{
			//init series
			Series series1 = new Series("plot1");

			//setType
			series1.ChartType = SeriesChartType.Point;

			//int L = plot1.Width * plot1.Height;

			for (int i = 0; i < plot1.GetLength(0); i++)
				for (int j = 0; j < plot1.GetLength(1); j++)
					series1.Points.AddXY(plot1[i, j], plot2[i, j]);

			//setValueLabel
			series1.IsValueShownAsLabel = false;
            series1.IsVisibleInLegend = false;

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


//			ca.AxisX.LabelStyle.IsEndLabelVisible = false;
//			ca.AxisY.LabelStyle.Enabled = false;
            ca.AxisX.LabelStyle.Format = "F2";
            ca.AxisY.LabelStyle.Format = "F2";
            ca.AxisX.Title="Measured";
            ca.AxisY.Title = "Estimated";
			

			//add series in Chart
			chart.Series.Clear();
			chart.Series.Add(series1);

			//add chartArea in Chart
			chart.ChartAreas.Clear();
			chart.ChartAreas.Add(ca);	

		}

        /// <summary>
        /// 折れ線をプロット
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="plot1"></param>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="minY"></param>
        /// <param name="maxY"></param>
        public static void PlotChart(Chart chart, float[] plot1, float minX, float maxX, float minY, float maxY)
        {
            // auto scaling
            if (minX >= maxX)
            {
                minX = 0.0f;
                maxX = (float)plot1.Length;
            }
            if (minY >= maxY)
            {
                minY = plot1[0];
                maxY = plot1[0];
                for (int i = 1; i < plot1.Length; i++)
                {
                    if (minY > plot1[i]) minY = plot1[i];
                    if (maxY < plot1[i]) maxY = plot1[i];
                }
            }
            //init series
            Series series1 = new Series("plot1");

            //setType
            series1.ChartType = SeriesChartType.Line;

            //setValue
            for (int i = 0; i < plot1.Length; i++)
                series1.Points.AddXY(i, plot1[i]);


            //setValueLabel
            series1.IsValueShownAsLabel = false;

            //add
            //            series1.IsVisibleInLegend = false;
            //            series2.IsVisibleInLegend = false;

            //setPointsize
            series1.MarkerSize = 3;

            //setMarkerType
            series1.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;


            //setMaxAndMin
            ChartArea ca = new ChartArea();
            ca.AxisX.Maximum = maxX;
            ca.AxisX.Minimum = minX;
            ca.AxisY.Maximum = maxY;
            ca.AxisY.Minimum = minY;
            ca.AxisX.LabelStyle.Format = "F2";
            ca.AxisY.LabelStyle.Format = "F2";
            ca.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            ca.AxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;

            //add series in Chart
            chart.Series.Clear();
            chart.Series.Add(series1);

            //add chartArea in Chart
            chart.ChartAreas.Clear();
            chart.ChartAreas.Add(ca);
        }




        /// <summary>
        /// 二本の折れ線をプロット
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="plot1"></param>
        /// <param name="plot2"></param>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="minY"></param>
        /// <param name="maxY"></param>
		public static void PlotChart(Chart chart, float[] plot1, float[] plot2, float minX, float maxX, float minY, float maxY)
		{
            // auto scaling
            if (minX >= maxX)
            {
                minX = 0.0f;
                if(plot1.Length>plot2.Length)  maxX = (float)plot1.Length;
                else maxX = (float)plot2.Length;
            }
            if (minY >= maxY)
            {
                minY = plot1[0];
                maxY = plot1[0];
                for (int i = 1; i < plot1.Length; i++)
                {
                    if (minY > plot1[i]) minY = plot1[i];
                    if (maxY < plot1[i]) maxY = plot1[i];
                }
                for (int i = 1; i < plot2.Length; i++)
                {
                    if (minY > plot2[i]) minY = plot2[i];
                    if (maxY < plot2[i]) maxY = plot2[i];
                }
            }
            //init series
			Series series1 = new Series("plot1");
			Series series2 = new Series("plot2");

			//setType
			series1.ChartType = SeriesChartType.Line;
			series2.ChartType = SeriesChartType.Line;

			//setValue
			for (int i = 0; i < plot1.Length; i++)
				series1.Points.AddXY(i, plot1[i]);

			for (int i = 0; i < plot2.Length; i++)
				series2.Points.AddXY(i, plot2[i]);

			//setValueLabel
			series1.IsValueShownAsLabel = false;
			series2.IsValueShownAsLabel = false;

			//add
//            series1.IsVisibleInLegend = false;
//            series2.IsVisibleInLegend = false;

			//setPointsize
			series1.MarkerSize = 3;
			series2.MarkerSize = 3;

			//setMarkerType
			series1.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
			series2.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Cross;
            

			//setMaxAndMin
			ChartArea ca = new ChartArea();
			ca.AxisX.Maximum = maxX;
			ca.AxisX.Minimum = minX;
			ca.AxisY.Maximum = maxY;
			ca.AxisY.Minimum = minY;
            ca.AxisX.LabelStyle.Format = "F2";
            ca.AxisY.LabelStyle.Format = "F2";

			//add series in Chart
			chart.Series.Clear();
			chart.Series.Add(series1);
			chart.Series.Add(series2);

			//add chartArea in Chart
			chart.ChartAreas.Clear();
			chart.ChartAreas.Add(ca);
		}


        /// <summary>
        /// 三本以上の折れ線をプロット
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="minX">x軸の最小値</param>
        /// <param name="maxX">x軸の最大値</param>
        /// <param name="minY">y軸の最小値</param>
        /// <param name="minY">y軸の最大値</param>
        /// <param name="plots">3つ以上のfloat配列をobjectにキャストして渡す</param>
        public static void PlotChart(Chart chart, float minX, float maxX, float minY, float maxY, params object[] plots)
        {
            // auto scaling
            //float MaxValue=float.MinValue, MinValue=float.MaxValue;
            List<float[]> valueList = new List<float[]>();

            // auto scaling
            if (minX >= maxX)
            {
                minX = 0.0f;
                if (plots.Length > 3)
                {
                    foreach (var dataArray in plots)
                    {
                        var data = dataArray as float[];
                        valueList.Add(data);
                        if (data.Length > maxX)
                        {
                            maxX = data.Length;
                        }
                    }
                }
            }
            else
            {
                foreach (var dataArray in plots)
                {
                    var data = dataArray as float[];
                    valueList.Add(data);
                }
            }
            if (minY >= maxY)
            {
                minY = ((float[])plots[0])[0];
                maxY = ((float[])plots[0])[0];

                foreach (var dataArray in plots)
                {
                    var data = dataArray as float[];
                    foreach (float i in data)
                    {
                        if (minY > i) minY = i;
                        if (maxY < i) maxY = i;

                    }

                }
            }

            //init series
            Series[] series = new Series[valueList.Count];

            for (int i = 0; i < series.Length; i++)
            {
                series[i] = new Series("plot" + (i+1).ToString());
                series[i].ChartType = SeriesChartType.Line;

                for (int j = 0; j < valueList[i].Length; j++)
                {
                    //Console.WriteLine(@"value: " + valueList[i][j].ToString() + @", i:" + i.ToString() + @", j:" + j.ToString());
                    series[i].Points.AddXY(j, valueList[i][j]);
                }
                
                series[i].IsValueShownAsLabel = false;
                series[i].MarkerSize = 3;
                series[i].MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;

            }


            //setMaxAndMin
            ChartArea ca = new ChartArea();
            ca.AxisX.Maximum = maxX;
            ca.AxisX.Minimum = minX;
            ca.AxisY.Maximum = maxY;
            ca.AxisY.Minimum = minY;
            ca.AxisX.LabelStyle.Format = "F2";
            ca.AxisY.LabelStyle.Format = "F2";

            //add series in Chart
            chart.Series.Clear();

            for (int i = 0; i < series.Length; i++)
            {
                chart.Series.Add(series[i]);
            }

            //add chartArea in Chart
            chart.ChartAreas.Clear();
            chart.ChartAreas.Add(ca);
        }

	}
}
