using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCvSharp;

namespace LabImg
{
    /// <summary>
    /// 晴佐久が必要だと思ったツールをまとめたライブラリ
    /// </summary>
    public static class HTUtility
    {
        /// <summary>
        /// ファイルパスからCSVファイルを読み取る
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>float配列に格納する</returns>
        public static float[,] loadCSV(string path)
        {

            float[,] csvArray;

            using (System.IO.StreamReader sr = new System.IO.StreamReader(path, System.Text.Encoding.Default))
            {
                string str = "";
                List<string> arrText = new List<string>();

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
                csvArray = new float[line_count, col_count];
                int a = 0, b = 0;
                foreach (string sOut in arrText)
                {
                    string[] temp_line = sOut.Split(',');
                    foreach (string value in temp_line)
                    {
                        if (value != "")
                        {
                            csvArray[a, b] = float.Parse(value);
                            b++;
                        }
                    }
                    b = 0;
                    a++;
                }

                sr.Close();
            }
            
           
            return csvArray;
        }

        /// <summary>
        /// 入力画像にHough変換をかけて出力画像に出力する
        /// </summary>
        /// <param name="src">入力画像</param>
        /// <param name="dst">出力画像</param>
        /// <param name="threshold">閾値(defaultでは120)</param>
        /// <param name="thickness">線の太さ(defaultでは1)</param>
        /// <param name="rho">Hough変換につかうρの値(defaultでは1)</param>
        /// <param name="theta">Hough変換につかうΘの値(defaultではπ/180)</param>
        public static void HoughConversion(IplImage src, IplImage dst,int threshold = 120,int thickness=1,  double rho=1, double theta=Cv.PI/180)
        {
            IplImage workImg = new IplImage(src.Size, BitDepth.U8, 1);
            if (src.NChannels != 1)
            {
                Cv.CvtColor(src, workImg, ColorConversion.BgrToGray);
            }
            else
            {
                workImg = src.Clone();
            }
            Cv.Canny(workImg, workImg, 50, 200, ApertureSize.Size3);
            CvMemStorage storage = new CvMemStorage();

            CvSeq lines = Cv.HoughLines2(workImg, storage, HoughLinesMethod.Standard, rho,theta, threshold);

            for (int i = 0; i < lines.Total; i++)
            {
                CvLineSegmentPolar elem = lines.GetSeqElem<CvLineSegmentPolar>(i).Value;
                float rhoValue = elem.Rho;
                float thetaValue = elem.Theta;
                double a = Math.Cos(thetaValue);
                double b = Math.Sin(thetaValue);

                double x0 = a * rhoValue;
                double y0 = b * rhoValue;

                CvPoint pt1 = new CvPoint(Cv.Round(x0 + 1000 * (-b)), Cv.Round(y0 + 1000 * (a)));
                CvPoint pt2 = new CvPoint(Cv.Round(x0 - 1000 * (-b)), Cv.Round(y0 - 1000 * (a)));
                dst.DrawLine(pt1, pt2, CvColor.Black, thickness, LineType.AntiAlias);
                //dst.Line(pt1, pt2, CvColor.Red, 1, LineType.AntiAlias, 0);
            }

            lines.Dispose();
            storage.Dispose();
        }

        /// <summary>
        /// Disparity画像から距離画像へ変換する
        /// </summary>
        /// <param name="img">入力画像</param>
        /// <param name="convParam">convert Parameter(Q(4, 3))</param>
        /// <param name="focusLength">focus Length(Q(3, 4))</param>
        /// <returns></returns>
        public static IplImage DisparityToDepthMap(IplImage img, float convParam, float focusLength)
        {
            IplImage returnImage = new IplImage(img.Width, img.Height, BitDepth.F32, 1);
            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    //int offsetImg = x + y * img.Width;
                    float disp = (float)img[y, x].Val0;
                    returnImage[y, x] = focusLength / (disp * convParam);
                }
            }

            return returnImage;
        }
    
    }



}
