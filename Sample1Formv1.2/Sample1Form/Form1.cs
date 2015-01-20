using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Kinect;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;

namespace Sample1Form
{
    public partial class Form1 : Form
    {
        readonly int Bgr32BytesPerPixel = 4;
        bool recFlag = false;
        int count = 0;
        byte[] bgImage=new byte[640*480*3];
        Bitmap depthImageBmp = new Bitmap(640, 480);
        Form2 f2 = new Form2();
        //string imgPath;
        bool compoFlag = false;
        Bitmap bgImageBmp;

        public Form1()
        {
            InitializeComponent();


            try
            {
                //Kinectが接続されているか確認
                if (KinectSensor.KinectSensors.Count == 0)
                {
                    throw new Exception("Kinectを接続してください。");
                }

                //Kinectの動作を開始する
                StartKinect(KinectSensor.KinectSensors[0]);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);

            }
        }

        /// <summary>
        /// Kinectの動作を開始する
        /// </summary>
        /// <param name="kinect"></param>

        private void StartKinect(KinectSensor kinect)
        {
            kinect.ColorStream.Enable();
            kinect.DepthStream.Enable();
            kinect.SkeletonStream.Enable();

            kinect.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinect_AllFramesReady);

            kinect.Start();

            //DefaultModeとNearModeの切り替え
            comboBox1.Items.Clear();
            foreach (var range in Enum.GetValues(typeof(DepthRange)))
            {
                comboBox1.Items.Add(range.ToString());

            }
            comboBox1.SelectedIndex = 0;
        }

        /// <summary>
        /// Kinectの動作を停止する
        /// </summary>
        /// <param name="kinect"></param>

        private void KinectStop(KinectSensor kinect)
        {
            if (kinect != null)
            {
                if (kinect.IsRunning)
                {
                    kinect.AllFramesReady -= kinect_AllFramesReady;

                    kinect.Stop();
                    kinect.Dispose();

                    pictureBox1.Image = null;
                }
            }
        }

        /// <summary>
        /// RGBカメラ、深度カメラ、骨格のフレーム更新イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            try
            {
                //Kinectのインスタンスを取得する
                KinectSensor kinect = sender as KinectSensor;
                if (kinect == null)
                {
                    return;
                }

                //RGBカメラのフレームデータを取得する
                using(DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
                {
                    if (colorFrame != null)
                    {
                        //RGBカメラのピクセルデータを取得する
                        byte[] colorPixel = new byte[colorFrame.PixelDataLength];
                        colorFrame.CopyPixelDataTo(colorPixel);
                        
                        //ピクセルデータをビットマップに変換する
                        Bitmap bmp = new Bitmap(kinect.ColorStream.FrameWidth, kinect.ColorStream.FrameHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                        Rectangle rect = new Rectangle(0, 0, bmp.Width,bmp.Height);
                        //Rectangle rect_comp = new Rectangle(0, 0, bmp_comp.Width, bmp_comp.Height);

                        BitmapData data = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
                        //BitmapData data_comp = bmp_comp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

                        if (checkBox1.Checked)
                        {
                            if (depthFrame != null)
                            {
                                byte[] gray = CreateGreenBack(kinect, depthFrame, colorFrame);
                                int k = 0;
                                //黒い部分が多少残っているためそこをグリーンにする
                                for (int i = 0; i < gray.Length; i += Bgr32BytesPerPixel)
                                {
                                    if (gray[i] == 0 &&
                                       gray[i + 1] == 0 &&
                                       gray[i + 2] == 0)
                                    {
                                        gray[i + 1] = 255;
                                    }
                                    if (compoFlag == true)
                                    {
                                        if (gray[i] == 0 && gray[i + 1] == 255 && gray[i + 2] == 0)
                                        {
                                            gray[i] = bgImage[k];
                                            gray[i + 1] = bgImage[k + 1];
                                            gray[i + 2] = bgImage[k + 2];
                                        }
                                    }
                                    k += 3;

                                }
                                    Marshal.Copy(gray, 0, data.Scan0, gray.Length);
                            }
                        }
                        else
                        Marshal.Copy(colorPixel, 0, data.Scan0, colorPixel.Length);

                        bmp.UnlockBits(data);
                       

                        pictureBox1.Image = bmp;
                      
                    }
                }
                


                //書き込み用のビットマップデータを作成(32ビット　ビットマップ)
                //16bpp グレースケールは表示できない
                //距離カメラのフレームデータを取得する
                
                using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                {
                    
                    if (depthFrame != null)
                    {
                        //距離データを画像化して表示
                        Bitmap bmp = new Bitmap(kinect.DepthStream.FrameWidth, kinect.DepthStream.FrameHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                        Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

                        BitmapData data = bmp.LockBits(rect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        byte[] gray = ConvertDepthColor(kinect, depthFrame);
                        Marshal.Copy(gray, 0, data.Scan0, gray.Length);
                        bmp.UnlockBits(data);
                        depthImageBmp = bmp;
                        //pictureBox2.Image = bmp;
                    }
                }


                //スケルトンのフレームデータを取得する
                if (checkBox2.Checked)
                {
                    using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                    {
                        if (skeletonFrame != null)
                        {

                            const int R = 5;
                            Graphics g = Graphics.FromImage(pictureBox1.Image);

                            //スケルトンデータを取得する
                            Skeleton[] skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                            skeletonFrame.CopySkeletonDataTo(skeletons);

                            //トラッキングされているスケルトンのジョイントを描画する
                            foreach (var skeleton in skeletons)
                            {
                                //スケルトンがトラッキングされてなければ次へ
                                if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
                                {
                                    continue;
                                }

                                //ジョイントを描画する
                                    foreach (Joint joint in skeleton.Joints)
                                    {
                                        //ジョイントがトラッキングされていなければ次へ
                                        if (joint.TrackingState != JointTrackingState.Tracked)
                                        {
                                            continue;
                                        }

                                        //スケルトンの座標をRGBカメラの座標に変換して円を描く
                                        //ColorImagePoint point = kinect.MapSkeletonPointToColor(joint.Position,kinect.ColorStream.Format)<-これは古い書き方らしい
                                        ColorImagePoint point = kinect.CoordinateMapper.MapSkeletonPointToColorPoint(joint.Position, kinect.ColorStream.Format);
                                        g.DrawEllipse(new Pen(Brushes.Red), new Rectangle(point.X - R, point.Y - R, R * 2, R * 2));
                                       
                                    }//end of Second foreach 
                                }//end of First foreach
                        }//end of if()
                    }//end of using()
                }
            }//end of try
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }

        }


        /// <summary>
        /// 距離データをカラー画像に変換
        /// </summary>
        /// <param name="kinect"></param>
        /// <param name="depthFrame"></param>
        /// <returns></returns>

        private byte[] ConvertDepthColor(KinectSensor kinect, DepthImageFrame depthFrame)
        {
            ColorImageStream colorStream = kinect.ColorStream;
            DepthImageStream depthStream = kinect.DepthStream;

            //距離カメラのピクセルごとのデータを取得する
            //short[] depthPixel = new short[depthFrame.PixelDataLength]<-古いやりかた
            DepthImagePixel[] depthPixel = new DepthImagePixel[depthFrame.PixelDataLength];
            depthFrame.CopyDepthImagePixelDataTo(depthPixel);


            //距離カメラの座標に対応するRGBカメラの座標を取得する
            ColorImagePoint[] colorPoint = new ColorImagePoint[depthFrame.PixelDataLength];
            //kinect.MapDepthFrameToColorFrame()は古い
            kinect.CoordinateMapper.MapDepthFrameToColorFrame(depthStream.Format, depthPixel, colorStream.Format, colorPoint);

            byte[] depthColor = new byte[depthFrame.PixelDataLength*Bgr32BytesPerPixel];
            for (int index = 0; index < depthPixel.Length; index++)
            {
                //距離カメラのデータからプレイヤーIDと距離を取得する
                int player = depthPixel[index].PlayerIndex;
                int distance = depthPixel[index].Depth;

                //変換した結果がフレームサイズを超えることがあるため、
                //小さいほうを使う
                int x = Math.Min(colorPoint[index].X, colorStream.FrameWidth - 1);
                int y = Math.Min(colorPoint[index].Y, colorStream.FrameHeight - 1);
                int colorIndex = ((y * depthFrame.Width) + x) * Bgr32BytesPerPixel;

                //プレイヤーがいるピクセルの場合
               if (player != 0)
                {
                    depthColor[colorIndex] = 255;
                    depthColor[colorIndex + 1] = 255;
                    depthColor[colorIndex + 2] = 255;
                }
                else//プレイヤーでないピクセルの場合
                {
                    //サポート外
                    if (distance == depthStream.UnknownDepth)
                    {
                        depthColor[colorIndex] = 0;
                        depthColor[colorIndex + 1] = 0;
                        depthColor[colorIndex + 2] = 255;
                    }
                    //近すぎ
                    else if (distance == depthStream.TooNearDepth)
                    {
                        depthColor[colorIndex] = 0;
                        depthColor[colorIndex + 1] = 255;
                        depthColor[colorIndex + 2] = 0;

                    }
                    //遠すぎ
                    else if (distance == depthStream.TooFarDepth)
                    {
                        depthColor[colorIndex] = 255;
                        depthColor[colorIndex + 1] = 0;
                        depthColor[colorIndex + 2] = 0;

                    }
                    //有効な距離データ
                    else
                    {
                        depthColor[colorIndex] = 0;
                        depthColor[colorIndex + 1] = 255;
                        depthColor[colorIndex + 2] = 255;

                    }
                }
            }



                return depthColor;
        }
        /// <summary>
        /// 距離データとrgbカメラをもとにグリーンバックでマスクされた画像を作成
        /// </summary>
        /// <param name="kinect"></param>
        /// <param name="depthFrame"></param>
        /// <param name="colorFrame"></param>
        /// <returns></returns>

        private byte[] CreateGreenBack(KinectSensor kinect, DepthImageFrame depthFrame, ColorImageFrame colorFrame)
        {
            ColorImageStream colorStream = kinect.ColorStream;
            DepthImageStream depthStream = kinect.DepthStream;

            //距離カメラのピクセルごとのデータを取得する
            DepthImagePixel[] depthPixel = new DepthImagePixel[depthFrame.PixelDataLength];
            depthFrame.CopyDepthImagePixelDataTo(depthPixel);

            //距離カメラの座標に対応するRGBカメラの座標を取得する
            ColorImagePoint[] colorPoint = new ColorImagePoint[depthFrame.PixelDataLength];
            kinect.CoordinateMapper.MapDepthFrameToColorFrame(depthStream.Format, depthPixel, colorStream.Format, colorPoint);

            byte[] depthColor = new byte[depthFrame.PixelDataLength * Bgr32BytesPerPixel];
            byte[] bgColor=new byte[pictureBox1.Image.Size.Width*pictureBox1.Image.Size.Height*3];
            byte[] colorPixel = new byte[colorFrame.PixelDataLength];
            colorFrame.CopyPixelDataTo(colorPixel);


            for (int index = 0; index < depthPixel.Length; index++)
            {
                //距離カメラのデータからプレイヤーIDと距離を取得する
                int player = depthPixel[index].PlayerIndex;
                int distance = depthPixel[index].Depth;

                //変換した結果がフレームサイズを超えることがあるため、
                //小さいほうを使う
                int x = Math.Min(colorPoint[index].X, colorStream.FrameWidth - 1);
                int y = Math.Min(colorPoint[index].Y, colorStream.FrameHeight - 1);
                int colorIndex = ((y * depthFrame.Width) + x) * Bgr32BytesPerPixel;

                //プレイヤーがいるピクセルの場合
                if (player != 0)
                {
                    depthColor[colorIndex] = colorPixel[colorIndex];
                    depthColor[colorIndex + 1] = colorPixel[colorIndex + 1];
                    depthColor[colorIndex + 2] = colorPixel[colorIndex + 2];
                    //continue;
                }
                else//プレイヤーでないピクセルの場合
                {
                        depthColor[colorIndex] = 0;
                        depthColor[colorIndex + 1] = 255;
                        depthColor[colorIndex + 2] = 0;

               
                }
            }

            return depthColor;


        }


        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox2.Items.Add(@"山");
            comboBox2.Items.Add(@"海");
            comboBox2.Items.Add(@"磐梯山");
            comboBox2.Items.Add(@"ニューヨーク");
            comboBox2.Items.Add(@"パリ");
            comboBox2.Items.Add(@"ロンドン");
            comboBox2.Items.Add(@"ブルージュ");
            comboBox2.Items.Add(@"あまちゃん");
            comboBox2.Items.Add(@"うめちゃん");

        }

        /// <summary>
        /// Windowが閉じられるときのイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            KinectStop(KinectSensor.KinectSensors[0]);
        }

        /// <summary>
        /// 距離カメラの通常/近接モード変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                KinectSensor.KinectSensors[0].DepthStream.Range = (DepthRange)comboBox1.SelectedIndex;
            }
            catch (Exception)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (recFlag == false)
            {
                button1.Text = @"STOP";
                recFlag = true;
                timer1.Start();
            }
            else
            {
                recFlag = false;
                button1.Text = @"REC";
                timer1.Stop();
                count = 0;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            //上部に表示する説明テキストを指定する
            fbd.Description = "フォルダを指定してください。";
            //ルートフォルダを指定する
            //デフォルトでDesktop
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            //最初に選択するフォルダを指定する
            //RootFolder以下にあるフォルダである必要がある
            fbd.SelectedPath = @"C:\Windows";
            //ユーザーが新しいフォルダを作成できるようにする
            //デフォルトでTrue
            fbd.ShowNewFolderButton = true;

            //ダイアログを表示する
            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                //選択されたフォルダを表示する
                textBox1.Text = fbd.SelectedPath;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (recFlag == true)
            {
                if (textBox1.Text != null)
                {
                    pictureBox1.Image.Save(textBox1.Text + @"\image" + count + @".bmp");
                    count++;
                }
                else MessageBox.Show(@"保存先が指定されてません。");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(comboBox2.SelectedIndex>=0){
                int k = 0;
                    for (int i = 0; i < bgImageBmp.Height; i++)
                    {
                        for (int j = 0; j < bgImageBmp.Width; j++)
                        {

                            bgImage[k] = bgImageBmp.GetPixel(j, i).B;
                            bgImage[k+1] = bgImageBmp.GetPixel(j, i).G;
                            bgImage[k+2] = bgImageBmp.GetPixel(j, i).R;
                            k += 3;
                        }
                    }
                    if (checkBox1.Checked)
                    {
                        compoFlag = true;
                    }
                    else
                    {
                        MessageBox.Show("合成を有効にするにはグリーンバックにチェックを入れる必要があります");
                    }
            }
            else{
                MessageBox.Show("画像を選択してください");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            f2.Owner = this;
            f2.ShowDialog(this);
        }


        public Bitmap depthImage
        {
            get
            {
                return depthImageBmp;
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {                    
            string current=Environment.CurrentDirectory;
            string imgPath=@"";
            
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    imgPath = current + @"\data\yama.jpg";
                    break;
                case 1:
                    imgPath = current + @"\data\umi.jpg";
                    break;
                case 2:
                    imgPath = current + @"\data\Bandai.jpg";
                    break;
                case 3:
                    imgPath = current + @"\data\nyu.jpg";
                    break;
                case 4:
                    imgPath=current+@"\data\paris.jpg";
                    break;
                case 5:
                    imgPath = current + @"\data\rondon.jpg";
                    break;
                case 6:
                    imgPath = current + @"\data\Bruges.jpg";
                    break;
                case 7:
                    imgPath = current + @"\data\amachan.jpg";
                    break;
                case 8:
                    imgPath = current + @"\data\umechan.jpg";
                    break;
                default:
                    break;
            }
            bgImageBmp = new Bitmap(imgPath);
            Bitmap thumb = new Bitmap(bgImageBmp, 100, 75);

            pictureBox2.Image = thumb;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                pictureBox1.Image.Save(Environment.CurrentDirectory + @"\image.bmp");
                MessageBox.Show(Environment.CurrentDirectory + @"\image.bmp" + "に保存しました");
            }
            else
            {
                pictureBox1.Image.Save(textBox1.Text+@"\image.bmp");
                MessageBox.Show(textBox1.Text + @"\image.bmp" + "に保存しました");
            }
        }

    }
}
