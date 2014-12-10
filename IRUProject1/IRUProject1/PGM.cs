using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace IRUProject1
{
    class PGM
    {
        private List<byte> imgData=new List<byte>();
        public int Width { get; set; }
        public int Height{ get; set; }
        public int MaxLevel { get; set; }


        public PGM(string file)
        {
            using (System.IO.StreamReader sr = new System.IO.StreamReader(file))
            {
                if (sr.ReadLine() != "P2")
                {
                    throw new Exception("File Open Error");
                }
                else
                {
                    string[] str = sr.ReadLine().Split(' ');
                    this.Width = int.Parse(str[0]);
                    this.Height = int.Parse(str[1]);
                    this.MaxLevel = int.Parse(sr.ReadLine());
                }


                while (!sr.EndOfStream)
                {
                    string[] vals = sr.ReadLine().Split(' ');
                    foreach (string str in vals)
                    {
                        if (str != "")
                        {
                            byte byteValue = byte.Parse(str);

                            imgData.Add(byteValue);
                        }
                    }
                }

                sr.Close();
            }
        }

        public PGM(int width, int height, byte[] datas)
        {
            Width = width;
            Height = height;
            MaxLevel = datas.Max();
            foreach (byte b in datas)
            {
                imgData.Add(b);
            }
            
        }

        /// <summary>
        /// 幅方向と高さ方向をサイズアップさせる
        /// </summary>
        /// <param name="width">変更後の幅</param>
        /// <param name="height">変更後の高さ</param>
        public void scaleTo(int width, int height)
        {
            List<byte> resizedImage = new List<byte>();
            double scalingX = (double)width / (double)this.Width;
            double scalingY = (double)height / (double)this.Height;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    byte pixelValue = this[(int)((double)i/(double)scalingY),(int)((double)j/(double)scalingX)];
                    resizedImage.Add(pixelValue);
                }
            }

            this.imgData = resizedImage;
            this.Width = width;
            this.Height = height;

        }

        public void Show()
        {
            Form f = new Form();
            f.Width = this.Width + 50;
            f.Height = this.Height + 50;
            f.Text = @"PGMData";
            PictureBox pb = new PictureBox();
            pb.Size = new Size(Width, Height);
            pb.Location = new Point(25, 25);
            pb.Image = this.toBitmap();
            f.Controls.Add(pb);
            f.ShowDialog();
            f.Dispose();
        }

        unsafe public Bitmap toBitmap()
        {
            Bitmap bmp = new Bitmap(Width, Height);
            //Bitmapの高速書き込み
            //http://daisy64.blogspot.jp/2009/01/getpixel.html
            
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            int nResidual = bmpData.Stride - Width * 3;
            byte* p = (byte*)bmpData.Scan0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    p[2] = this[y, x];
                    p[1] = this[y, x];
                    p[0] = this[y, x];
                    p += 3;
                }
                p += nResidual;
            }
            bmp.UnlockBits(bmpData);
            return bmp;
        }
       
        /// <summary>
        /// インデクサーの定義
        /// </summary>
        /// <param name="i">y方向のインデックス</param>
        /// <param name="j">x方向のインデックス</param>
        /// <returns></returns>
        public byte this[int i, int j]
        {
            set
            {
                imgData[i * Width + j] = value;
            }
            get
            {
                byte val = imgData[i * Width + j];
                return val;
            }
        }
        /// <summary>
        /// インデクサーの定義(一次元)
        /// </summary>
        /// <param name="i">一次元上でのインデックス</param>
        /// <returns></returns>
        public byte this[int i]
        {
            set
            {
                imgData[i] = value;
            }
            get
            {
                return imgData[i];
            }
        }

 

    }
}
