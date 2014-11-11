using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Reflection;
using OpenNI;



namespace XtionLib
{
    public enum NodeMode
    {
        QVGA, VGA
    }

    public class XtionUtility
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        //埋め込みData
        private string DEFAULT_XML_FILE_QVGA;
        private string DEFAULT_XML_FILE_VGA;
        private string DEFAULT_COLOR_BAR;


        //private readonly string DEFAULT_XML_FILE = 
        private Context __context;

        /// <summary>
        /// contextを取得する
        /// </summary>
        public Context context
        {
            get { return __context; }
        }

        private ImageGenerator __image;
        private DepthGenerator __depth;

       // public int putImgWidth { set; get; }
       // public int putImgHeight { set; get; }

        private int __putImgWidth;
        private int __putImgHeight;
        private List<Color> __colorBarList = new List<Color>();

        /// <summary>
        /// 画像の幅を取得
        /// </summary>
        public int putImgWidth{
            get{return __putImgWidth;}
        }
        /// <summary>
        /// 画像の高さを取得
        /// </summary>
        public int putImgHeight
        {
            get { return __putImgHeight; }
        }
        

        private const int __blockWidthNumber = 16;
        private int __blockSize;

        private float __depthMax;
        private float __depthMin;

        public float depthMax
        {
            get
            {
                return __depthMax;
            }
        }

        public float depthMin
        {
            get
            {
                return __depthMin;
            }
        }
        /// <summary>
        /// 設定ファイル指定のコンストラクタ
        /// </summary>
        /// <param name="xmlFile">デバイスの設定が記述されたXMLファイル</param>

        public XtionUtility(string xmlFile)
        {
            InitEmbeddedFile();
            ScriptNode node;

            __context = Context.CreateFromXmlFile(xmlFile, out node);
            __context.GlobalMirror = false;

            __depth = __context.FindExistingNode(NodeType.Depth) as DepthGenerator;
            __image = __context.FindExistingNode(NodeType.Image) as ImageGenerator;

            if (__image == null)
            {
                throw new Exception(__context.GlobalErrorState);
            }

            __context.StartGeneratingAll();

            this.__putImgWidth = 320;
            this.__putImgHeight = 240;
            this.__blockSize = __putImgWidth / __blockWidthNumber;


            InitColorBar();
        }

        /// <summary>
        /// 幅、高さ指定のコンストラクタ
        /// </summary>
        /// <param name="width">画像の幅</param>
        /// <param name="height">画像の高さ</param>
        /// <param name="NM">"NodeMode 解像度を640x480ならNodeMode.VGAを、320x240ならNodeMode.QVGAを使用する"</param>

        public XtionUtility(int width, int height, NodeMode NM)
        {
            InitEmbeddedFile();
            ScriptNode node;
            string currentDir = Environment.CurrentDirectory;

            using(StreamWriter sw = new StreamWriter(currentDir+@"\temp.xml")){

                if (NM == NodeMode.VGA)
                {
                    sw.Write(DEFAULT_XML_FILE_VGA);
                    
                }
                else
                {
                    sw.Write(DEFAULT_XML_FILE_QVGA);
                }
                sw.Close();
                __context = Context.CreateFromXmlFile(currentDir + @"\temp.xml", out node);
                
            }

            File.Delete(currentDir + @"\temp.xml");

            __context.GlobalMirror = false;

            __depth = __context.FindExistingNode(NodeType.Depth) as DepthGenerator;
            __image = __context.FindExistingNode(NodeType.Image) as ImageGenerator;


            if (__image == null)
            {
                throw new Exception(__context.GlobalErrorState);
            }

            __context.StartGeneratingAll();

            this.__putImgWidth = width;
            this.__putImgHeight = height;

            this.__blockSize = __putImgWidth / __blockWidthNumber;
            InitColorBar();
        }

        /// <summary>
        /// xml, 幅, 高さ指定のコンストラクタ
        /// </summary>
        /// <param name="xmlFile">デバイスの設定が記述されたXMLファイル</param>
        /// <param name="width">画像の幅</param>
        /// <param name="height">画像の高さ</param>

        public XtionUtility(string xmlFile, int width, int height)
        {
            InitEmbeddedFile();

            ScriptNode node;
            
            __context = Context.CreateFromXmlFile(xmlFile, out node);
            __context.GlobalMirror = false;
            
            __depth = __context.FindExistingNode(NodeType.Depth) as DepthGenerator;
            __image = __context.FindExistingNode(NodeType.Image) as ImageGenerator;


            if (__image == null)
            {
                throw new Exception(__context.GlobalErrorState);
            }

            __context.StartGeneratingAll();

            this.__putImgWidth = width;
            this.__putImgHeight = height;
            this.__blockSize = __putImgWidth / __blockWidthNumber;

            
            InitColorBar();
        }


