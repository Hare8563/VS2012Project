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
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Extensions;

namespace ImageSegmentation
{
    struct KMeanCluster
    {
        public CvColor color;
        public int Label;

        public KMeanCluster(CvColor color, int label)
        {
            this.color = color;
            this.Label = label;
        }
    }

    struct Sphere
    {
        public CvPoint3D32f center;
        public float Radius;

        public Sphere(float x, float y, float z, float R)
        {
            this.center.X = x;
            this.center.Y = y;
            this.center.Z = z;
            this.Radius = R;
        }
        public Sphere(CvPoint3D32f center, float R)
        {
            this.center = center;
            this.Radius = R;
        }
        /// <summary>
        /// 入力点がこの球体の内部にあるか
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsInner(CvPoint3D32f p)
        {
            double twoPointDistance = Math.Sqrt((center.X - p.X) * (center.X - p.X)
                                                +(center.Y - p.Y) * (center.Y - p.Y)
                                                + (center.Z - p.Z) * (center.Z - p.Z));
            if (twoPointDistance <= Radius) return true;
            else return false;
        }
    }

    public partial class Form1 : Form
    {
        IplImage img = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //はじめのファイル名を指定する

            //はじめに表示されるフォルダを指定する
            //指定しない（空の文字列）の時は、現在のディレクトリが表示される
            ofd.InitialDirectory = @"C:\";
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しないとすべてのファイルが表示される
            ofd.Filter =
                "Imageファイル(*.bmp;*.jpg;*.png)|*.bmp;*.jpg;*.png";
            //[ファイルの種類]ではじめに
            //「すべてのファイル」が選択されているようにする
            ofd.FilterIndex = 2;
            //タイトルを設定する
            ofd.Title = "開くファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;
            //存在しないファイルの名前が指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckFileExists = true;
            //存在しないパスが指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckPathExists = true;

