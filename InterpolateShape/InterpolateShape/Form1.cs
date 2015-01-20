using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using System.Drawing.Imaging;
using LabImg;

namespace InterpolateShape
{
    public partial class Form1 : Form
    {
        IplImage Rimg;
        IplImage Limg;
        IplImage BlockMatch;
        List<List<PVector>> vList = new List<List<PVector>>();
        List<PVector> pointList = new List<PVector>();
        Texture texture = new Texture();
    

        float PlaneMax;
        float PlaneMin;



        bool renderFlag = false;

        public Form1()
        {
            InitializeComponent();

            OpenGL gl = GLControl.OpenGL;
            gl.BlendFunc(SharpGL.Enumerations.BlendingSourceFactor.SourceAlpha, SharpGL.Enumerations.BlendingDestinationFactor.OneMinusSourceAlpha);
            gl.Enable(OpenGL.GL_BLEND);
           

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string path = Environment.CurrentDirectory;
            Rimg = new IplImage(path + @"\RectifyRight.bmp");
            Limg = new IplImage(path + @"\RectifyLeft.bmp");

            CommonUtility.FillPicBox(Rimg.ToBitmap(), RightImageBox);
            CommonUtility.FillPicBox(Limg.ToBitmap(), LeftImageBox);

            BlockMatch = Disparity.BlockMatch(Limg.toGray(), Rimg.toGray(), 7, 48);
            CreateColorBar(pictureBox1);

            //固定地点の世界座標を取得する
            pointList.Add(WorldPointFromImagePoint(420, 300));
            pointList.Add(WorldPointFromImagePoint(460, 300));
            pointList.Add(WorldPointFromImagePoint(420, 340));
            pointList.Add(WorldPointFromImagePoint(460, 340));

            OpenGL gl = GLControl.OpenGL;
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            texture.Create(gl, Rimg.ToBitmap());

            drawPicturePoint();
            //   OpenGL gl = GLControl.OpenGL;
         //   gl.Enable(OpenGL.GL_TEXTURE_2D);
         //   texture.Create(gl, "RectifyRight.bmp");
        }

        private void drawPicturePoint()
        {
            Bitmap canvas = (Bitmap)RightImageBox.Image.Clone();
            System.Drawing.Point p1 = new Point(210, 150);

            Graphics g = Graphics.FromImage(canvas);
            Pen rPen = new Pen(Color.Red, 3);
            g.DrawRectangle(rPen, new Rectangle(p1, new Size(40, 40)));

            CommonUtility.FillPicBox(canvas, RightImageBox);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            //指定幅で再描画
            PlaneMax = float.Parse(textBox2.Text);
            PlaneMin = float.Parse(textBox1.Text);

            renderFlag = true;
        }


        private float TripleMax(float val1, float val2, float val3)
        {
            if (val1 > val2)
            {
                if (val1 > val3) return val1;//val1 > val3 > val2 or val1 > val2 > val3
                else return val3;//val3 > val1 > val2
            }
            else
            {
                if (val2 > val3) return val2;//val2 > val3 > val1 or val2 > val1 > val3
                else return val3;//val3 > val2 > val1
            }
        }

        private float TripleMin(float val1, float val2, float val3)
        {
            if (val1 > val2)
            {
                if (val3 > val2) return val2;//val1 > val3 > val2 or val1 > val2 > val3
                else return val3;//val3 > val1 > val2
            }
            else
            {
                if (val3 > val1) return val1;//val2 > val3 > val1 or val2 > val1 > val3
                else return val3;//val3 > val2 > val1
            }
        }


        private float MultiMax(params float[] vals)
        {
            float Max = float.MinValue;
            foreach (float val in vals)
            {
                if (Max < val)
                {
                    Max = val;
                }
            }

            return Max;
        }

        private float MultiMin(params float[] vals)
        {
            float Min = float.MaxValue;

            foreach (float val in vals)
            {
                if (val < Min)
                {
                    Min = val;
                }
            }

            return Min;
        }