        private void InitEmbeddedFile()
        {
            var assm = Assembly.GetExecutingAssembly();
            StreamReader sr1 = new StreamReader(assm.GetManifestResourceStream("XtionLib.SamplesConfigQVGA.xml"));
            StreamReader sr2 = new StreamReader(assm.GetManifestResourceStream("XtionLib.ColorBar2.txt"));
            StreamReader sr3 = new StreamReader(assm.GetManifestResourceStream("XtionLib.SampleConfigVGA.xml"));
            DEFAULT_XML_FILE_QVGA = sr1.ReadToEnd();
            DEFAULT_COLOR_BAR = sr2.ReadToEnd();
            DEFAULT_XML_FILE_VGA = sr3.ReadToEnd();

            sr1.Close();
            sr2.Close();
            sr3.Close();
        }


        private void InitColorBar()
        {
            foreach (string line in DEFAULT_COLOR_BAR.Split("\n".ToArray()))
            {
                string[] colorStr = line.Split(" ".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                __colorBarList.Add(Color.FromArgb((byte)int.Parse(colorStr[0]), (byte)int.Parse(colorStr[1]), (byte)int.Parse(colorStr[2])));  
            }
        }

        /// <summary>
        /// 指定した幅、高さのカラーバーを作成する
        /// </summary>
        /// <param name="width">画像幅</param>
        /// <param name="height">画像高さ</param>
        /// <returns>カラーバーのIplImage</returns>
        public Bitmap loadColorBar(int width, int height)
        {
            
            int index = __colorBarList.Count - 1;
            byte[] rgb = new byte[(int)(width * height) * 3];
            for (int i = 0; i < height*width; i++)
            {
                if (i % (int)((width*height)/__colorBarList.Count) == 0 && i != 0 && index > 0)
                {
                    index--;
                }
                int rgbIndex = i * 3;
                rgb[rgbIndex] = __colorBarList[index].R;
                rgb[rgbIndex + 1] = __colorBarList[index].G;
                rgb[rgbIndex + 2] = __colorBarList[index].B;
            }

            var dst = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var dstData = dst.LockBits(new Rectangle(Point.Empty, dst.Size),
                                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            unsafe
            {
                var dstScan0 = (byte*)dstData.Scan0;
                
                int i = 0;
                for (int y = 0; y < dst.Height; y++)
                {
                    for (int x = 0; x < dst.Width; x++)
                    {
                        int rgbIndex = i * 3;
                        int dPos = 3 * x + dstData.Stride * y;
                        dstScan0[dPos + 2] = rgb[rgbIndex];
                        dstScan0[dPos + 1] = rgb[rgbIndex+1];
                        dstScan0[dPos + 0] = rgb[rgbIndex+2];
                        i++;
                    }
                }
            }

            dst.UnlockBits(dstData);
            return dst;

        }

        /// <summary>
        /// 取得されたDepth値からDepthMap画像を作成する
        /// </summary>
        /// <returns>作成されたDepthmap画像</returns>
        public Bitmap doNormalDepth()
        {
//            __context.WaitOneUpdateAll(__depth);
            
            //depth情報のメタデータを取得
            DepthMetaData depthMD = __depth.GetMetaData();
            //Xの解像度*Yの解像度ぶんの16bit-int型配列を作成
            Int16[] depthArray = new Int16[depthMD.XRes * depthMD.YRes];
            //depthMap情報をすべてdepthArrayにコピーしていく
            Marshal.Copy(depthMD.DepthMapPtr, depthArray, 0, depthArray.Length);

            __depthMax = depthArray.Max();
            __depthMin = depthArray.Min();

            //depthDataSet depthSet = new depthDataSet();
            return PutNormalDepthImg(depthArray.Max(), depthMD);
        }

        private Bitmap PutNormalDepthImg(int maxDepthVal, DepthMetaData depthMD)
        {
            //depthDataSet depthSet = new depthDataSet();
            //IplImage depthImg = new IplImage(putImgWidth, putImgHeight, BitDepth.U8, 3);
            Int16[] depthArray = new Int16[depthMD.YRes * depthMD.XRes];
            Marshal.Copy(depthMD.DepthMapPtr, depthArray, 0, depthArray.Length);

            //depthSet.maxValue = maxDepthVal;
            float colorBarMax = maxDepthVal;
            float colorBarMin = 0;
            //float getDepthCount = 0;
            List<Color> colorList = new List<Color>();

            for (int y = 0; y < depthMD.YRes; y++)
            {
                for (int x = 0; x < depthMD.XRes; x++)
                {
                    //内部のカラーバーの数(標準では64)から1引く。
                    //これによりカラーバーを参照するためのIndex(0~63)の最大値を取得
                    double colorBarMaxIndex = __colorBarList.Count - 1;
                    //Depth情報をカラーバーの範囲内に正規化
                    double colorIndex = colorBarMaxIndex * (double)(depthArray[y * depthMD.XRes + x] - colorBarMin) / (double)(colorBarMax - colorBarMin);

                    
                    if (colorIndex > 63) colorIndex = 63;
                    else if (colorIndex < 0) colorIndex = 0;

                    int index = (int)colorIndex;

                    //not detected
                    if (depthArray[y * depthMD.XRes + x] == 0)
                    {
                        int R = 255;
                        int G = 255;
                        int B = 255;
                        colorList.Add(Color.FromArgb((byte)R, (byte)G, (byte)B));
                    }
                    else
                    {
                        //getDepthCount++;
                        colorList.Add(Color.FromArgb(__colorBarList[index].R, __colorBarList[index].G, __colorBarList[index].B));
                    }
                
                }
            }

            //depthSet.measurement = getDepthCount / putImgHeight / putImgWidth * 100;

            //byte[] rgb = new byte[putImgHeight * putImgWidth * 3];
            var dst = new Bitmap(putImgWidth, putImgHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            var dstData = dst.LockBits(new Rectangle(Point.Empty, dst.Size), 
                                       System.Drawing.Imaging.ImageLockMode.WriteOnly, 
                                       System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            unsafe
            {
                var dstScan0 = (byte*)dstData.Scan0;
                int i = 0;
                for (int y = 0; y < dst.Height; y++)
                {
                    for (int x = 0; x < dst.Width; x++)
                    {
                        int dPos = 3 * x + dstData.Stride * y;
                        dstScan0[dPos + 2] = colorList[i].R;
                        dstScan0[dPos + 1] = colorList[i].G;
                        dstScan0[dPos + 0] = colorList[i].B;
                        i++;
                    }
                }

            }
            dst.UnlockBits(dstData);
            return dst;     
        }

        /// <summary>
        /// RGBカメラから画像を取得する
        /// </summary>
        /// <returns></returns>
        public Bitmap doColor()
        {

//            __context.WaitOneUpdateAll(__image);

            ImageMetaData imd = __image.GetMetaData();

            lock (this)
            {
                //**************************************//
                //***********RGB Camera Feed************//
                //**************************************//
                var dst = new Bitmap(putImgWidth, putImgHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Rectangle rect = new Rectangle(0, 0, dst.Width, dst.Height);
                var data = dst.LockBits(rect,System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                unsafe
                {
                    byte* pDest = (byte*)data.Scan0.ToPointer();
                    byte* imstp = (byte*)__image.ImageMapPtr.ToPointer();

                    // set pixels
                    for (int i = 0; i < imd.DataSize; i += 3, pDest += 3, imstp += 3)
                    {
                        pDest[0] = imstp[2];
                        pDest[1] = imstp[1];
                        pDest[2] = imstp[0];
                    }
                }
                dst.UnlockBits(data);
                return dst;
            }   
        }
        
        /// <summary>
        /// CSVフォーマットで指定した場所に距離情報を保存する
        /// </summary>
        /// <param name="FilePath">保存場所の絶対パス</param>
        public void SaveDepthCSV(string FilePath)
        {
            __context.WaitOneUpdateAll(__depth);

            StreamWriter sw = new StreamWriter(FilePath+@"\depthMap.csv");

            DepthMetaData depthMD = __depth.GetMetaData();
            Int16[] depthArray = new Int16[depthMD.XRes * depthMD.YRes];
            Marshal.Copy(depthMD.DepthMapPtr, depthArray, 0, depthArray.Length);

            for (int y = 0; y < depthMD.YRes; y++)
            {
                string data = "";
                for (int x = 0; x < depthMD.XRes; x++)
                {
                    data += depthArray[y * depthMD.XRes + x].ToString() + ", ";

                }
                int index = data.Length - 1;
                string reData = data.Remove(index, 1);
                sw.WriteLine(reData);
            }
        }

        /// <summary>
        /// RGBカメラの位置にdepthカメラの結果を合わせる
        /// </summary>
        public void alterNate()
        {
            AlternativeViewpointCapability avc = __depth.AlternativeViewpointCapability;
            avc.SetViewpoint(__image);
        }

        /// <summary>
        /// WPF向け、WindowFormのBitmapからWPFのImageSourceに変換する
        /// </summary>
        /// <param name="bmp">WindowFormのBitmap</param>
        /// <returns>WPFのImageSource</returns>

        public System.Windows.Media.ImageSource ConvertImageSourceFromBitmap(Bitmap bmp)
        {
            IntPtr hBitmap = bmp.GetHbitmap();
            try
            {
                var bmpSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap,
                    IntPtr.Zero, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                return (System.Windows.Media.ImageSource)bmpSource;
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return null;
        }
    }


}