            //ダイアログを表示する
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //OKボタンがクリックされたとき
                //選択されたファイル名を表示する
                textBox1.Text = ofd.FileName;
                img = Cv.LoadImage(ofd.FileName, LoadMode.Color);
                LoadImage(img.ToBitmap());
            }
        }


        private void LoadImage(Bitmap bmp)
        {
            double hi;
            double imagew = bmp.Width;
            double imageh = bmp.Height;

            if ((pictureBox1.Height / pictureBox1.Width) <= (imageh / imagew))
            {
                hi = pictureBox1.Height / imageh;
            }
            else
            {
                hi = pictureBox1.Width / imagew;
            }
            int w = (int)(imagew * hi);
            int h = (int)(imageh * hi);

            Bitmap result = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(result);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(bmp, 0, 0, result.Width, result.Height);

            pictureBox1.Image = result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0 || img == null)
            {
                MessageBox.Show(@"Segmentationの選択、またはファイルを選択してください");
                return;
            }


            IplImage HsvImage = new IplImage(img.Width, img.Height, BitDepth.U8, 3);
            Cv.CvtColor(img, HsvImage, ColorConversion.BgrToHsv_Full);

            IplImage resultImg = null;
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    resultImg = ColorGradientSegmentation(HsvImage);
                    break;
                case 1:
                    resultImg = BrightnessSegmentation(HsvImage);
                    break;
                case 2:
                    resultImg = KMeansSegmentation(img);
                    break;
                case 3:
                    
                    resultImg = MeanShiftSegmentation(img);
                    break;
                default:
                    MessageBox.Show(@"Segmentationのタイプを選択してください");
                    return;
            }

            LoadImage(resultImg.ToBitmap());


            //Evaluation
            string answerFile = @"C:\Users\Tetsushi\Desktop\IRU2\answer2.jpg";
            IplImage answer = new IplImage(answerFile, LoadMode.GrayScale);
            //答え: t or f, 計測: p or n
            int tp=0, fp=0, tn=0, fn=0;


            for (int y = 0; y < answer.Height; y++)
            {
                for (int x = 0; x < answer.Width; x++)
                {
                    var val = answer[y, x].Val0 - resultImg[y, x].Val0;
                    if (val == 0)
                    {
                        if (answer[y, x].Val0 == 255) fn++;//間違いを間違いと認識。今回は白を白と認識したことにする
                        else tp++;//正解を正解と認識。今回は黒を黒と認識
                    }
                        
                    else if (val > 0) fp++;//白を黒と認識
                    else if (val < 0) tn++;//黒を白と認識
                       
                }
            }

            MessageBox.Show(@"tp: " + 100 * ((float)tp / (answer.Width * answer.Height)) + "%\nfp: " + 100 * ((float)fp / (answer.Width * answer.Height)) + "%\ntn: " + 100 * ((float)tn / (answer.Width * answer.Height)) + "%\nfn: " + 100 * ((float)fn / (answer.Width * answer.Height)));


        }

        private IplImage ColorGradientSegmentation(IplImage HsvImage)
        {
            const int threshold = 20;
            IplImage returnImg = new IplImage(HsvImage.Size, BitDepth.U8, 1);
            IplImage dstImg = new IplImage(HsvImage.Size, BitDepth.U8, 3);
           // Mat dst = new Mat(HsvImage.Size,MatType.CV_8UC3);
          //  Cv2.GaussianBlur(new Mat(HsvImage.Clone()), dst, Cv.Size(5, 5), 10);
            dstImg = HsvImage;

            for (int y = 0; y < HsvImage.Height; y++)
            {
                for (int x = 0; x < HsvImage.Width; x++)
                {
                    if ((x - 1 < 0 || x + 1 >= HsvImage.Width)||
                        (y - 1 < 0 || y + 1 >= HsvImage.Height))
                    {
                        returnImg[y, x] = 0;
                    }
                    else
                    {
                        
                        double Gx = dstImg[y,x+1].Val0 - dstImg[y, x - 1].Val0;
                        double Gy = dstImg[y + 1, x].Val0 - dstImg[y - 1, x].Val0;
                        if (Math.Sqrt(Gx*Gx + Gy*Gy) > threshold)
                        {
                            returnImg[y, x] = 255;
                        }
                        else
                        {
                            returnImg[y, x] = 0;
                        }
                    }
                }

            }

                return returnImg;
        }

        private IplImage BrightnessSegmentation(IplImage HsvImage)
        {
            const int threshold = 18;
            IplImage returnImg = new IplImage(HsvImage.Size, BitDepth.U8, 1);
            IplImage dstImg = new IplImage(HsvImage.Size, BitDepth.U8, 3);
            //Mat dst = new Mat(HsvImage.Size, MatType.CV_8UC3);
            //Cv2.GaussianBlur(new Mat(HsvImage.Clone()), dst, Cv.Size(5, 5), 10);
            
            dstImg =HsvImage;

            for (int y = 0; y < HsvImage.Height; y++)
            {
                for (int x = 0; x < HsvImage.Width; x++)
                {
                    if ((x - 1 < 0 || x + 1 >= HsvImage.Width) ||
                        (y - 1 < 0 || y + 1 >= HsvImage.Height))
                    {
                        returnImg[y, x] = 0;
                    }
                    else
                    {

                        double Gx = dstImg[y, x + 1].Val2 - dstImg[y, x - 1].Val2;
                        double Gy = dstImg[y + 1, x].Val2 - dstImg[y - 1, x].Val2;
                        //Console.WriteLine(Math.Sqrt(Gx * Gx + Gy * Gy));
                        if (Math.Sqrt(Gx * Gx + Gy * Gy) > threshold)
                        {
                            
                            returnImg[y, x] = 255;
                        }
                        else
                        {
                            returnImg[y, x] = 0;
                        }
                    }
                }

            }

            return returnImg;

        }

        private IplImage KMeansSegmentation(IplImage colorImg)
        {
            const int K =　3, trialLimit = 10;

            KMeanCluster[] data = KMeansAlgorithms(colorImg, K, trialLimit);
            IplImage result = new IplImage(colorImg.Width, colorImg.Height, BitDepth.U8, 1);

         

            for (int y = 0; y < colorImg.Height;y++ )
            {
                for (int x = 0; x < colorImg.Width; x++)
                {
                    if(x-1 < 0 || x + 1 >= colorImg.Width
                        || y - 1 < 0 || y + 1 >= colorImg.Height)
                    {
                        result[y, x] = 0;
                    }
                    else
                    {
                        if((data[y *colorImg.Width + (x-1)].Label != data[y * colorImg.Width + (x + 1)].Label)||
                        (data[(y - 1) * colorImg.Width + x].Label != data[(y + 1) * colorImg.Width + x].Label))
                        {
                            result[y, x] = 255;
                        }
                        else
                        {
                            result[y, x] = 0;
                        }
                    }
                }
            }
                return result;
        }

        /// <summary>
        /// K-Meansアルゴリズムの実装
        /// 参考: http://tech.nitoyon.com/ja/blog/2009/04/09/kmeans-visualise/
        /// </summary>
        /// <param name="colorImg"></param>
        /// <param name="K"></param>
        /// <param name="trialLimit"></param>
        /// <returns></returns>
        private KMeanCluster[] KMeansAlgorithms(IplImage colorImg, int K, int trialLimit)
        {
           
            KMeanCluster[] data = new KMeanCluster[colorImg.Width * colorImg.Height];
            IplImage HsvImage = new IplImage(img.Width, img.Height, BitDepth.U8, 3);
            Cv.CvtColor(colorImg, HsvImage, ColorConversion.BgrToHsv_Full);

            //Step 1, Initialize state
            for (int y = 0; y < colorImg.Height; y++)
            {
                for (int x = 0; x < colorImg.Width; x++)
                {

                    data[y * colorImg.Width + x].Label = (y * colorImg.Width + x) % K;

                    data[y * colorImg.Width + x].color.B = (byte)colorImg[y, x].Val0;
                    data[y * colorImg.Width + x].color.G = (byte)colorImg[y, x].Val1;
                    data[y * colorImg.Width + x].color.R = (byte)colorImg[y, x].Val2;

                }
            }

      
            //Step 2, Step 3 loop
            for (int i = 0; i < trialLimit; i++)
            {
                //Step 2
                CvPoint3D32f[] meanOfPoint = new CvPoint3D32f[K];
                int[] meanOfLength = new int[K];
                for (int j = 0; j < data.Length; j++)
                {
                    meanOfPoint[data[j].Label].X += data[j].color.B;
                    meanOfPoint[data[j].Label].Y += data[j].color.G;
                    meanOfPoint[data[j].Label].Z += data[j].color.R;
                    meanOfLength[data[j].Label]++;
                }

                for (int j = 0; j < K; j++)
                {
                    meanOfPoint[j].X /= meanOfLength[j];
                    meanOfPoint[j].Y /= meanOfLength[j];
                    meanOfPoint[j].Z /= meanOfLength[j];
                }

                //Step 3
                for (int j = 0; j < data.Length; j++)
                {
                    double minDistance = double.MaxValue;
                    int minLabel = 0;

                    for (int k = 0; k < K; k++)
                    {
                        float differenceB = data[j].color.B - meanOfPoint[k].X;
                        float differenceG = data[j].color.G - meanOfPoint[k].Y;
                        float differenceR = data[j].color.R - meanOfPoint[k].Z;

                        double twoPointDistance = Math.Sqrt(differenceB * differenceB + differenceG * differenceG + differenceR * differenceR);
                        if (twoPointDistance < minDistance)
                        {
                            minDistance = twoPointDistance;
                            minLabel = k;
                        }

                    }
                    data[j].Label = minLabel;
                }

                
            }//end of step 2, 3 loop


            return data;
        }

        private IplImage MeanShiftSegmentation(IplImage colorImg)
        {
            
            Mat colorMat = new Mat(colorImg);
            Mat dstMat = new Mat(colorImg.Size, MatType.CV_8UC3);

            Cv2.GaussianBlur(colorMat, dstMat, Cv.Size(5, 5), 10);
            IplImage dstImg = dstMat.ToIplImage();
           // KMeanCluster[] meanShiftCluster = MeanShiftAlgorithms(dstImg, 10.0f);
            Cv.PyrMeanShiftFiltering(colorImg, dstImg, 128.0f, 150.0f);
            //return dstImg;

            IplImage result = new IplImage(colorImg.Width, colorImg.Height, BitDepth.U8, 1);
            for (int y = 0; y < colorImg.Height; y++)
            {
                for (int x = 0; x < colorImg.Width; x++)
                {
                    if(x-1 < 0 || x+1 >= colorImg.Width ||
                        y - 1 < 0 || y + 1 >= colorImg.Height)
                    {
                        result[y,x] = 0;
                    }
                    else
                    {
                       // float gradientClusterX = meanShiftCluster[y * colorImg.Width + (x - 1)].Label - meanShiftCluster[y * colorImg.Width + (x + 1)].Label;
                      //  float gradientClusterY = meanShiftCluster[(y - 1) * colorImg.Width + x].Label - meanShiftCluster[(y + 1) * colorImg.Width + 1].Label;
                        double gradientClusterX = dstImg[y, x - 1].Val0 - dstImg[y, x + 1].Val0;
                        double gradientClusterY = dstImg[y - 1, x].Val0 - dstImg[y + 1, x].Val0;

                        if (gradientClusterX != 0 || gradientClusterY != 0)
                        {
                            result[y, x] = 255;
                        }
                        else
                        {
                            result[y, x] = 0;
                        }
                    }

                }
            }
            
            return result;
        }
        /// <summary>
        /// MeanShiftアルゴリズムの実装(実装したけど時間がかかるため今回は使用しない)
        /// 参考: http://blog.livedoor.jp/itukano/archives/51806727.html
        /// </summary>
        /// <param name="colorImg">カラー画像</param>
        /// <param name="R">球体半径</param>
        /// <param name="trialLimit">試行回数</param>

        private KMeanCluster[] MeanShiftAlgorithms(IplImage colorImg, float R)
        {

            List<CvPoint3D32f> clusterPointList = new List<CvPoint3D32f>();
            KMeanCluster[] cluster = new KMeanCluster[colorImg.Width * colorImg.Height];
            for (int y = 0; y < colorImg.Height; y++) 
            {
                for (int x = 0; x < colorImg.Width; x++)
                {
                    
                    Sphere s = new Sphere((float)colorImg[y, x].Val0, (float)colorImg[y, x].Val1, (float)colorImg[y, x].Val2, R);
                    
                    //MeanShift algorithms
                    while (true)
                    {
                       // bool[,] cluster = new bool[colorImg.Height, colorImg.Width];
                        CvPoint3D32f sum = new CvPoint3D32f(0, 0, 0);
                        int count = 0;
                        for (int i = 0; i < colorImg.Height; i++)
                        {
                            for (int j = 0; j < colorImg.Width; j++)
                            {

                                if (s.IsInner(new CvPoint3D32f(colorImg[i, j].Val0, colorImg[i, j].Val1, colorImg[i, j].Val2)))
                                {
                                    //もしもBGRが球体内部に入っていたら
                                 //   cluster[i, j] = true;
                                    sum.X += (float)colorImg[i, j].Val0;
                                    sum.Y += (float)colorImg[i, j].Val1;
                                    sum.Z += (float)colorImg[i, j].Val2;
                                    count++;
                                }
                                else
                                {
                                 //   cluster[i, j] = false;
                                }

                            }
                        }

                        //compute Mean
                        sum.X /= count;//B軸の平均
                        sum.Y /= count;//G軸の平均
                        sum.Z /= count;//R軸の平均

                        if (sum == s.center) break;

                        s.center = sum;//中心点移動
                    }

                    //var result = clusterPointList.Find(i => i.X == s.center.X && i.Y == s.center.Y && i.Z == s.center.Z);
                    if(!clusterPointList.Contains(s.center))//もしクラスタのリストに入っていない場合
                    {
                        clusterPointList.Add(s.center);
                    }

                    for (int n = 0; n < clusterPointList.Count; n++)
                    {
                        if (clusterPointList[n] == s.center)
                        {
                            cluster[y * colorImg.Width + x].color.B = (byte)colorImg[y, x].Val0;
                            cluster[y * colorImg.Width + x].color.G = (byte)colorImg[y, x].Val1;
                            cluster[y * colorImg.Width + x].color.R = (byte)colorImg[y, x].Val2;
                            cluster[y * colorImg.Width + x].Label = n;
                            break;
                        }
                    }
                        



                }//end of x loop       
            }//end of y loop

            return cluster;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    
    }
}