        private Point ImagePointFromWorldPoint(PVector v)
        {
            CvMat mat = new CvMat(3,1, MatrixType.F32C1);
            mat[0,0]　= v.X;
            mat[1,0] = v.Y;
            mat[2,0] = v.Z;
            CvMat transMat = new CvMat(3,1,MatrixType.F32C1);
            transMat[0,0] = 500;
            transMat[1,0] = -100;
            transMat[2,0] = -100;

            //世界座標からカメラ座標に戻す
            CvMat result = Disparity.CameraToWorldXYZ(mat, 0.0F, 88.0F, -90.0F, transMat);

            
            if (result[0, 0] == float.PositiveInfinity || result[0, 0] == float.NegativeInfinity || result[0, 0] == float.NaN)
            {
                result[0, 0] = 0;
            }
            if (result[1, 0] == float.PositiveInfinity || result[1, 0] == float.NegativeInfinity || result[1, 0] == float.NaN)
            {
                result[1, 0] = 0;
            }
            if (result[2, 0] == float.PositiveInfinity || result[2, 0] == float.NegativeInfinity || result[2, 0] == float.NaN)
            {
                result[2, 0] = 0;
            }
            float w = 6.1901241939439672e+002F / (float)result[2,0];
            Point p = new Point((int)(result[0,0]*w+3.3693316268920898e+002F), (int)(result[1,0]*w+ 2.5559978485107422e+002F));
            return p;
        }


        private PVector WorldPointFromImagePoint(int x, int y)
        {
            float disp = (float)BlockMatch[y, x].Val0;
            disp /= 16;
            if (disp == 0) disp = 0.0001F;
            float w = (float)(9.9896417441799987e-002F * disp);
            CvMat mat = new CvMat(3, 1, MatrixType.F32C1);
            CvMat transMat = new CvMat(3, 1, MatrixType.F32C1);
            mat[0, 0] = (x - 3.3693316268920898e+002F) / w;
            mat[1, 0] = (y - 2.5559978485107422e+002F) / w;
            mat[2, 0] = 6.1901241939439672e+002F / w;


            transMat[0, 0] = -500;//原点からカメラの位置まで(-500, 0, 100)動かさないといけない
            transMat[1, 0] = 100;
            transMat[2, 0] = 100;
            CvMat result = Disparity.CameraToWorldXYZ(mat, 90.0F, -88.0F, 0.0F, transMat);

            if (result[0, 0] == float.PositiveInfinity || result[0, 0] == float.NegativeInfinity || result[0, 0] == float.NaN)
            {
                result[0, 0] = 0;
            }
            if (result[1, 0] == float.PositiveInfinity || result[1, 0] == float.NegativeInfinity || result[1, 0] == float.NaN)
            {
                result[1, 0] = 0;
            }
            if (result[2, 0] == float.PositiveInfinity || result[2, 0] == float.NegativeInfinity || result[2, 0] == float.NaN)
            {
                result[2, 0] = 0;
            }






            PVector v = new PVector((float)result[0, 0], (float)result[1, 0], (float)result[2, 0]);

            return v;
        }


        //private void glDraw(object sender, RenderEventArgs args)
        //{

        //    OpenGL gl = GLControl.OpenGL;

        //    PVector p1 = WorldPointFromImagePoint(0, 0);
        //    PVector p2 = WorldPointFromImagePoint(Rimg.Width - 1, 0);
        //    PVector p3 = WorldPointFromImagePoint(0, Rimg.Height - 1);
        //    PVector p4 = WorldPointFromImagePoint(Rimg.Width - 1, Rimg.Height - 1);

        //    float dY = p2.Y - p1.Y;
        //    float dX = p3.X - p1.X;

