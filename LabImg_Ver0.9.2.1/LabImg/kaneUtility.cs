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
	/// Kanemotoの作成したクラスライブラリー
	/// </summary>
	public static class kaneUtility
	{
		/// <summary>
		/// 矩形領域を抽出する。左上と右下座標を指定。IplImageは、Byte単位のRGBかGray
		/// </summary>
		/// <param name="img"></param>
		/// <param name="xs"></param>
		/// <param name="ys"></param>
		/// <param name="xe"></param>
		/// <param name="ye"></param>
		/// <returns></returns>
		public static IplImage getRectSE(IplImage img, int xs, int ys, int xe, int ye)
		{
			if (xs < 0) xs = 0;
			if (xe >= img.Width) xe = img.Width - 1;
			if (ys < 0) ys = 0;
			if (ye >= img.Height) ye = img.Height - 1;
			int w = xe - xs + 1;
			int h = ye - ys + 1;
			int nc = img.NChannels;
			IplImage rimg = new IplImage(w,h,img.Depth,nc);
			unsafe
			{
				byte* ptrSrc = (byte*)img.ImageData;
				byte* ptrDst = (byte*)rimg.ImageData;
				for (int x = 0; x < w; x++)
				{
					for (int y = 0; y < h; y++)
					{
						int offsetSrc = (img.WidthStep * (y+ys)) + nc * (x+xs);
						int offsetDst = (rimg.WidthStep * y) + nc * x;
						for (int n = 0; n < nc; n++)
						{
							ptrDst[offsetDst+n] = ptrSrc[offsetSrc+n];
						}
					}
				}
			}
			return rimg;
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
			if ((xs+w-1) >= img.Width) w = img.Width-xs;
			if (ys < 0) ys = 0;
			if ((ys+h-1) >= img.Height) h = img.Height - ys;
			int nc = img.NChannels;
			IplImage rimg = new IplImage(w, h, img.Depth, nc);
			unsafe
			{
				byte* ptrSrc = (byte*)img.ImageData;
				byte* ptrDst = (byte*)rimg.ImageData;
				for (int x = 0; x < w; x++)
				{
					for (int y = 0; y < h; y++)
					{
						int offsetSrc = (img.WidthStep * (y + ys)) + nc * (x + xs);
						int offsetDst = (rimg.WidthStep * y) + nc * x;
						for (int n = 0; n < nc; n++)
						{
							ptrDst[offsetDst + n] = ptrSrc[offsetSrc + n];
						}
					}
				}
			}
			return rimg;
		}
		/// <summary>
		/// 時計回りに９０度回転
		/// </summary>
		/// <param name="img"></param>
		/// <returns></returns>
		public static IplImage imageRotation90(IplImage img)
		{
			IplImage img90 = new IplImage(img.Height, img.Width, img.Depth, img.NChannels);
			for (int i = 0; i < img.Height; i++)
				for (int j = 0; j < img.Width; j++)
					img90[j, img.Height - i - 1] = img[i, j];
			return img90;
		}
		/// <summary>
		/// 時計回りに１８０度回転
		/// </summary>
		/// <param name="img"></param>
		/// <returns></returns>
		public static IplImage imageRotation180(IplImage img)
		{
			IplImage img180 = new IplImage(img.Width, img.Height, img.Depth, img.NChannels);
			for (int i = 0; i < img.Height; i++)
				for (int j = 0; j < img.Width; j++)
					img180[img.Height - i - 1, img.Width - j - 1] = img[i, j];
			return img180;
		}
		/// <summary>
		/// 時計回りに２７０度回転（反時計回りに９０度）
		/// </summary>
		/// <param name="img"></param>
		/// <returns></returns>
		public static IplImage imageRotation270(IplImage img)
		{
			IplImage img270 = new IplImage(img.Height, img.Width, img.Depth, img.NChannels);
			for (int i = 0; i < img.Height; i++)
				for (int j = 0; j < img.Width; j++)
					img270[img.Width - j - 1, i] = img[i, j];
			return img270;
		}
		/// <summary>
		/// Eular回転行列う
		/// </summary>
		/// <param name="az">Z軸周りの回転(deg)</param>
		/// <param name="by">Y軸周りの回転(deg)</param>
		/// <param name="gz">新Z軸周りの回転(deg)</param>
		/// <returns></returns>
		public static CvMat rotationMatrix(float az, float by, float gz)
		{

			float pi = (float)Math.PI / 180.0f;
			float cs1 = (float)Math.Cos(az * pi);
			float ss1 = (float)Math.Sin(az * pi);
			float cs2 = (float)Math.Cos(by * pi);
			float ss2 = (float)Math.Sin(by * pi);
			float cs3 = (float)Math.Cos(gz * pi);
			float ss3 = (float)Math.Sin(gz * pi);
			CvMat R1 = new CvMat(3, 3, MatrixType.F32C1);
			CvMat R2 = new CvMat(3, 3, MatrixType.F32C1);
			CvMat R3 = new CvMat(3, 3, MatrixType.F32C1);
			CvMat R = new CvMat(3, 3, MatrixType.F32C1);
			R1.SetZero();
			R2.SetZero();
			R3.SetZero();
			R1[0, 0] = cs1; R1[0, 1] = -ss1; R1[1, 0] = ss1; R1[1, 1] = cs1; R1[2, 2] = 1.0f;
			R2[0, 0] = cs2; R2[0, 2] = ss2; R2[1, 1] = 1.0f; R2[2, 0] = -ss2; R2[2, 2] = cs2;
			R3[0, 0] = cs3; R3[0, 1] = -ss3; R3[1, 0] = ss3; R3[1, 1] = cs3; R3[2, 2] = 1.0f;
			R = R1 * R2 * R3;
			return R;
		}
		/// <summary>
		/// 一回微分画像を作成（入力は、グレースケール。
		/// </summary>
		/// <param name="img"></param>
		/// <returns>戻り値は振幅と位相を２ｃｈのF32ビットのIplImageで戻す</returns>
		public static IplImage imageGradient(IplImage img)
		{
			IplImage FG = new IplImage(img.Height, img.Width, BitDepth.F32, 2);
			for (int y = 0; y < img.Height; y++)
			{
				for (int x = 0; x < img.Width; x++)
				{
					float gx = (float)(img[y , x+1].Val0 - img[y , x-1].Val0) / 2;
					float gy = (float)(img[y + 1, x].Val0 - img[y - 1, x].Val0) / 2;
					FG[y, x] = gx;
					//FG[y, x].Val1 = gy;
				}
			}
			return FG;
		}

		/// <summary>
		/// 画像読み込みとグレースケール、γ補正変換
		/// </summary>
		/// <param name="imgPath"></param>
		/// <param name="gamma">２．２が標準</param>
		/// <returns>0-1のグレイスケールで読み込み</returns>
		public static IplImage floatImageRead(string imgPath, float gamma)
		{
			//IplImage img = new IplImage(imgPath, LoadMode.GrayScale);
			IplImage imgRGB = new IplImage(imgPath);
			IplImage FG = new IplImage(imgRGB.Height, imgRGB.Width,BitDepth.U8,1);

			unsafe
			{
				byte* ptrSrc = (byte*)imgRGB.ImageData;
				double gammai = 1.0 / (double)gamma;
				for (int y = 0; y < imgRGB.Height; y++)
				{
					for (int x = 0; x < imgRGB.Width; x++)
					{
						int offsetRGB = (imgRGB.WidthStep * y) + 3 * x;
						//double gg= 0.298912 * (double)ptrSrc[offsetRGB+2] + 0.586611 * (double)ptrSrc[offsetRGB+1] + 0.114478 * (double)ptrSrc[offsetRGB];
						//if (gg > 255.0) gg = 255.0;
						//ptrDst[offsetG] = (byte)gg;
						//グレイスケール化を行いガンマ補正
						//参考URL:http://homepage2.nifty.com/tsugu/sotuken/ronbun/sec3-2.html#0007

						double R = 0.222015 * (double)ptrSrc[offsetRGB + 2];
						double G = 0.706655 * (double)ptrSrc[offsetRGB + 1];
						double B = 0.071330 * (double)ptrSrc[offsetRGB];
						double gg = Math.Pow((R + B + G)/255,gammai)*255.0;

						if (gg > 255.0) gg = 255.0;
						FG[y, x] = (byte)gg;
					}
				}
			}
			return FG;
		}
		/// <summary>
		/// 画像読み込みとグレースケール、γ補正変換
		/// </summary>
		/// <param name="imgPath"></param>
		/// <param name="gamma">gamma=2.2が標準</param>
		/// <returns></returns>
		public static IplImage grayImageRead(string imgPath, float gamma)
		{
			//IplImage img = new IplImage(imgPath, LoadMode.GrayScale);
			IplImage imgRGB = new IplImage(imgPath);
			IplImage imgG = new IplImage(imgRGB.Width, imgRGB.Height, BitDepth.U8, 1);

			unsafe
			{
				byte* ptrSrc = (byte*)imgRGB.ImageData;
				byte* ptrDst = (byte*)imgG.ImageData;
				double gammai = 1.0 / (double)gamma;
				for (int y = 0; y < imgRGB.Height; y++)
				{
					for (int x = 0; x < imgRGB.Width; x++)
					{
						int offsetRGB = (imgRGB.WidthStep * y) + 3 * x;
						int offsetG = (imgG.WidthStep * y) + x;
						//double gg= 0.298912 * (double)ptrSrc[offsetRGB+2] + 0.586611 * (double)ptrSrc[offsetRGB+1] + 0.114478 * (double)ptrSrc[offsetRGB];
						//if (gg > 255.0) gg = 255.0;
						//ptrDst[offsetG] = (byte)gg;
						//double R = 0.222015 * Math.Pow((double)ptrSrc[offsetRGB + 2] / 255.0, (double)gamma);
						//double G = 0.706655 * Math.Pow((double)ptrSrc[offsetRGB + 1] / 255.0, (double)gamma);
						//double B = 0.071330 * Math.Pow((double)ptrSrc[offsetRGB] / 255.0, (double)gamma);
						//double gg = Math.Pow((R + G + B), gammai) * 255.0;

						//グレイスケール化を行いガンマ補正
						double R = 0.222015 * (double)ptrSrc[offsetRGB + 2];
						double G = 0.706655 * (double)ptrSrc[offsetRGB + 1];
						double B = 0.071330 * (double)ptrSrc[offsetRGB];
						double gg = 255*Math.Pow((R + B + G) / 255, gammai);

						if (gg > 255.0) gg = 255.0;
						ptrDst[offsetG] = (byte)gg;
					}
				}
			}
			return imgG;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="img"></param>
		/// <returns></returns>
		public static IplImage PyramidUp(IplImage img)
		{
			int nx = img.Width;
			int ny = img.Height;
			IplImage NewD = new IplImage(nx * 2, ny * 2, img.Depth, img.NChannels);
			for (int i = 0; i < nx; i++)
			{
				for (int j = 0; j < ny; j++)
				{
					int i2 = i * 2; int j2 = j * 2;
					int i21 = i2 + 1; int j21 = j2 + 1;
					NewD[j2, i2] = NewD[j21, i2] = NewD[j2, i21] = NewD[j21, i21] = img[j, i];
				}
			}
			return NewD;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="img"></param>
		/// <returns></returns>
		public static IplImage PyramidDown(IplImage img)
		{
			int nx2 = img.Width / 2;
			int ny2 = img.Height / 2;

			IplImage NewD = new IplImage(nx2, ny2, BitDepth.U8, img.NChannels);
			if (img.NChannels == 1)
			{
				for (int i = 0; i < nx2; i++)
				{
					for (int j = 0; j < ny2; j++)
					{
						int i2 = i * 2; int j2 = j * 2;
						int i21 = i2 + 1; int j21 = j2 + 1;

						float B = (float)(img[j2, i2].Val0 + img[j21, i2].Val0 + img[j2, i21].Val0 + img[j21, i21].Val0) / 4.0f;
						NewD[j, i] = (float)B;
					}
				}
			}
			else if(img.NChannels==3)
			{
				for (int i = 0; i < nx2; i++)
				{
					for (int j = 0; j < ny2; j++)
					{
						int i2 = i * 2; int j2 = j * 2;
						int i21 = i2 + 1; int j21 = j2 + 1;

						double B = (img[j2, i2].Val0 + img[j21, i2].Val0 + img[j2, i21].Val0 + img[j21, i21].Val0) / 4;
						double G = (img[j2, i2].Val1 + img[j21, i2].Val1 + img[j2, i21].Val1 + img[j21, i21].Val1) / 4;
						double R = (img[j2, i2].Val2 + img[j21, i2].Val2 + img[j2, i21].Val2 + img[j21, i21].Val2) / 4;
						NewD[j, i] = new CvScalar(B, G, R);
					}
				}
			}
			return NewD;
		}
        /// <summary>
        /// 二次元floatアレイの最大値を返す
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static float floatArrayMax(float[,] S)
        {
            float fmax = float.MinValue;
            int ht = S.GetLength(0);
            int wd = S.GetLength(1);
            for (int y = 0; y < ht; y++)
            {
                for (int x = 0; x < wd; x++)
                {
                    if (fmax < S[y, x]) fmax = S[y, x];
                }
            }
            return fmax;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="S"></param>
        /// <returns></returns>
        public static float floatArrayMin(float[,] S)
        {
            float fmin = float.MaxValue;
            int ht = S.GetLength(0);
            int wd = S.GetLength(1);
            for (int y = 0; y < ht; y++)
            {
                for (int x = 0; x < wd; x++)
                {
                    if (fmin > S[y, x]) fmin = S[y, x];
                }
            }
            return fmin;
        }
        /// <summary>
        /// 行列の絶対値の加算
        /// </summary>
        /// <param name="E"></param>
        /// <returns></returns>
        public static float matrixAbsSum(float[,] E)
        {
            float sum = 0;
            for (int y = 0; y < E.GetLength(0); y++)
            {
                for (int x = 0; x < E.GetLength(1); x++)
                {
                    sum += Math.Abs(E[y, x]);
                }
            }
            return sum;
        }
        /// <summary>
        /// 行列の差を計算
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static float[,] matrixDiff(float[,] A, float[,] B)
        {
            int ht = A.GetLength(0);
            int wd = A.GetLength(1);
            float[,] D = new float[ht, wd];
            for (int y = 0; y < ht; y++)
            {
                for (int x = 0; x < wd; x++)
                {
                    D[y, x] = A[y, x] - B[y, x];
                }
            }
            return D;
        }
		/// <summary>
		/// IplImage.Val0　の最大値を求める
		/// </summary>
		/// <param name="I"></param>
		/// <returns></returns>
		public static float maxIplImage(IplImage I)
		{
			int NY = I.Height;
			int NX = I.Width;
			float fmax = float.MinValue;
			for (int y = 0; y < NY; y++)
			{
				for (int x = 0; x < NX; x++)
				{
					if (fmax < I[y, x].Val0)
					{
						fmax = (float)I[y, x].Val0;
					}
				}
			}
			return fmax;
		}
		/// <summary>
		/// IplImage.Val0　の最小値を求める
		/// </summary>
		/// <param name="I"></param>
		/// <returns></returns>
		public static float minIplImage(IplImage I)
		{
			int NY = I.Height;
			int NX = I.Width;
			float fmin = float.MaxValue;
			for (int y = 0; y < NY; y++)
			{
				for (int x = 0; x < NX; x++)
				{
					if (fmin > I[y, x].Val0)
					{
						fmin = (float)I[y, x].Val0;
					}
				}
			}
			return fmin;
		}

        /// <summary>
        /// 1チャンネルのIplImageの輝度値の最大値を返す
        /// </summary>
        /// <param name="I"></param>
        /// <returns></returns>
        public static float maxIplImageVal(IplImage I)
        {
            float maxVal = float.MinValue;

            unsafe{
                byte* IPtr = (byte*)I.ImageData;
                for(int y=0;y < I.Height;y++){
                    for(int x = 0;x<I.Width;x++){
                        int offset = x+y*I.WidthStep;

                        if (maxVal < IPtr[offset])
                        {
                            maxVal = IPtr[offset];
                        }

                    }

                }

            }

            return maxVal;

        }


        /// <summary>
        /// 1チャンネルのIplImageの輝度値の最少値を返す
        /// </summary>
        /// <param name="I"></param>
        /// <returns></returns>
        public static float minIplImageVal(IplImage I)
        {
            float minVal = float.MaxValue;

            unsafe
            {
                byte* IPtr = (byte*)I.ImageData;
                for (int y = 0; y < I.Height; y++)
                {
                    for (int x = 0; x < I.Width; x++)
                    {
                        int offset = x + y * I.WidthStep;

                        if (minVal > IPtr[offset])
                        {
                            minVal = IPtr[offset];
                        }

                    }

                }

            }

            return minVal;

        }

		/// <summary>
		/// 二次元行列全体CvMatの最大値 
		/// </summary>
		/// <param name="D"></param>
		/// <returns></returns>
		public static float maxCvMat(CvMat D)
		{
			float dmax = (float)D.Max(x => x.Val0);
			return dmax;
		}
		/// <summary>
		/// 二次元行列全体CvMatの最小値 
		/// </summary>
		/// <param name="D"></param>
		/// <returns></returns>
		public static float minCvMat(CvMat D)
		{
			float dmin = (float)D.Min(x => x.Val0);
			return dmin;
		}
		/// <summary>
		/// ２変数の大きい方を返す
		/// </summary>
		/// <param name="F1"></param>
		/// <param name="F2"></param>
		/// <returns></returns>
		public static float maxFloat2(float F1, float F2)
		{
			float fmax = F1;
			if (fmax < F2) fmax = F2;
			return fmax;
		}
		/// <summary>
		/// ２変数の小さい方を返す
		/// </summary>
		/// <param name="F1"></param>
		/// <param name="F2"></param>
		/// <returns></returns>
		public static float minFloat2(float F1, float F2)
		{
			float fmin = F1;
			if (fmin > F2) fmin = F2;
			return fmin;
		}
		/// <summary>
		/// Float型のCvMatを、F32のIplImageに変換する
		/// </summary>
		/// <param name="D"></param>
		/// <returns></returns>
		public static IplImage CvMatFloatToIplImage(CvMat D)
		{
			IplImage DF = new IplImage(D.Width, D.Height, BitDepth.F32, 1);
			Cv.Scale(D,DF);
			return DF;

		}
		/// <summary>
		///  F32のIplImageを、Float型のCvMatに変換する
		/// </summary>
		/// <param name="D"></param>
		/// <returns></returns>
		public static CvMat IplImageFloatToCvMat(IplImage D)
		{
			CvMat DF = new CvMat(D.Height, D.Width, MatrixType.F32C1);
			Cv.Scale(D, DF);
			return DF;

		}
		/// <summary>
		///  IplImage(Gray　または　RGB）を、Float型のCvMatに変換する。RGB型は、グレイスケールに自動変換する
		/// </summary>
		/// <param name="D"></param>
		/// <returns></returns>
		public static CvMat IplImageToCvMat(IplImage D)
		{
			CvMat DF = new CvMat(D.Height, D.Width, MatrixType.F32C1);
			int nc = D.NChannels;
			unsafe
			{
				byte* ptrSrc = (byte*)D.ImageData;
				for (int y = 0; y < D.Height; y++)
				{
					for (int x = 0; x < D.Width; x++)
					{
						if (nc == 1)
						{
							DF[y, x] = (float)ptrSrc[(D.WidthStep * y) +  x];
						}
						else if (nc == 3)
						{
							int offsetRGB = (D.WidthStep * y) + 3 * x;
							//double gg= 0.298912 * (double)ptrSrc[offsetRGB+2] + 0.586611 * (double)ptrSrc[offsetRGB+1] + 0.114478 * (double)ptrSrc[offsetRGB];
							//if (gg > 255.0) gg = 255.0;
							//ptrDst[offsetG] = (byte)gg;
							//グレイスケール化を行いガンマ補正
							//参考URL:http://homepage2.nifty.com/tsugu/sotuken/ronbun/sec3-2.html#0007

							double R = 0.222015 * (double)ptrSrc[offsetRGB + 2];
							double G = 0.706655 * (double)ptrSrc[offsetRGB + 1];
							double B = 0.071330 * (double)ptrSrc[offsetRGB];
							double gg = R + B + G;
							if (gg > 255.0) gg = 255.0;
							DF[y, x] = (float)gg;
						}
						else
						{
							DF[y, x] = 0.0f;
						}
					}
				}
			}           
			return DF;
		}
		/// <summary>
		/// FLoatArryをIplImageに変換する。
		/// </summary>
		/// <param name="D"></param>
		/// <param name="dmin"></param>
		/// <param name="dmax"></param>
		/// <param name="Gray">’G’でグレイスケール、それ以外はRGB</param>
		/// <returns></returns>
		public static IplImage FloatArrayToIplImage(FloatArray D, float dmin, float dmax, char Gray)
		{
			int H = D.size0;
			int W = D.size1;
			unsafe
			{
				if (Gray == 'G' || Gray == 'g')
				{
					IplImage img = new IplImage(W, H, BitDepth.U8, 1);
					byte* ptrSrc = (byte*)img.ImageData;

					for (int y = 0; y < H; y++)
					{
						for (int x = 0; x < W; x++)
						{
							int gry = (int)(255.0f * (D[y, x] - dmin) / (dmax - dmin));
							if (gry < 0) gry = 0;
							if (gry > 255) gry = 255;
							int offset = (img.WidthStep * y) + x;
							ptrSrc[offset] = (byte)gry;
						}
					}
					return img;
				}
				else
				{
					IplImage img = new IplImage(W, H, BitDepth.U8, 3);
					byte* ptrSrc = (byte*)img.ImageData;

					for (int y = 0; y < H; y++)
					{
						for (int x = 0; x < W; x++)
						{
							Color col = kaneUtility.GetScaleColor((D[y, x] - dmin) / (dmax - dmin));
							int offset = (img.WidthStep * y) + 3 * x;
							ptrSrc[offset] = col.B;
							ptrSrc[offset + 1] = col.G;
							ptrSrc[offset + 2] = col.R;
						}
					}
					return img;
				}
			}
		}
		/// <summary>
		/// IplImageをFloatArrayに変換する。
		/// </summary>
		/// <param name="src">ソースIplImage</param>
		/// <returns>FloatArray(Heught,Width)</returns>
		public static FloatArray IplImageToFloatArray(IplImage src)
		{
			FloatArray dstArray = new FloatArray(src.Height, src.Width);
			unsafe
			{
				int nc = src.NChannels;

				if (nc == 1)
				{
					short* ptrSrci = (short*)src.ImageData;
					byte ix = (byte)src.Depth;
					if (ix == 16)
					{
						for (int y = 0; y < src.Height; y++)
						{
							for (int x = 0; x < src.Width; x++)
							{

								int offset = (src.WidthStep * y) / 2 + x;
								dstArray[y, x] = (float)ptrSrci[offset];
							}
						}
					}
					else if(ix==32) {
						for (int y = 0; y < src.Height; y++)
						{
							for (int x = 0; x < src.Width; x++)
							{

								int offset = (src.WidthStep * y) / 4 + x;
								dstArray[y, x] = (float)ptrSrci[offset];
							}
						}                    
					}
					else
					{
						byte* ptrSrc = (byte*)src.ImageData;
						for (int y = 0; y < src.Height; y++)
						{
							for (int x = 0; x < src.Width; x++)
							{

								int offset = (src.WidthStep * y) + x;
								dstArray[y, x] = (float)ptrSrc[offset];
							}
						}
					}
				}
				else if (nc == 3)
				{
					byte* ptrSrc24 = (byte*)src.ImageData;
					float w;
					for (int y = 0; y < src.Height; y++)
					{
						for (int x = 0; x < src.Width; x++)
						{
							int offset1 = (src.WidthStep * y) + 3 * x;
							w = (float)ptrSrc24[offset1] * (float)ptrSrc24[offset1] + (float)ptrSrc24[offset1 + 1] * (float)ptrSrc24[offset1 + 1] + (float)ptrSrc24[offset1 + 2] * (float)ptrSrc24[offset1 + 2];
							dstArray[y, x] = (float)Math.Sqrt((double)w);
						}
					}
				}
			}
			return dstArray;
		}
		/// <summary>
		/// IplImageを、FloatArrayデータに変換数る
		/// </summary>
		/// <param name="src">ソース構造体</param>
		/// <param name="RGB">R,G,B,Oを、Byteデータで指定する</param>
		/// <returns></returns>
		public static FloatArray IplImageToFloatArray(IplImage src, char RGB)
		{
			FloatArray dstArray = new FloatArray(src.Height, src.Width);
			unsafe
			{
				byte* ptrSrc = (byte*)src.ImageData;
				int nc = src.NChannels;

				if (nc == 1)
				{
					for (int y = 0; y < src.Height; y++)
					{
						for (int x = 0; x < src.Width; x++)
						{
							int offset = (src.WidthStep * y) + x;
							dstArray[y, x] = (float)ptrSrc[offset];
						}
					}
				}
				else if (RGB == 'R' || RGB == 'r')
				{
					for (int y = 0; y < src.Height; y++)
					{
						for (int x = 0; x < src.Width; x++)
						{
							int offset = (src.WidthStep * y) + 3 * x+2;
							dstArray[y, x] = (float)ptrSrc[offset];
						}
					}
				}
				else if (RGB == 'G' || RGB == 'g')
				{
					for (int y = 0; y < src.Height; y++)
					{
						for (int x = 0; x < src.Width; x++)
						{
							int offset = (src.WidthStep * y) + 3 * x + 1;
							dstArray[y, x] = (float)ptrSrc[offset];
						}
					}
				}
				else if (RGB == 'B' || RGB == 'b')
				{
					for (int y = 0; y < src.Height; y++)
					{
						for (int x = 0; x < src.Width; x++)
						{
							int offset = (src.WidthStep * y) + 3 * x;
							dstArray[y, x] = (float)ptrSrc[offset];
						}
					}
				}
				else if (nc == 3)
				{
					for (int y = 0; y < src.Height; y++)
					{
						for (int x = 0; x < src.Width; x++)
						{
							int offset1 = (src.WidthStep * y) + 3 * x;
							dstArray[y, x] = (float)ptrSrc[offset1] * (float)ptrSrc[offset1] + (float)ptrSrc[offset1 + 1] * (float)ptrSrc[offset1 + 1] + (float)ptrSrc[offset1 + 2] * (float)ptrSrc[offset1 + 2];
						}
					}
				}
			}
			return dstArray;
		}

		/// <summary>
		/// IplImageをDoubleArrayに変換する。
		/// </summary>
		/// <param name="src">ソースIplImage</param>
		/// <returns>DoubleArray(Heught,Width)</returns>
		public static DoubleArray IplImageToDoubleArray(IplImage src)
		{
			DoubleArray dstArray = new DoubleArray(src.Height, src.Width);
			unsafe
			{
				byte* ptrSrc = (byte*)src.ImageData;
				int nc = src.NChannels;

				if (nc == 1)
				{
					for (int y = 0; y < src.Height; y++)
					{
						for (int x = 0; x < src.Width; x++)
						{
							int offset = (src.WidthStep * y) + x;
							dstArray[y, x] = (double)ptrSrc[offset];
						}
					}
				}
				else if (nc == 3)
				{
					for (int y = 0; y < src.Height; y++)
					{
						for (int x = 0; x < src.Width; x++)
						{
							int offset = (src.WidthStep * y) + x * 3;
							dstArray[y, x] = (double)ptrSrc[offset];
						}
					}
				}
			}
			return dstArray;
		}
		/// <summary>
		/// 二次元配列の濃淡表示
		/// </summary>
		/// <param name="Disp"></param>
		/// <param name="dmin"></param>
		/// <param name="dmax"></param>
		/// <param name="PicBox"></param>
		/// <param name="ColorBar"></param>
		/// <param name="tbmax"></param>
		/// <param name="tbmin"></param>
		public static void SetDisparity(CvMat Disp, float dmin, float dmax, PictureBox PicBox, PictureBox ColorBar, TextBox tbmax, TextBox tbmin)
		{
			IplImage D=new IplImage(Disp.Width,Disp.Height,BitDepth.F32,1);
			Cv.Scale(Disp,D);
			kaneUtility.SetDisparity(D, dmin, dmax, PicBox, ColorBar, tbmax, tbmin);
		}
		/// <summary>
		/// 二次元配列の濃淡表示
		/// </summary>
		/// <param name="Disp">表示データ</param>
		/// <param name="dmin">表示スケール最小値</param>
		/// <param name="dmax">表示スケール最大値</param>
		/// <param name="PicBox">表示するピクチャーボックス</param>
		/// <param name="ColorBar">表示スケールのカラーバー</param>
		/// <param name="tbmax">最大値表示のテキストボックス</param>
		/// <param name="tbmin">最小値表示のテキストボックス</param>
		public static void SetDisparity(IplImage Disp, float dmin, float dmax, PictureBox PicBox, PictureBox ColorBar, TextBox tbmax, TextBox tbmin)
		{
			int width = Disp.Width;
			int height = Disp.Height;
			Bitmap D = new Bitmap(width, height);
			int cbwidth = ColorBar.Width;
			int cbheight = ColorBar.Height;
			Bitmap CB = new Bitmap(cbwidth, cbheight);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					Color col = kaneUtility.GetScaleColor(((float)Disp[y, x].Val0 - dmin) / (dmax - dmin));
					D.SetPixel(x, y, col);
				}
			}

			//PicBox.Image = D;
			CommonUtility.FillPicBox(D, PicBox);



			for (int y = 0; y < cbheight; y++)
			{
				for (int x = 0; x < cbwidth; x++)
				{
					Color col = kaneUtility.GetScaleColor((float)1.0 - (float)y / (float)cbheight);
					CB.SetPixel(x, y, col);
				}
			}
			//ColorBar.Image = CB;
			CommonUtility.FillPicBox(CB, ColorBar);

			tbmin.Text = dmin.ToString("F3");
			tbmax.Text = dmax.ToString("F3");
		}
        /// <summary>
        /// 二次元配列の濃淡表示
        /// </summary>
        /// <param name="Disp"></param>
        /// <param name="dmin"></param>
        /// <param name="dmax"></param>
        /// <param name="PicBox"></param>
        /// <param name="ColorBar"></param>
        /// <param name="tbmax"></param>
        /// <param name="tbmin"></param>
        public static void SetDisparity(float[,] Disp, float dmin, float dmax, PictureBox PicBox, PictureBox ColorBar, TextBox tbmax, TextBox tbmin)
        {
            int width = Disp.GetLength(1);
            int height = Disp.GetLength(0);
            Bitmap D = new Bitmap(width, height);
            int cbwidth = ColorBar.Width;
            int cbheight = ColorBar.Height;
            Bitmap CB = new Bitmap(cbwidth, cbheight);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color col = kaneUtility.GetScaleColor((Disp[y, x] - dmin) / (dmax - dmin));
                    D.SetPixel(x, y, col);
                }
            }
            PicBox.Image = D;
            for (int y = 0; y < cbheight; y++)
            {
                for (int x = 0; x < cbwidth; x++)
                {
                    Color col = kaneUtility.GetScaleColor((float)1.0 - (float)y / (float)cbheight);
                    CB.SetPixel(x, y, col);
                }
            }
            ColorBar.Image = CB;
            tbmin.Text = dmin.ToString("F3");
            tbmax.Text = dmax.ToString("F3");
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Disp"></param>
		/// <param name="dmin"></param>
		/// <param name="dmax"></param>
		/// <param name="PicBox"></param>
		/// <param name="ColorBar"></param>
		/// <param name="tbmax"></param>
		/// <param name="tbmin"></param>
		public static void SetDisparity(DoubleArray Disp, double dmin, double dmax, PictureBox PicBox, PictureBox ColorBar, TextBox tbmax, TextBox tbmin)
		{
			int width = Disp.size1;
			int height = Disp.size0;
			Bitmap D = new Bitmap(width, height);
			int cbwidth = ColorBar.Width;
			int cbheight = ColorBar.Height;
			Bitmap CB = new Bitmap(cbwidth, cbheight);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					Color col = kaneUtility.GetScaleColor(((float)Disp[y, x] - (float)dmin) / (float)(dmax - dmin));
					D.SetPixel(x, y, col);
				}
			}
			PicBox.Image = D;
			for (int y = 0; y < cbheight; y++)
			{
				for (int x = 0; x < cbwidth; x++)
				{
					Color col = kaneUtility.GetScaleColor((float)1.0 - (float)y / (float)cbheight);
					CB.SetPixel(x, y, col);
				}
			}
			ColorBar.Image = CB;
			tbmin.Text = dmin.ToString("F3");
			tbmax.Text = dmax.ToString("F3");
		}

		/// <summary>
		/// カラーコード変換
		/// </summary>
		/// <param name="zl">０～１の色レベル</param>
		/// <returns>色コード</returns>
		public static Color GetScaleColor(float zl)
		{   /* 2011-06-13 Copyright (C) 2011 by T.Onodera
				 */
			if (zl <= (float)0.0)
				return Color.FromArgb(0, 0, 0);
			else if (zl < 0.25f)
				return Color.FromArgb(0, (byte)(1024.0f * zl), 255);                  // blue -> light blue
			else if (zl < 0.5f)
				return Color.FromArgb(0, 255, (byte)(255.0f - 1024.0f * (zl - 0.25f))); // light blue -> green
			else if (zl < 0.75f)
				return Color.FromArgb((byte)(1024.0f * (zl - 0.5f)), 255, 0);          // green -> yellow
			else if (zl < 1.0)
				return Color.FromArgb(255, (byte)(255.0f - 1024.0f * (zl - 0.75f)), 0); // yellow -> red
			else
				return Color.FromArgb(255, 0, 0);
		}
	}
}
