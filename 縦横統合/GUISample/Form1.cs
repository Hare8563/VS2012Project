using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Extensions;
using ShoNS.Array;
using System.Runtime.InteropServices;
using LabImg;

namespace GUISample
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		System.Drawing.Point p1, startPoint, endPoint;
		IplImage Rimg, Limg, Upimg, Lowimg, RblockImg, LblockImg, UpblocImg, LowblockImg, InblockImg;
        IplImage depthHorizontal, depthVertical,depthDP,depth2CDP;
        String RightImageLink;
        String LeftImageLink;
        String UpImageLink;
        String LowImageLink;
        Boolean RightEyesLoaded = false;
        Boolean UpEyesLoaded = false;

        Bitmap bmp1, bmp2,bmp3,bmp4;
        Boolean renderedFlag = false, drawing=false;
        float[,] xtionData;

		private void Form1_Load(object sender, EventArgs e)
		{
			label1.Text = @"右画像";
			label2.Text = @"左画像";
            label12.Text = @"上画像(左)";
            label13.Text = @"下画像(右)";
			string dir = Environment.CurrentDirectory;

            //必要なとき自動的にフォームにスクロールバーが表示されるようにする。
            //フォームに表示されない位置にコントロールがあるとき、
            //そのコントロールが表示されるようにスクロールバーが表示される。
            this.AutoScroll = true;
            //AutoScrollMarginを指定するとマージンのサイズを設定できる。
            this.AutoScrollMargin = new System.Drawing.Size(10, 10);
            //AutoScrollMinSizeを指定すると
            //スクロールバーを表示する最小サイズを設定できる。
            this.AutoScrollMinSize = new System.Drawing.Size(100, 100);
            //AutoScrollPositionを指定すると位置を設定できる。
            this.AutoScrollPosition = new System.Drawing.Point(-50, 50);

            using(StreamReader sr = new StreamReader(dir+@"\depthMap1.csv", System.Text.Encoding.Default))
            {
                string str="";
                List<string> arrText=new List<string>();

                while (str != null)
                {
                    str = sr.ReadLine();
                    if (str != null)
                    {
                        arrText.Add(str);
                    }

                }
                int line_count = arrText.Count;
                string temp = (string)arrText[0];
                string[] temp2 = temp.Split(',');
                int col_count = temp2.Length;
                xtionData = new float[line_count, col_count];
                int a = 0, b = 0;
                foreach (string sOut in arrText)
                {
                    string[] temp_line = sOut.Split(',');
                    foreach (string value in temp_line)
                    {
                        if (value != "")
                        {
                            xtionData[a, b] = float.Parse(value);
                            b++;
                        }
                    }
                    b = 0;
                    a++;
                }

                sr.Close();
            }
            //LoadImg();
		}

        /*
        private void LoadImg()
        {
            if (radioButton1.Checked == true)
            {
                Rimg = new IplImage(RightImageLink);
                Limg = new IplImage(LeftImageLink);
                if (Rimg.Width >= 640)
                {
                    Rimg = kaneUtility.PyramidDown(Rimg);
                    Limg = kaneUtility.PyramidDown(Limg);
                    
                    /*
                    上を左に、下を右に
                    */                    
                   //Rimg = kaneUtility.imageRotation270(Rimg);
                   //Limg = kaneUtility.imageRotation270(Limg);
                    
                    /*
                    上を右に、下を左に
                     /
                    //IplImage tmp = Rimg;
                    //Rimg = kaneUtility.imageRotation90(Limg);
                    //Limg = kaneUtility.imageRotation90(tmp);
                }
            }
            else
            {
                float gamma = float.Parse(textBox7.Text);
                Limg = kaneUtility.grayImageRead(LeftImageLink, gamma);
                Rimg = kaneUtility.grayImageRead(RightImageLink, gamma);
                if (Rimg.Width >= 640)
                {
                    Rimg = kaneUtility.PyramidDown(Rimg);
                    Limg = kaneUtility.PyramidDown(Limg);

                    //Rimg = kaneUtility.imageRotation90(Rimg);
                    //Limg = kaneUtility.imageRotation90(Limg);
                }
            }
            //            pictureBox3.Image = Bitmap.FromFile(textBox1.Text);
            CommonUtility.FillPicBox(Rimg.ToBitmap(), pictureBox1);
            CommonUtility.FillPicBox(Limg.ToBitmap(), pictureBox2);
        }*/

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "カメラパラメータファイル(*.yml)|*.yml|すべてのファイル(*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Title = "参照ファイルを選択してください";
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = ofd.FileName;
            }
        }
		private void rightEyes_Down(object sender, MouseEventArgs e)
		{
            if (e.Button == MouseButtons.Right)
            {
                //右クリックが押されたときの処理
                if (renderedFlag == false && pictureBox1.Image != null && pictureBox2.Image != null)
                {
                    bmp1 = (Bitmap)pictureBox1.Image.Clone();
                    bmp2 = (Bitmap)pictureBox2.Image.Clone();

                    bmp3 = (Bitmap)pictureBox7.Image.Clone();
                    bmp4 = (Bitmap)pictureBox8.Image.Clone();

                    /*
                    startPoint = e.Location;
                    drawing = true;*/

                    pictureBox1.Image = bmp1;
                    pictureBox2.Image = bmp2;

                    Bitmap canvas = (Bitmap)pictureBox1.Image.Clone();
                    Bitmap canvas2 = (Bitmap)pictureBox2.Image.Clone();


                    Bitmap canvas3 = (Bitmap)pictureBox7.Image.Clone();
                    Bitmap canvas4 = (Bitmap)pictureBox8.Image.Clone();

                    Graphics g = Graphics.FromImage(canvas);
                    Graphics g2 = Graphics.FromImage(canvas2);

                    Graphics g3 = Graphics.FromImage(canvas3);
                    Graphics g4 = Graphics.FromImage(canvas4);

                    Pen pen = new Pen(Color.Yellow);
                    startPoint = e.Location;
                    endPoint = e.Location;
                    int HalfWidthSize = 16;// bmp1.Size.Width / 4;
                    int HalfHeightSize = 16;// bmp1.Size.Height / 4;
                    //g.DrawRectangle(pen, startPoint.X-HalfWidthSize, startPoint.Y-HalfHeightSize, 2*HalfWidthSize+1, 2*HalfHeightSize+1);
                    //g2.DrawRectangle(pen, startPoint.X - HalfWidthSize, startPoint.Y - HalfHeightSize, 2 * HalfWidthSize+1, 2 * HalfHeightSize+1);
                    //g3.DrawRectangle(pen, (bmp1.Height - startPoint.Y) - HalfHeightSize, startPoint.X - HalfWidthSize, 2 * HalfHeightSize+1, 2 * HalfWidthSize+1);
                    //g4.DrawRectangle(pen, (bmp1.Height - startPoint.Y) - HalfHeightSize, startPoint.X - HalfWidthSize, 2 * HalfHeightSize+1, 2 * HalfWidthSize+1);

                    g.DrawRectangle(pen, startPoint.X-30, startPoint.Y-30, 2*30+1, 2*30+1);
                    g2.DrawRectangle(pen, startPoint.X - 30, startPoint.Y -30, 2 * 30+1, 2 *30+1);
                    g3.DrawRectangle(pen, (bmp1.Height - startPoint.Y) - 30, startPoint.X - 30, 2 * 30 + 1, 2 * 30 + 1);
                    g4.DrawRectangle(pen, (bmp1.Height - startPoint.Y) - 30, startPoint.X - 30, 2 * 30 + 1, 2 * 30 + 1);

                    g.Dispose();
                    g2.Dispose();
                    g3.Dispose();
                    g4.Dispose();
                    pictureBox1.Image = canvas;
                    pictureBox2.Image = canvas2;
                    pictureBox7.Image = canvas3;
                    pictureBox8.Image = canvas4;

                    renderedFlag = true;
/*
                    RblockImg = (kaneUtility.getRect(Rimg, startPoint.X - HalfWidthSize, startPoint.Y - HalfHeightSize, 2 * HalfWidthSize+1, 2 * HalfHeightSize+1)).Clone();
                    LblockImg = (kaneUtility.getRect(Limg, startPoint.X - HalfWidthSize, startPoint.Y - HalfHeightSize, 2 * HalfWidthSize+1, 2 * HalfHeightSize+1)).Clone();
                    UpblocImg = (kaneUtility.getRect(Upimg, (bmp1.Height - startPoint.Y) - HalfHeightSize, startPoint.X - HalfWidthSize, 2 * HalfHeightSize+1, 2 * HalfWidthSize+1)).Clone();
                    LowblockImg = (kaneUtility.getRect(Lowimg, (bmp1.Height - startPoint.Y) - HalfHeightSize, startPoint.X - HalfWidthSize, 2 * HalfHeightSize+1, 2 * HalfWidthSize+1)).Clone();
                    InblockImg = (kaneUtility.getRect(Limg, startPoint.X - HalfWidthSize, startPoint.Y - HalfHeightSize, 2 * HalfWidthSize + 11, 2 * HalfHeightSize + 1)).Clone();
                    */
                    RblockImg = (kaneUtility.getRect(Rimg, startPoint.X - 30, startPoint.Y - 30, 2 * 30 + 1, 2 * 30 + 1)).Clone();
                    LblockImg = (kaneUtility.getRect(Limg, startPoint.X - 30, startPoint.Y - 30, 2 * 30 + 1, 2 * 30 + 1)).Clone();
                    UpblocImg = (kaneUtility.getRect(Upimg, (bmp1.Height - startPoint.Y) - 30, startPoint.X - 30, 2 * 30 + 1, 2 * 30 + 1)).Clone();
                    LowblockImg = (kaneUtility.getRect(Lowimg, (bmp1.Height - startPoint.Y) - 30, startPoint.X - 30, 2 * 30 + 1, 2 * 30 + 1)).Clone();
                    InblockImg = (kaneUtility.getRect(Limg, startPoint.X - HalfWidthSize, startPoint.Y - HalfHeightSize, 2 * HalfWidthSize + 11, 2 * HalfHeightSize + 1)).Clone();
                
                }
                else if (renderedFlag == true)//すでに書き込まれてる状態でクリックしたら消しますよ
                {
                    renderedFlag = false;
                    pictureBox1.Image = bmp1;
                    pictureBox2.Image = bmp2;
                    pictureBox7.Image = bmp3;
                    pictureBox8.Image = bmp4;
                }
            }
            else if (RightEyesLoaded)
            {
                //右以外のクリックで画像がすでにロードされているとき
                p1 = e.Location;
                System.Drawing.Point p2 = new System.Drawing.Point();
                p2.X = (int)((float)(p1.X / 320.0) * 640);
                p2.Y = (int)((float)(p1.Y / 240.0) * 480);
                label24.Text = @"X:" + p2.X + @", Y:" + p2.Y;
                depthBox.Text = xtionData[p2.Y, p2.X].ToString();
                Console.WriteLine(p2.Y.ToString() + "," + p2.X.ToString());
                CommonUtility.FillPicBox(Rimg.ToBitmap(), pictureBox1);
                CommonUtility.FillPicBox(Limg.ToBitmap(), pictureBox2);
                CommonUtility.FillPicBox(Upimg.ToBitmap(), pictureBox7);
                CommonUtility.FillPicBox(Lowimg.ToBitmap(), pictureBox8);


                Bitmap canvas = (Bitmap)pictureBox1.Image.Clone();
                Bitmap canvas2 = (Bitmap)pictureBox2.Image.Clone();

                Bitmap canvas3 = (Bitmap)pictureBox7.Image.Clone();
                Bitmap canvas4 = (Bitmap)pictureBox8.Image.Clone();


                Graphics g = Graphics.FromImage(canvas);
                Graphics g2 = Graphics.FromImage(canvas2);
                Graphics g3 = Graphics.FromImage(canvas3);
                Graphics g4 = Graphics.FromImage(canvas4);


                Pen pen = new Pen(Color.Red);
                int bh = int.Parse(textBox4.Text);
                int bh21 = 2 * bh + 1;

                g.DrawRectangle(pen, p1.X - bh, p1.Y - bh, bh21, bh21);
                g2.DrawRectangle(pen, p1.X - bh, p1.Y - bh, bh21, bh21);
                
                float rotateX = pictureBox1.Image.Height-p1.Y;
                float rotateY = p1.X;

                g3.DrawRectangle(pen, rotateX - bh, rotateY - bh, bh21, bh21);
                g4.DrawRectangle(pen, rotateX - bh, rotateY - bh, bh21, bh21);

                g.Dispose();
                g2.Dispose();
                g3.Dispose();
                g4.Dispose();

                pictureBox1.Image = canvas;
                pictureBox2.Image = canvas2;
                pictureBox7.Image = canvas3;
                pictureBox8.Image = canvas4;

            }

		}

		private void rightEyes_Move(object sender, MouseEventArgs e)
		{/*
            if (e.Button == MouseButtons.Right)
            {
                if (drawing == true &&
                     e.Location.X >= 0 && e.Location.X < bmp1.Width &&//画面端にポインタがいったときのため
                     e.Location.Y >= 0 && e.Location.Y < bmp1.Height//
                     )
                {
                    pictureBox1.Image = bmp1;
                    pictureBox2.Image = bmp2;

                    pictureBox7.Image = bmp3;
                    pictureBox8.Image = bmp4;

                    Bitmap canvas = (Bitmap)pictureBox1.Image.Clone();
                    Bitmap canvas2 = (Bitmap)pictureBox2.Image.Clone();


                    Bitmap canvas3 = (Bitmap)pictureBox7.Image.Clone();
                    Bitmap canvas4 = (Bitmap)pictureBox8.Image.Clone();

                    Graphics g = Graphics.FromImage(canvas);
                    Graphics g2 = Graphics.FromImage(canvas2);

                    Graphics g3 = Graphics.FromImage(canvas3);
                    Graphics g4 = Graphics.FromImage(canvas4);

                    Pen pen = new Pen(Color.Yellow);
                    endPoint = e.Location;

                    g.DrawRectangle(pen, startPoint.X, startPoint.Y, endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
                    g2.DrawRectangle(pen, startPoint.X, startPoint.Y, endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);

                    float rotateX = pictureBox1.Image.Height - startPoint.Y;
                    float rotateY = startPoint.X;
                    float rotateEndX=pictureBox1.Image.Height - endPoint.Y;
                    float rotateEndY=endPoint.X;

                    g3.DrawRectangle(pen, rotateEndX, rotateY, rotateX-rotateEndX, rotateEndY - rotateY);
                    g4.DrawRectangle(pen, rotateEndX, rotateY, rotateX - rotateEndX, rotateEndY - rotateY);

                    g.Dispose();
                    g2.Dispose();
                    g3.Dispose();
                    g4.Dispose();

                    pictureBox1.Image = canvas;
                    pictureBox2.Image = canvas2;
                    pictureBox7.Image = canvas3;
                    pictureBox8.Image = canvas4;

                }
            }*/

		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void rightEyes_Up(object sender, MouseEventArgs e)
		{/*
            if (e.Button == MouseButtons.Right)
            {
                if (drawing == true && e.Location != p1 &&
                  e.Location.X >= 0 && e.Location.X < bmp1.Width &&//画面端にポインタがいったときのため
                  e.Location.Y >= 0 && e.Location.Y < bmp1.Height//   
                  )
                {
                    drawing = false;
                    endPoint = e.Location;

                    pictureBox1.Image = bmp1;
                    pictureBox2.Image = bmp2;

                    Bitmap canvas = (Bitmap)pictureBox1.Image.Clone();
                    Bitmap canvas2 = (Bitmap)pictureBox2.Image.Clone();


                    Bitmap canvas3 = (Bitmap)pictureBox7.Image.Clone();
                    Bitmap canvas4 = (Bitmap)pictureBox8.Image.Clone();

                    Graphics g = Graphics.FromImage(canvas);
                    Graphics g2 = Graphics.FromImage(canvas2);

                    Graphics g3 = Graphics.FromImage(canvas3);
                    Graphics g4 = Graphics.FromImage(canvas4);

                    Pen pen = new Pen(Color.Yellow);

                    endPoint = e.Location;

                    g.DrawRectangle(pen, startPoint.X, startPoint.Y, endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
                    g2.DrawRectangle(pen, startPoint.X, startPoint.Y, endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);


                    float rotateX = pictureBox1.Image.Height - startPoint.Y;
                    float rotateY = startPoint.X;
                    float rotateEndX = pictureBox1.Image.Height - endPoint.Y;
                    float rotateEndY = endPoint.X;

                    g3.DrawRectangle(pen, rotateEndX, rotateY, rotateX - rotateEndX, rotateEndY - rotateY);
                    g4.DrawRectangle(pen, rotateEndX, rotateY, rotateX - rotateEndX, rotateEndY - rotateY);



                    g.Dispose();
                    g2.Dispose();
                    g3.Dispose();
                    g4.Dispose();
                    pictureBox1.Image = canvas;
                    pictureBox2.Image = canvas2;
                    pictureBox7.Image = canvas3;
                    pictureBox8.Image = canvas4;

                    renderedFlag = true;

                    RblockImg = (kaneUtility.getRectSE(Rimg, startPoint.X, startPoint.Y, endPoint.X, endPoint.Y)).Clone();
                    LblockImg = (kaneUtility.getRectSE(Limg, startPoint.X, startPoint.Y, endPoint.X, endPoint.Y)).Clone();
                    UpblocImg = (kaneUtility.getRect(Upimg, (int)rotateEndX, (int)rotateY, (int)(rotateX - rotateEndX)+1, (int)(rotateEndY - rotateY)+1)).Clone();
                    LowblockImg = (kaneUtility.getRect(Lowimg, (int)rotateEndX, (int)rotateY, (int)(rotateX - rotateEndX)+1, (int)(rotateEndY - rotateY)+1)).Clone();
                }
                else
                {
                    drawing = false;
                    renderedFlag = false;
                }
            }*/
		}
        

		private void button3_Click(object sender, EventArgs e)
		{

            //LoadImg();

            int blockHalf = int.Parse(textBox4.Text);
            int b21 = 2 * blockHalf + 1;
            int blockSearchRange = int.Parse(textBox5.Text);
            
            //横方向のXY
            int Xs = p1.X-blockHalf;
            int Ys = p1.Y-blockHalf;

            //縦方向のXY
            int Xr = (pictureBox1.Height - p1.Y)-blockHalf;
            int Yr = p1.X - blockHalf;

            float[] result = Disparity.HorizontalBlockMatch(Limg, Rimg, blockHalf, blockSearchRange, Xs,Ys);
            //13/10/30修正 LowimgとUpimgが逆になってた
            float[] resultRow = Disparity.HorizontalBlockMatch(Lowimg, Upimg, blockHalf, blockSearchRange, Xr, Yr);

            float minValue = result.Min();
            float maxValue = result.Max();

            float minValueRow = resultRow.Min();
            float maxValueRow = resultRow.Max();

            int minIndex = result.ToList().IndexOf(minValue);
            int minIndexRow = resultRow.ToList().IndexOf(minValueRow);

            LineDisparity.Text = minIndex.ToString();
            RowDisparity.Text = minIndexRow.ToString();

            CvMat Q = Disparity.ymlRead(textBox3.Text,0.5f);//横用のymlの読み込み
            CvMat Qr = Disparity.ymlRead(textBox1.Text, 0.5f);//縦用のyml
            
            //外部パラメータの補正
            float rz = float.Parse(LineRotateZBox.Text);
            float ry = float.Parse(LineRotateYBox.Text);

            float Rowrz = float.Parse(RowRotateZBox.Text);
            float Rowry = float.Parse(RowRotateYBox.Text);


            CvMat T = new CvMat(3, 1, MatrixType.F32C1);
            T[0, 0] = 0.0f;
            T[1, 0] = 0.0f;
            T[2, 0] = float.Parse(LineTransationBox.Text);


            CvMat RowT = new CvMat(3, 1, MatrixType.F32C1);
            T[0, 0] = 0.0f;
            T[1, 0] = 0.0f;
            T[2, 0] = float.Parse(RowTransationBox.Text);


            //横のカメラ座標、世界座標を算出
            CvMat cameraXYZ = Disparity.DisparityToDistance((float)Q[2,3],(float)Q[3,2],(int)Rimg.Width,(int)Rimg.Height,minIndex, p1.X, p1.Y);
           // CvMat cameraXYZ2 = Disparity.DisparityToDistance(Q, minIndex, p1.X, p1.Y);
            CvMat worldXYZ = Disparity.CameraToWorldXYZ(cameraXYZ, rz, ry, 0.0f, T);
            LineCX.Text = cameraXYZ[0, 0].ToString();
            LineCY.Text = cameraXYZ[1, 0].ToString();
            LineCZ.Text = cameraXYZ[2, 0].ToString();
            LineWX.Text = worldXYZ[0, 0].ToString();
            LineWY.Text = worldXYZ[1, 0].ToString();
            LineWZ.Text = worldXYZ[2, 0].ToString();
            

            //縦のカメラ座標、世界座標を算出
            CvMat RowcameraXYZ = Disparity.DisparityToDistance((float)Qr[2, 3], (float)Qr[3, 2], (int)Upimg.Width, (int)Upimg.Height, minIndexRow, Upimg.Width-p1.Y, p1.X);
            // CvMat cameraXYZ2 = Disparity.DisparityToDistance(Q, minIndex, p1.X, p1.Y);
            CvMat RowworldXYZ = Disparity.CameraToWorldXYZ(RowcameraXYZ, Rowrz, Rowry, 0.0f, RowT);
            RowCX.Text = RowcameraXYZ[0, 0].ToString();
            RowCY.Text = RowcameraXYZ[1, 0].ToString();
            RowCZ.Text = RowcameraXYZ[2, 0].ToString();
            RowWX.Text = RowworldXYZ[0, 0].ToString();
            RowWY.Text = RowworldXYZ[1, 0].ToString();
            RowWZ.Text = RowworldXYZ[2, 0].ToString();

            //横画像へ描画
            Bitmap canvas = (Bitmap)pictureBox2.Image.Clone();
            Graphics g = Graphics.FromImage(canvas);
            Pen gPen = new Pen(Color.Green, 3);
            Pen rPen = new Pen(Color.Red, 3);
            g.DrawRectangle(rPen, p1.X - blockHalf , p1.Y - blockHalf, b21, b21);
            g.DrawRectangle(gPen, p1.X - blockHalf + minIndex, p1.Y - blockHalf, b21, b21);
            g.Dispose();
            pictureBox2.Image = canvas;

            Bitmap canvas1 = (Bitmap)pictureBox1.Image.Clone();
            Graphics g1 = Graphics.FromImage(canvas1);
            g1.DrawRectangle(rPen, p1.X - blockHalf, p1.Y - blockHalf, b21, b21);
            g1.Dispose();
            pictureBox1.Image = canvas1;

            //縦画像へ描画
            Bitmap canvas3 = (Bitmap)pictureBox8.Image.Clone();
            Graphics g2 = Graphics.FromImage(canvas3);
            g2.DrawRectangle(rPen, (Upimg.Width - p1.Y) - blockHalf, p1.X - blockHalf, b21, b21);
            g2.DrawRectangle(gPen, (Upimg.Width-p1.Y) - blockHalf + minIndexRow, p1.X - blockHalf, b21, b21);
            g2.Dispose();
            pictureBox8.Image = canvas3;

            Bitmap canvas4 = (Bitmap)pictureBox7.Image.Clone();
            Graphics g3 = Graphics.FromImage(canvas4);
            g3.DrawRectangle(rPen, (Upimg.Width-p1.Y) - blockHalf, p1.X - blockHalf, b21, b21);
            g3.Dispose();
            pictureBox7.Image = canvas4;


            IplImage Rblock = kaneUtility.getRect(Rimg, p1.X - blockHalf, p1.Y - blockHalf, b21, b21);
            IplImage Lblock = kaneUtility.getRect(Upimg, Xr, Yr, b21, b21);

            Lblock = kaneUtility.imageRotation270(Lblock);
            //チャートダイアログを表示
            //ChartForm cform = new ChartForm(result, resultRow, 0.0f, blockSearchRange, 0.0f, maxValue,Rblock.ToBitmap(), Lblock.ToBitmap());
            ChartForm cform = new ChartForm(result,resultRow,0.0f, blockSearchRange, 0.0f, maxValue, Rblock, Lblock);

            cform.ShowDialog();
           // ChartUtility.PlotChart(chart1, result,  0.0f, blockSearchRange,0.0f,maxValue);

            //   CvMat gray=kaneUtility.IplImageFloatToCvMat(Rimg);
            //   kaneUtility.SetDisparity(gray, 0.0f, 255.0f, pictureBox3, pictureBox4, textBox8, textBox9);
 
            
            
            //CvMat Rg = kaneUtility.IplImageToCvMat(Rblock);
            //CvMat Lg = kaneUtility.IplImageToCvMat(Lblock);
            //kaneUtility.SetDisparity(Rg, 0.0f, 255.0f, pictureBox3, pictureBox4, textBox8, textBox9);
            //kaneUtility.SetDisparity(Lg, 0.0f, 255.0f, pictureBox6, pictureBox5, textBox20, textBox19);
        }

		private void pictureBox4_Click(object sender, EventArgs e)
		{

		}
		private void pictureBox6_Click(object sender, EventArgs e)
		{

		}

		private void label4_Click(object sender, EventArgs e)
		{

		}

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            int picWidth, picHeight;

            picWidth = pictureBox1.Width;
            picHeight = pictureBox1.Height;

            pictureBox1.Width = picHeight;
            pictureBox2.Width = picHeight;
            pictureBox1.Height = picWidth;
            pictureBox2.Height = picWidth;

            Rimg = kaneUtility.imageRotation270(Rimg);
            Limg = kaneUtility.imageRotation270(Limg);

            CommonUtility.FillPicBox(Rimg.ToBitmap(), pictureBox1);
            CommonUtility.FillPicBox(Limg.ToBitmap(), pictureBox2);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            IplImage tmpImage = Rimg;
            Rimg = Limg;
            Limg = tmpImage;

            CommonUtility.FillPicBox(Rimg.ToBitmap(), pictureBox1);
            CommonUtility.FillPicBox(Limg.ToBitmap(), pictureBox2);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            int picWidth, picHeight;

            picWidth = pictureBox1.Width;
            picHeight = pictureBox1.Height;

            pictureBox1.Width = picHeight;
            pictureBox2.Width = picHeight;
            pictureBox1.Height = picWidth;
            pictureBox2.Height = picWidth;

            Rimg = kaneUtility.imageRotation90(Rimg);
            Limg = kaneUtility.imageRotation90(Limg);

            CommonUtility.FillPicBox(Rimg.ToBitmap(), pictureBox1);
            CommonUtility.FillPicBox(Limg.ToBitmap(), pictureBox2);
        }

        private void RightImageCliked(object sender, EventArgs e)
        {
            if (RightEyesLoaded == true) return;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "画像ファイル(*.bmp;*.jpg;*.png;*.gif;*.ppm)|*.jpg;*.bmp;*.png;*.gif;*.ppm|すべてのファイル(*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Title = "入力画像ファイルを選択してください";
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.FileName = "";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                RightImageLink = ofd.FileName;
                if (radioButton2.Checked == true)
                {
                    float gamma = float.Parse(textBox7.Text);
                    Rimg = kaneUtility.grayImageRead(RightImageLink, gamma);
                }
                else
                {
                    Rimg = new IplImage(RightImageLink);
                }

                if (Rimg.Width >= 640)
                {
                    Rimg = kaneUtility.PyramidDown(Rimg);
                }

                CommonUtility.FillPicBox(Rimg.ToBitmap(), pictureBox1);
                RightEyesLoaded = true;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void LeftImageClicked(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "画像ファイル(*.bmp;*.jpg;*.png;*.gif;*.ppm)|*.jpg;*.bmp;*.png;*.gif;*.ppm|すべてのファイル(*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Title = "参照画像ファイルを選択してください";
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LeftImageLink = ofd.FileName;
                if (radioButton2.Checked == true)
                {
                    float gamma = float.Parse(textBox7.Text);
                    Limg = kaneUtility.grayImageRead(LeftImageLink, gamma);
                }
                else
                {
                    Limg = new IplImage(LeftImageLink);
                }

                if (Limg.Width >= 640)
                {
                    Limg = kaneUtility.PyramidDown(Limg);
                }

                CommonUtility.FillPicBox(Limg.ToBitmap(), pictureBox2);
                //pictureBox2.Image = Bitmap.FromFile(ofd.FileName);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "カメラパラメータファイル(*.yml)|*.yml|すべてのファイル(*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Title = "参照ファイルを選択してください";
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
            }
        }

        private void UpImageClicked(object sender, EventArgs e)
        {
            if (UpEyesLoaded == true) return;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "画像ファイル(*.bmp;*.jpg;*.png;*.gif;*.ppm)|*.jpg;*.bmp;*.png;*.gif;*.ppm|すべてのファイル(*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Title = "参照画像ファイルを選択してください";
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                UpImageLink = ofd.FileName;
                //pictureBox2.Image = Bitmap.FromFile(ofd.FileName); 
                if (radioButton2.Checked == true)
                {
                    float gamma = float.Parse(textBox7.Text);
                    Upimg = kaneUtility.grayImageRead(UpImageLink, gamma);
                }
                else
                {
                    Upimg = new IplImage(UpImageLink);
                }

                if (Upimg.Width >= 640)
                {
                    Upimg = kaneUtility.PyramidDown(Upimg);
                }
                Upimg=kaneUtility.imageRotation270(Upimg);
                CommonUtility.FillPicBox(Upimg.ToBitmap(), pictureBox7);
                UpEyesLoaded = true;
            }
        }

        private void LowImageClicked(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "画像ファイル(*.bmp;*.jpg;*.png;*.gif;*.ppm)|*.jpg;*.bmp;*.png;*.gif;*.ppm|すべてのファイル(*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Title = "参照画像ファイルを選択してください";
            ofd.RestoreDirectory = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LowImageLink = ofd.FileName;
                //pictureBox2.Image = Bitmap.FromFile(ofd.FileName); 
                if (radioButton2.Checked == true)
                {
                    float gamma = float.Parse(textBox7.Text);
                    Lowimg = kaneUtility.grayImageRead(LowImageLink, gamma);
                }
                else
                {
                    Lowimg = new IplImage(LowImageLink);
                }

                if (Lowimg.Width >= 640)
                {
                    Lowimg = kaneUtility.PyramidDown(Lowimg);
                }
                Lowimg = kaneUtility.imageRotation270(Lowimg);
                CommonUtility.FillPicBox(Lowimg.ToBitmap(), pictureBox8);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            HVCompare hv = new HVCompare(Rimg, kaneUtility.imageRotation270(Upimg));
            hv.ShowDialog();  
        }


        private void button2_Click(object sender, EventArgs e)
        {
            IplImage grayR = new IplImage(Rimg.Width, Rimg.Height, BitDepth.U8, 1);
            IplImage grayL = new IplImage(Limg.Width, Rimg.Height, BitDepth.U8, 1);

            Cv.CvtColor(Rimg,grayR,ColorConversion.BgrToGray);
            Cv.CvtColor(Limg,grayL,ColorConversion.BgrToGray);

            int SearchRange = int.Parse(textBox5.Text);
            if(SearchRange%16!=0){
                SearchRange -= SearchRange%16;
                textBox5.Text = SearchRange.ToString();
                MessageBox.Show(@"探索範囲が16の倍数でないためもっとも近い値でブロックマッチを行います");
            }

            IplImage disp = Disparity.BlockMatch(grayL, grayR, int.Parse(textBox4.Text), SearchRange);
            IplImage depth = DisparityToDepthMap(disp, textBox3.Text);
            depthHorizontal = depth.Clone();
            DepthMap depthWindow = new DepthMap(depth);
            depthWindow.ShowDialog();
            //Cv.ShowImage("Horizontal Disparity",depth);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            IplImage grayLow = new IplImage(Lowimg.Width, Lowimg.Height, BitDepth.U8, 1);
            IplImage grayUp = new IplImage(Upimg.Width, Upimg.Height, BitDepth.U8, 1);

            Cv.CvtColor(Lowimg, grayLow, ColorConversion.BgrToGray);
            Cv.CvtColor(Upimg, grayUp, ColorConversion.BgrToGray);

            int SearchRange = int.Parse(textBox5.Text);
            if (SearchRange % 16 != 0)
            {
                SearchRange -= SearchRange % 16;
                textBox5.Text = SearchRange.ToString();
                MessageBox.Show(@"探索範囲が16の倍数でないためもっとも近い値でブロックマッチを行います");
            }

            IplImage disp = Disparity.BlockMatch(grayLow, grayUp, int.Parse(textBox4.Text), SearchRange);
            IplImage depth = DisparityToDepthMap(disp, textBox1.Text);
            depthVertical = depth.Clone();
            DepthMap depthWindow = new DepthMap(kaneUtility.imageRotation270(depth));
            depthWindow.ShowDialog();

            //Cv.ShowImage("Vertical Disparity", kaneUtility.imageRotation270(disp));
        }


        private void button10_Click(object sender, EventArgs e)
        {
            if (renderedFlag != false)
            {
                IplImage horizontalBlock = simpleBlockMatching(LblockImg, RblockImg);
                IplImage verticalBlock = simpleBlockMatching(LowblockImg, UpblocImg);

                depthCompare depthWindow = new depthCompare(horizontalBlock, verticalBlock,RblockImg);
                depthWindow.ShowDialog();
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (renderedFlag == true)
            {
                int SearchRange = int.Parse(textBox5.Text);
                IplImage cuttedR = kaneUtility.getRectSE(Rimg, startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
                if (endPoint.X + SearchRange >= Limg.Size.Width)
                {
                    SearchRange = Limg.Size.Width - endPoint.X;
                }
                IplImage cuttedL = kaneUtility.getRectSE(Limg, startPoint.X, startPoint.Y - 10, endPoint.X + SearchRange, endPoint.Y + 10);

                IplImage DPimg = new IplImage(cuttedR.Width, cuttedR.Height, BitDepth.U8, 1);
                DPimg=DPMatching(cuttedL, cuttedR);
                depthDP = DPimg;
                DepthMap depthMap = new DepthMap(DPimg);
                depthMap.ShowDialog();
            }
        }




        /// <summary>
        /// Disparity画像からDepthMap画像を生成する
        /// </summary>
        /// <param name="img">Disparity画像</param>
        /// <param name="ymlPath">ymlファイルのパス</param>
        /// <returns>Depth情報を格納したIplImage</returns>
        public static IplImage DisparityToDepthMap(IplImage img, String ymlPath)
        {
            IplImage returnImage = new IplImage(img.Width, img.Height, BitDepth.F32, 1);
            CvMat Q = Disparity.ymlRead(ymlPath, 0.5f);//横用のymlの読み込み
            unsafe
            {
                float* returnImagePtr = (float*)returnImage.ImageData;
                byte* imgPtr = (byte*)img.ImageData;

                for (int y = 0; y < img.Height; y++)
                {
                    for (int x = 0; x < img.Width; x++)
                    {
                        //int offsetImg = x + y * img.Width;
                        float disp = (float)img[y, x].Val0;
                        if (disp != 0)
                        {


                                    //CvMat cameraXYZ = Disparity.DisparityToDistance((float)Q[2, 3], (float)Q[3, 2], (int)img.Width, (int)img.Height, disp, x, y);
                                    CvMat cameraXYZ = Disparity.DisparityToDistance(Q, disp, x, y);
                                    returnImage[y, x] = cameraXYZ[2, 0];


                        }
                        else
                        {
                            returnImage[y, x] = 0;
                        }

                    }
                }

            }
            return returnImage;
        }


        private void button12_Click(object sender, EventArgs e)
        {
            int hb = int.Parse(textBox4.Text);

            if (renderedFlag == true)
            {
                
                IplImage CDP = TwoDCDPMatching(LblockImg.Clone(), RblockImg.Clone());
                IplImage DP = DPMatching(RblockImg, LblockImg);
                IplImage horizontalDisp = simpleBlockMatching(LblockImg, RblockImg);
                IplImage verticalDisp = simpleBlockMatching(LowblockImg, UpblocImg);

                //IplImage horizontalDepth = (kaneUtility.getRectSE(horizontalBlock, startPoint.X, startPoint.Y, endPoint.X, endPoint.Y)).Clone();
                //IplImage verticalDepth = kaneUtility.getRectSE(kaneUtility.imageRotation270(verticalBlock), startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
                IplImage depthHorizontal = DisparityToDepthMap(horizontalDisp, textBox3.Text);


                for (int y = 0; y < depthHorizontal.Height; y++)
                {
                    for (int x = 0; x < depthHorizontal.Width; x++)
                    {
                        depthHorizontal[y, x] = depthHorizontal[y, x].Val0 * 16;
                    }
                }

                IplImage depthVertical = DisparityToDepthMap(verticalDisp, textBox3.Text);
                for (int y = 0; y < depthVertical.Height; y++)
                {
                    for (int x = 0; x < depthVertical.Width; x++)
                    {
                        depthVertical[y, x] = depthVertical[y, x].Val0 * 16;
                    }
                }

                
                IplImage horizontalResult = getRect(depthHorizontal, (depthHorizontal.Width / 2) - hb, hb, (depthHorizontal.Width / 2) - hb, depthHorizontal.Height - (2 * hb));
                IplImage verticalResult = getRect(depthVertical, hb, (depthVertical.Height / 2) - hb, horizontalResult.Height, horizontalResult.Width);
                IplImage DPResult = getRect(DP, (DP.Width /2) - hb, hb, (DP.Width / 2) - hb, DP.Height - (2 * hb));
                IplImage CDPResult = getRect(CDP, (CDP.Width / 2) - hb, hb, (CDP.Width / 2) - hb, CDP.Height - (2 * hb));

                IplImage baseR = getRect(RblockImg, (RblockImg.Width / 2) - hb, hb, (RblockImg.Width / 2) - hb, RblockImg.Height - (2 * hb));
                IplImage baseUp = getRect(UpblocImg, hb, (UpblocImg.Height / 2)-hb, horizontalResult.Height, horizontalResult.Width);
                IplImage baseL = getRect(LblockImg, (LblockImg.Width / 2) - hb, hb, (LblockImg.Width / 2) - hb, LblockImg.Height - (2 * hb));
                IplImage baseLow = getRect(LowblockImg, hb, (LowblockImg.Height / 2) - hb, horizontalResult.Height, horizontalResult.Width);


                MatchingCompare comp = new MatchingCompare(horizontalResult, verticalResult, DPResult, CDPResult, baseR, baseUp, baseL, baseLow);
                //MatchingCompare comp = new MatchingCompare(depthHorizontal, depthVertical, DP, CDP, RblockImg, UpblocImg);
                comp.Show();
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (renderedFlag == true)
            {
                int SearchRange = int.Parse(textBox5.Text);
                IplImage cuttedR = kaneUtility.getRectSE(Rimg, startPoint.X, startPoint.Y, endPoint.X, endPoint.Y);
                if (endPoint.X + SearchRange >= Limg.Size.Width)
                {
                    SearchRange = Limg.Size.Width - endPoint.X;
                }
                IplImage cuttedL = kaneUtility.getRectSE(Limg, startPoint.X, startPoint.Y - 10, endPoint.X + SearchRange, endPoint.Y + 10);

                IplImage CDPimg = new IplImage(cuttedR.Width, cuttedR.Height, BitDepth.U8, 1);
                CDPimg = TwoDCDPMatching(cuttedL, cuttedR);
                depth2CDP = CDPimg;
                DepthMap depthMap = new DepthMap(CDPimg);
                depthMap.ShowDialog();
            }
        }
        
        private IplImage simpleBlockMatching(IplImage left,IplImage right)
        {
            IplImage grayR = new IplImage(right.Width, right.Height, BitDepth.U8, 1);
            IplImage grayL = new IplImage(left.Width, left.Height, BitDepth.U8, 1);

            Cv.CvtColor(right, grayR, ColorConversion.BgrToGray);
            Cv.CvtColor(left, grayL, ColorConversion.BgrToGray);

            int SearchRange = int.Parse(textBox5.Text);
            int halfBlockSize = int.Parse(textBox4.Text);
            
           if (SearchRange % 16 != 0)
            {
                SearchRange -= SearchRange % 16;
                textBox5.Text = SearchRange.ToString();
            }

            IplImage disp = Disparity.BlockMatch(grayL, grayR, halfBlockSize, SearchRange);
            return disp;
            /*
            IplImage depth = DisparityToDepthMap(disp, textBox3.Text);


            for (int y = 0; y < depth.Height; y++)
            {
                for(int x = 0; x < depth.Width; x++)
                {
                    depth[y, x] = depth[y,x].Val0*16;
                }
            }
                return depth;*/
        }

        private IplImage DPMatching(IplImage left, IplImage right)
        {

            IplImage dispMap = Disparity.KanemotoDPmatch(right, left, int.Parse(textBox4.Text), int.Parse(textBox5.Text));
            IplImage depth = DisparityToDepthMap(dispMap, textBox3.Text);
            return depth;    
        }

        private IplImage TwoDCDPMatching(IplImage left, IplImage right)
        {
                int SearchRange = int.Parse(textBox5.Text);

                int[] disparityX = new int[right.Width * right.Height];
                int[] disparityY = new int[right.Width * right.Height];

                unsafe
                {
                    fixed (int* ptrX = disparityX)
                    fixed (int* ptrY = disparityY)
                    {
                        Disparity.twoDCDP((byte*)left.ImageData, left.Size.Height, left.Size.Width, left.WidthStep, (byte*)right.ImageData, right.Size.Height, right.Size.Width, right.WidthStep, ptrX, ptrY);
                    }
                }


                IplImage dispMap = new IplImage(right.Width, right.Height, BitDepth.U8, 1);
                for (int y = 0; y < right.Height; y++)
                {
                    for (int x = 0; x < right.Width; x++)
                    {
                        dispMap[y, x] = disparityX[y * right.Width + x];
                    }
                }

                IplImage depth = DisparityToDepthMap(cdpAvoidNoise(dispMap), textBox3.Text);

                return depth;
        }
        /// <summary>
        /// 矩形領域を抽出する。左上と右下座標を指定。IplImageは、Byte単位のRGBかGray
        /// </summary>
        /// <param name="img"></param>
        /// <param name="xs"></param>
        /// <param name="ys"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static IplImage getRect(IplImage img, int xs, int ys, int w, int h)
        {
            //           int w = xe - xs + 1;
            //           int h = ye - ys + 1;
            if (xs < 0) xs = 0;
            if ((xs + w - 1) >= img.Width) w = img.Width - xs;
            if (ys < 0) ys = 0;
            if ((ys + h - 1) >= img.Height) h = img.Height - ys;
            int nc = img.NChannels;
            IplImage rimg = new IplImage(w, h, img.Depth, nc);
            
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        rimg[y, x] = img[y + ys, x + xs]; 
                    }
                }
           
            return rimg;
        }

        private IplImage cdpAvoidNoise(IplImage cdpImage)
        {
            IplImage output=new IplImage(cdpImage.Width, cdpImage.Height, cdpImage.Depth, cdpImage.NChannels);
            int halfBlock = 3;


            for (int y = 0; y < cdpImage.Height; y++)
            {
                for(int x = 0; x < cdpImage.Width; x++)
                {
                    if (x - halfBlock < 0 || y - halfBlock < 0
                        || x + halfBlock >= cdpImage.Width || y + halfBlock >= cdpImage.Height)
                    {
                        output[y, x] = cdpImage[y, x];
                    }
                    else
                    {
                        double sum = 0;
                        int i = 0, j = 0, count=0;
                        for(i=y-halfBlock;i<y+halfBlock;i++){
                            for (j = x - halfBlock; j < x + halfBlock;j++)
                            {
                                sum += cdpImage[i, j].Val0;
                                count++;
                            }
                        }

                        sum /= count;

                        output[y, x] = sum;
                    }
                }
            }
                return output;
        }

    }
}