        //    if (renderFlag == true)
        //    {
        //         gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
        //        gl.ClearColor(0.0F, .0F, .0F, 1.0F);
        //        gl.MatrixMode(SharpGL.Enumerations.MatrixMode.Projection);
        //        gl.LoadIdentity();
        //        if (TopViewButton.Checked)
        //        {
        //            gl.Ortho(-GLControl.Width * 1.5, GLControl.Width * 1.5, -GLControl.Height * 3, GLControl.Height * 5, 0.01, 10000);
        //            gl.LookAt(250, 0, 500, 250, 0, 0, 1, 0, 0);
        //        }
        //        else if (FlontViewButton.Checked)
        //        {
        //            gl.Ortho(-GLControl.Width * 1.5, GLControl.Width * 1.5, -GLControl.Height * 3, GLControl.Height * 5, 0.01, 10000);
        //            gl.LookAt(-500, 0, 100, 0, 0, 0, 0, 0, 1);
        //        }
        //        else
        //        {
        //            gl.Perspective(60.0, GLControl.Width / GLControl.Height, 0.01, 10000);
        //            gl.LookAt(-500.0, 300.0, 200.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0);
        //        }
        //        gl.Viewport(0, 0, GLControl.Width, GLControl.Height);

        //        gl.MatrixMode(SharpGL.Enumerations.MatrixMode.Modelview);
        //        gl.LoadIdentity();

        //        texture.Bind(gl);
        //        gl.Begin(OpenGL.GL_QUADS);
            
        //        gl.TexCoord(0, 0);
        //        gl.Vertex(p1.X, p1.Y, p1.Z);
            
        //        gl.TexCoord(1.0, 0.0);
        //        gl.Vertex(p2.X, p2.Y, p2.Z);
            
        //        gl.TexCoord(0.0, 1.0);
        //        gl.Vertex(p3.X, p3.Y, p3.Z);
            
        //        gl.TexCoord(1.0, 1.0);
        //        gl.Vertex(p4.X, p4.Y, p4.Z);

        //        gl.End();



        //           /* gl.Begin(OpenGL.GL_POINT_BIT);
        //            {
        //                foreach (List<PVector> list in vList)
        //                {
        //                    VectorComparer comp = new VectorComparer();
        //                    list.Sort(comp);
        //                    PVector vertex;

        //                    //中央値を描画していったもの
        //                    if (list.Count > 2)//3点以上の交点があった場合
        //                    {
        //                        float[] sourseArray = new float[list.Count];
        //                        int i = 0;
        //                        foreach (PVector v in list)
        //                        {
        //                            sourseArray[i] = v.Z;
        //                            i++;
        //                        }
        //                        float zPoint = CommonUtility.median(sourseArray);
        //                        vertex = new PVector(list[0].X, list[0].Y, zPoint);

        //                    }
        //                    else if (list.Count == 2)//平均で対処
        //                    {
        //                        float[] sourseArray = new float[2];
        //                        sourseArray[0] = list[0].Z;
        //                        sourseArray[1] = list[1].Z;
        //                        float zPoint = CommonUtility.average(sourseArray);
        //                        vertex = new PVector(list[0].X, list[0].Y, zPoint);
        //                    }
        //                    else//そのまま使用
        //                    {
        //                        vertex = list[0];
        //                    }


        //                    float col = renderZColor(vertex.Z);
        //                    float r, g, b;
        //                    renderColorGladation(col, out r, out g, out b);
        //                    gl.Color(r, g, b, 1.0F);//色を指定
        //                    gl.Vertex(vertex.X, vertex.Y, vertex.Z);
        //                    //ブレンドですべての三角形との交点を描画していったもの
        //                    /*
        //                    foreach (PVector v in list)
        //                    {
                            
        //                        float col = renderZColor(v.Z);
        //                        float r, g, b;
        //                        renderColorGladation(col, out r, out g, out b);
        //                        gl.Color(r, g, b, 0.5F);//色を指定
        //                        gl.Vertex(v.X, v.Y, v.Z);
        //                    }
        //                }
        //            }
        //            gl.End();
        //            //指定点の座標表示
        //            gl.PointSize(5.0f);
        //            gl.Begin(OpenGL.GL_POINTS);
        //            {
                    
