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

namespace FaceRecognition
{
    public partial class Form1 : Form
    {

        IplImage img;
        Dictionary<string, string> cascadeDictionary = new Dictionary<string, string>();

        public Form1()
        {
            InitializeComponent();

            string[] files = System.IO.Directory.GetFiles(@"C:\dev\2.4.6\opencv\data\haarcascades", "*.xml", System.IO.SearchOption.AllDirectories);
            foreach (string file in files)
            {
                comboBox1.Items.Add(file);
            }


            for (int i = 0; i < comboBox1.Items.Count;i++)
                cascadeDictionary.Add(comboBox1.Items[i].ToString(), @"C:\dev\2.4.6\opencv\data\haarcascades\" + comboBox1.Items[i].ToString());

        }

        private void button1_Click(object sender, EventArgs e)
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

        private void button2_Click(object sender, EventArgs e)
        {
            const double ScaleFactor = 1.0850;
            const int MinNeighbors = 2;


            if (comboBox1.SelectedIndex < 0 || img == null)
            {
                MessageBox.Show(@"cascadeの選択、またはファイルを選択してください");
                return;
            }

            IplImage grayImg = new IplImage(img.Width, img.Height, BitDepth.U8, 1);
            Cv.CvtColor(img, grayImg, ColorConversion.BgrToGray);

            //String cascadeName = cascadeDictionary[comboBox1.SelectedItem];
            //CascadeClassifier cascadeClassifier = new CascadeClassifier(cascadeName);
            CvHaarClassifierCascade cascade = CvHaarClassifierCascade.FromFile(comboBox1.SelectedItem.ToString());
            CvMemStorage storage = new CvMemStorage();
            storage.Clear();

            CvSeq<CvAvgComp> faces = Cv.HaarDetectObjects(grayImg, cascade, storage, ScaleFactor, MinNeighbors, 0, new CvSize(30, 30));
            
            
            // 検出した箇所に四角をつける
            for (int i = 0; i < faces.Total; i++)
            {
                CvRect r = faces[i].Value.Rect;
                img.Rectangle(r, new CvColor(255, 0, 0));
            }

            LoadImage(img.ToBitmap());
        }



    }
}