        //                gl.Color(1.0f, 0.0f, 0.0f, 1.0f);
        //                foreach (PVector point in pointList)
        //                {
        //                    gl.Vertex(point.X, point.Y, point.Z);
        //                }
        //            }
        //            gl.End();*/
        //            renderFlag = false;
        //    }
        //}




        private float renderZColor(float val)
        {
            float MIN = PlaneMin;
            float MAX = PlaneMax;

            float ratio = val / (MAX - MIN);
            return ratio * 1.0F;
        }

        private void glInit(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<ExTriangle> TriangleList = new List<ExTriangle>();
           

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //ProgressDialog pd = new ProgressDialog();
            sw.Start();
          /*  pd.Title = @"進行状況";
            pd.Minimum = 0;
            pd.Maximum = 100;

            pd.Value = 0;
            pd.Show(this);
            */
            renderFlag = false;
            
            //三角形リストの生成
          //  pd.Message = @"三角形リストの生成．．．";
          //  float progressValue = 0.0F;
            
           /*for (int y = 0; y < Rimg.Height; y+=5)
            {   

                for (int x = 0; x <= Rimg.Width-10; x+=6)
                {
                    
                    PVector v1 = WorldPointFromImagePoint(x, y);
                    PVector v2 = WorldPointFromImagePoint(x + 5, y);
                    PVector v3 = WorldPointFromImagePoint(x, y + 4);
                    PVector v4 = WorldPointFromImagePoint(x + 5, y + 4);

                    ExTriangle t1 = new ExTriangle(v1, v2, v3);
                    ExTriangle t2 = new ExTriangle(v2, v3, v4);

                    TriangleList.Add(t1);
                    TriangleList.Add(t2);
                   // progressValue += (5.0F / 48.0F);
                    /*if (progressValue >= 1.0)
                    {
                        progressValue = progressValue - 1.0F;
                        pd.Value += 1;
                    }
                }

            }*/


           for (int y = 0; y < Rimg.Height-40; y += 40)
            {
                for (int x = 0; x <= Rimg.Width - 40; x += 40)
                {
                    PVector v1 = WorldPointFromImagePoint(x, y);
                    PVector v2 = WorldPointFromImagePoint(x + 39, y);
                    PVector v3 = WorldPointFromImagePoint(x, y + 39);
                    PVector v4 = WorldPointFromImagePoint(x + 39, y + 39);

                    ExTriangle t1 = new ExTriangle(v1, v2, v3);
                    ExTriangle t2 = new ExTriangle(v2, v3, v4);

                    TriangleList.Add(t1);
                    TriangleList.Add(t2);
                    // progressValue += (5.0F / 48.0F);
                    /*if (progressValue >= 1.0)
                    {
                        progressValue = progressValue - 1.0F;
                        pd.Value += 1;
                    }*/
                }

            }

           // pd.Value = 20;


            //世界座標空間上で-500<y<500, -500<x<1500上の座標をすべてvListに格納する
           // pd.Message = @"世界座標計算中…";
          //  progressValue = 0.0F;
            for (float y = -1000; y < 1000; y++)
            {
                for (float x = 0; x < 2000; x++)
                {
                  /*  progressValue += (2.0F / 50000.0F);
                    if (progressValue >= 1.0)
                    {
                        progressValue = progressValue - 1.0F;
                        pd.Value += 1;
                    }*/
                    List<ExTriangle> triangles = new List<ExTriangle>();
                    List<PVector> addList = new List<PVector>();
                    triangles.Clear();



                 
                    foreach (ExTriangle t in TriangleList)
                    {
                        if (x >= t.xMin && x <= t.xMax
                            && y >= t.yMin && y <= t.yMax)
                        {
                            triangles.Add(t);
                        }

                    }

     
                    if (triangles.Count == 0)
                    {
                        addList.Add(new PVector(x, y, -100.0F));
                        vList.Add(addList);
                        continue;
                    }




                    PVector start = new PVector((float)x, (float)y, -10.0F);
                    PVector end = new PVector((float)x, (float)y, 1000000.0F);
                    PVector v = new PVector(0.0F, 0.0F, 0.0F);
                    bool collision = false;

                    foreach (ExTriangle t in triangles)
                    {
                        collision = Collision.JudgeTrianglePlane(start, end, t,out v);

                        if (v.X == float.PositiveInfinity || v.X == float.NegativeInfinity || v.X == float.NaN)
                        {
                            v.X = 0;
                        }
                        if (v.Y == float.PositiveInfinity || v.Y == float.NegativeInfinity || v.Y == float.NaN)
                        {
                            v.Y = 0;
                        }
                        if (v.Z == float.PositiveInfinity || v.Z == float.NegativeInfinity || v.Z == float.NaN)
                        {
                            v.Z = 0;
                        }


                        if (collision == true) addList.Add(v);

                    }

                    if (collision == true) vList.Add(addList);
                    else if (collision == false)
                    {
                        addList.Add(new PVector(x, y, -100.0F));
                        vList.Add(addList);
                    }

                }

            }
          //  pd.Message = @"高さの最大、最少を計測・・・";
            foreach (var val in vList)
            {
                foreach (var v in val)
                {
                    if (PlaneMax < v.Z) PlaneMax = v.Z;
                    if (PlaneMin > v.Z) PlaneMin = v.Z;
                }
            }

            textBox1.Text = PlaneMin.ToString();
            textBox2.Text = PlaneMax.ToString();
           // PlaneMax = 200;
          //  PlaneMin = -100;

            renderFlag = true;
          //  pd.Value = 100;
           // pd.Close();

            sw.Stop();
            MessageBox.Show(sw.Elapsed.ToString(), @"計測時間");

            //確認のために使用する四点
            
        }

        private void CreateColorBar(PictureBox px)
        {
            int width = px.Width;
            int height = px.Height;
            Bitmap bmp = new Bitmap(width, height);

            //bitmap高速処理 http://daisy64.blogspot.jp/2009/01/getpixel.html
            BitmapData bitmapData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            int bDWidth = bitmapData.Width;
            int bDHeight = bitmapData.Height;
            int nResidual = bitmapData.Stride - bDWidth * 3;
            unsafe
            {
                byte* p = (byte*)bitmapData.Scan0;
                for (int y = 0; y < bDHeight; y++)
                {
                    for (int x = 0; x < bDWidth; x++)
                    {
                        float r;
                        float g;
                        float b;

                        float val = (float)y / height;
                        renderColorGladation(val, out r, out g, out b);

                        p[0] = (byte)(r*255);
                        p[1] = (byte)(g*255);
                        p[2] = (byte)(b*255);

                        p += 3;
                    }
                    p += nResidual;
                }
            }
            bmp.UnlockBits(bitmapData);
            px.Image = bmp;
        }



        private void renderColorGladation(float val, out float r, out float g, out float b)
        {

            //float a = (float)5/(float)6;
            float value = val;
            float tmp_val = (float)Math.Cos(4 * Math.PI * value);
            float col_val = (float)(-tmp_val / 2 + 0.5);
            if (value > (4.0 / 4.0)) { r = 1.0F; g = 1.0F; b = 1.0F; }
            else if (value == (4.0 / 4.0)) { r = 1.0F; g = 0; b = 0; }   // 赤
            else if (value >= (3.0 / 4.0)) { r = 1.0F; g = col_val; b = 0; }   // 黄～赤
            else if (value >= (2.0 / 4.0)) { r = col_val; g = 1.0F; b = 0; }   // 緑～黄
            else if (value >= (1.0 / 4.0)) { r = 0; g = 1.0F; b = col_val; }   // 水～緑
            else if (value > (0.0 / 4.0)) { r = 0; g = col_val; b = 1.0F; }   // 青～水
            else if (value == 0.0) { r = 0; g = 0; b = 1.0F; }
            else { r = 0; g = 0; b = 0; }   
        }

        private void TopViewButton_CheckedChanged(object sender, EventArgs e)
        {
            renderFlag = true;
        }

        private void ParseViewButton_CheckedChanged(object sender, EventArgs e)
        {
            renderFlag = true;
        }

        private void FlontViewButton_CheckedChanged(object sender, EventArgs e)
        {
            renderFlag = true;
        }

        bool firstClick = true;
        Point startPoint, endPoint;
        List<Point> drawPointList = new List<Point>();
        private void imageClick(object sender, MouseEventArgs e)
        {
            if (firstClick == true)
            {
                firstClick = false;
                startPoint = new Point(e.X*2, e.Y*2);
            }
            else
            {
                endPoint =new Point(e.X*2, e.Y*2);
                //傾きを計算
                double a = (double)(endPoint.Y - startPoint.Y) / (double)(endPoint.X - startPoint.X);
                double b = startPoint.Y - (a * startPoint.X);


                if (startPoint.X < endPoint.X)
                {
                    for (int i = startPoint.X; i < endPoint.X; i++)
                    {
                        Point p = new Point(i, (int)(a * i + b));
                        drawPointList.Add(p);
                    }
                }
                else
                {
                    for (int i = endPoint.X; i < startPoint.X; i++)
                    {
                        Point p = new Point(i, (int)(a * i + b));
                        drawPointList.Add(p);
                    }
                }

                Bitmap bmp = (Bitmap)RightImageBox.Image.Clone();
                Graphics g = Graphics.FromImage(bmp);
                Pen rPen = new Pen(Color.Red);

                g.DrawLine(rPen, new Point(startPoint.X / 2, startPoint.Y / 2), new Point(endPoint.X / 2, endPoint.Y / 2));

                CommonUtility.FillPicBox(bmp, RightImageBox);

                foreach (Point p in drawPointList)
                {
                    pointList.Add(WorldPointFromImagePoint(p.X, p.Y));
                }
            }

        }
    }


    //IplImageの拡張クラス
    static class IplImageExtencion
    {
        public static IplImage toGray(this IplImage img)
        {
            if (img.NChannels != 1)
            {
                IplImage gray = new IplImage(img.Size, BitDepth.U8, 1);
                Cv.CvtColor(img, gray, ColorConversion.BgrToGray);
                return gray;
            }
            else
            {
                return img;
            }
        }
    }
    class VectorComparer : IComparer<PVector>
    {
        public int Compare(PVector x, PVector y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            if (x == null)
            {
                return -1;
            }
            if (y == null)
            {
                return 1;
            }

            if (x.Z > y.Z)
            {
                return 1;

            }
            else
            {
                return -1;
            }
          
        }


    }
    class ExTriangle : Triangle
    {
        public float xMax { private set; get; } 
        public float xMin { private set; get; }
        public float yMax { private set; get; }
        public float yMin { private set; get; }


        public ExTriangle(PVector v1, PVector v2, PVector v3):base(v1,v2,v3)
        {
            this.xMax = MultiMax(v1.X, v2.X, v3.X);
            this.xMin = MultiMin(v1.X, v2.X, v3.X);
            this.yMax = MultiMax(v1.Y, v2.Y, v3.Y);
            this.yMin = MultiMin(v1.Y, v2.Y, v3.Y);
        }

        private float MultiMax(params float[] vals)
        {
            float Max = float.MinValue;
            foreach (float val in vals)
            {
                if (Max < val)
                {
                    Max = val;
                }
            }

            return Max;
        }

        private float MultiMin(params float[] vals)
        {
            float Min = float.MaxValue;

            foreach (float val in vals)
            {
                if (val < Min)
                {
                    Min = val;
                }
            }

            return Min;
        }
    }
}
