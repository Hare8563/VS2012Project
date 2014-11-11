using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;//Dll読み込みのために必要

using ShoNS.Array;
using ShoNS.MathFunc;

using OpenCvSharp;
using OpenCvSharp.CPlusPlus;

namespace LabImg
{
	/// <summary>
	/// 視差を求めるクラス
	/// </summary>
	public unsafe static class Disparity
	{
		/// <summary>
		/// CDPの呼び出し
		/// </summary>
		/// <param name="inImgData">入力画像のImageDataのポインタ</param>
		/// <param name="inImgHeight">入力画像の高さ</param>
		/// <param name="inImgWidth">入力画像の幅</param>
		/// <param name="inImgWidthStep">入力画像のWidthStep</param>
		/// <param name="refImgData">参照画像のImageDataのポインタ</param>
		/// <param name="refImgHeight">参照画像の高さ</param>
		/// <param name="refImgWidth">参照画像の幅</param>
		/// <param name="refImgWidthStep">参照画像のWidthStep</param>
		/// <param name="disparityX">X方向の視差配列の先頭アドレス（出力用)</param>
		/// <param name="disparityY">Y方向の視差配列の先頭アドレス(出力用)</param>
		[DllImport("cvTwoDCDPLibrary.dll")]
		public static extern void twoDCDP(byte* inImgData, int inImgHeight, int inImgWidth, int inImgWidthStep,
										  byte* refImgData, int refImgHeight, int refImgWidth, int refImgWidthStep,
										  int* disparityX, int* disparityY);

        /// <summary>
        /// カメラパラメータファイルの読み込み
        /// </summary>
        /// <param name="fname"></param>
        ///  <param name="size"></param>
        /// <returns>CvMat　Q(4,4) F32C1</returns>
        public static  CvMat ymlRead(string fname,float size)
        {
            CvMat Q = new CvMat(4, 4, MatrixType.F32C1);
            //Qデータの更新
            using (CvFileStorage fs = new CvFileStorage(fname, null, FileStorageMode.Read))
            {
                CvFileNode node;
                node = fs.GetFileNodeByName(null, "Q");
                Q = fs.Read<CvMat>(node);
                if (size < 1.0f)
                {
                    Q[0, 3] = Q[0, 3] *size;
                    Q[1, 3] = Q[1, 3] *size;
                    Q[3, 2] = Q[3, 2] /size;
                }
            }
            return Q;

        }
        /// <summary>
        /// Disparity、画像座標（ｘ、ｙ）をQ(4,4)を用いて、カメラ座標（X、Y、Z）に変換する
        /// </summary>
        /// <param name="Q"></param>
        /// <param name="disparity"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns>CvMat　cameraXYZ(3,1)　F32C1</returns>
        public static CvMat DisparityToDistance(CvMat Q, float disparity, int X, int Y)
        {
            MatrixType mm = Q.ElemType;

            CvMat xyDisp1 = new CvMat(4, 1, Q.ElemType);
            xyDisp1[0, 0] = (float)X;
            xyDisp1[1, 0] = (float)Y;
            xyDisp1[2, 0] = (float)disparity;
            xyDisp1[3, 0] = 1.0f;
            CvMat distance = Q * xyDisp1;
            CvMat cameraXYZ = new CvMat(3, 1, MatrixType.F32C1);
            cameraXYZ[0, 0] = distance[0, 0] / distance[3, 0];
            cameraXYZ[1, 0] = distance[1, 0] / distance[3, 0];
            cameraXYZ[2, 0] = distance[2, 0] / distance[3, 0];

            return cameraXYZ;
        }
        /// <summary>
        /// Disparity、画像座標（ｘ、ｙ）をQ(4,4)を用いて、カメラ座標（X、Y、Z）に変換する
        /// </summary>
        /// <param name="focus">Q(3,4):focus</param>
        /// <param name="scale">Q(4,3):Scale</param>
        /// <param name="width">Width:Scale</param>
        /// <param name="height">Height:Scale</param>
        /// <param name="disparity"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <returns>F32C1のCvMat(3,1)：X,Y,Zのカメラ座標値</returns>
        public static CvMat DisparityToDistance(float focus, float scale, float width, float height,float disparity, int X, int Y)
        {
            float u0 = (float)width / 2.0f;
            float v0 = (float)height / 2.0f;
            CvMat cameraXYZ = new CvMat(3, 1, MatrixType.F32C1);
            if (disparity <= 0.0f) disparity = 0.0000000001f;
            float Z=focus/disparity/scale;
            float ZZ=Z/focus;
            cameraXYZ[0, 0] = ((float)X - u0) * ZZ;
            cameraXYZ[1, 0] = ((float)Y - v0) * ZZ;
            cameraXYZ[2, 0] = Z;
            return cameraXYZ;
        }
        /// <summary>
        /// カメラ座標を世界座標に変換
        /// </summary>
        /// <param name="cameraXYZ"></param>
        /// <param name="rz"></param>
        /// <param name="ry"></param>
        /// <param name="rnz"></param>
        /// <param name="T"></param>
        /// <returns></returns>
        public static CvMat CameraToWorldXYZ(CvMat cameraXYZ, float rz, float ry, float rnz, CvMat T)
        {
            CvMat R = kaneUtility.rotationMatrix(rz, ry, rnz);
            CvMat worldXYZ = R.T() * cameraXYZ + T;
            return worldXYZ;
        }
        /// <summary>
		///グレイ画像の曲率を求める
		///opt==0,平均曲率(mean curvature) 
		///opt==1,ガウス曲率（Gaussian Curvature)
		///opt==2,Kitchen-Rosenfeld operator
		///opt==3,Zuniga-Haralick operator
		///opt==4,Harris operator
		///opt==5,MORAVEC-SUZAN (threはSUZAN作用素で用いる）
		///opt==6,rerative SIFT operator 
		/// </summary>
		/// <param name="img"></param>
		/// <param name="opt"></param>
		/// <returns></returns>
		public static CvMat imageCurvature(IplImage img,int opt)
		{
			CvMat Cx = new CvMat(img.Height, img.Width, MatrixType.F32C1);
			CvMat G2 = Disparity.imageGradient2(img);
			CvMat G1 = Disparity.imageGradient(img,0);
			if (opt == 0)
			{
				for (int y = 0; y < img.Height; y++)
				{
					for (int x = 0; x < img.Width; x++)
					{
						CvScalar fx = G1.Get2D(y, x);
						CvScalar fxx = G2.Get2D(y, x);
						double h = (fxx.Val0 + fxx.Val1 + (fxx.Val0 * fx.Val1 * fx.Val1) - (2.0f * fxx.Val2 * fx.Val0 * fx.Val1) + (fxx.Val1 * fx.Val0 * fx.Val0)) / (2.0f * (1.0f + fx.Val0 * fx.Val0 + fx.Val1 * fx.Val1));
						Cx.Set2D(y, x, new CvScalar(h));
					}
				}
			}
			else if (opt == 1)
			{
				for (int y = 0; y < img.Height; y++)
				{
					for (int x = 0; x < img.Width; x++)
					{
						CvScalar fx = G1.Get2D(y, x);
						CvScalar fxx = G2.Get2D(y, x);
						double K=(fxx.Val0*fxx.Val1 - fxx.Val2*fxx.Val2 ) / (1.0+fx.Val0*fx.Val0 + fx.Val1*fx.Val1);
						Cx.Set2D(y, x, new CvScalar(K));
					}
				}
			}
			else if (opt == 2)
			{
				for (int y = 0; y < img.Height; y++)
				{
					for (int x = 0; x < img.Width; x++)
					{
						CvScalar fx = G1.Get2D(y, x);
						CvScalar fxx = G2.Get2D(y, x);
						double fx2=fx.Val0*fx.Val0;
						double fy2=fx.Val1*fx.Val1; 
						double fxfy=fx.Val0*fx.Val1; 
						double fxxyy=fx2+fy2; 
//                        double fxxyy2=fxxyy*Math.Sqrt(fxxyy);
						double fxx2=fxx.Val0*fy2 + fxx.Val1*fx2 - 2.0*fxx.Val2*fxfy;
						double K=fxx2/fxxyy;
						Cx.Set2D(y, x, new CvScalar(K));
					}
				}
				//    fx2=fx.^2;  fy2=fy.^2; fxfy=fx.*fy; fxxyy=fx2+fy2; fxxyy2=fxxyy.*sqrt(fxxyy);
				//    fxx2=fxx.*fy2 + fyy.*fx2 - 2*fxy.*fxfy;
				//    K=fxx2./fxxyy;
				//    H=fxx2./fxxyy2;
			}
			else if (opt == 3)
			{
				for (int y = 0; y < img.Height; y++)
				{
					for (int x = 0; x < img.Width; x++)
					{
						CvScalar fx = G1.Get2D(y, x);
						CvScalar fxx = G2.Get2D(y, x);
						double fx2 = fx.Val0 * fx.Val0;
						double fy2 = fx.Val1 * fx.Val1;
						double fxfy = fx.Val0 * fx.Val1;
						double fxxyy = fx2 + fy2;
						double fxxyy2 = fxxyy * Math.Sqrt(fxxyy);
						double fxx2 = fxx.Val0 * fy2 + fxx.Val1 * fx2 - 2.0 * fxx.Val2 * fxfy;
						double H = fxx2 / fxxyy2;
						Cx.Set2D(y, x, new CvScalar(H));
					}
				}
 
			}
			return Cx;
		}
		/// <summary>
		/// 一階微分画像を作成（入力は、グレースケール) 
		/// </summary>
		/// <param name="img"></param>
		/// <param name="opt">opt=0 GX,GY,opt=1　Gain,Phase</param>
		/// <returns>>X方向微分、Y方向微分、振幅、位相（度）を４ｃｈCvMatで戻す</returns>
		public static CvMat imageGradient(IplImage img, int opt)
		{
			unsafe
			{
				CvMat GXYGP = new CvMat(img.Height, img.Width, MatrixType.F32C2);
				double gx, gy, gn, ph;
				double[,] GX = new double[img.Height, img.Width];
				double[,] GY = new double[img.Height, img.Width];

				for (int y = 0; y < img.Height; y++)
				{
					for (int x = 1; x < (img.Width - 1); x++)
					{
						int xm = x - 1;
						int xp = x + 1;
						gx = (double)(img[y, xp].Val0 - img[y, xm].Val0) / 2.0;
						GX[y,x]=gx;
					}
					GX[y,0]=0.0;
					GX[y,img.Width - 1]=0.0;
				}
				for (int x = 0; x < img.Width; x++)
				{
					for (int y = 2; y < (img.Height - 1); y++)
					{
						int ym = y - 1;
						int yp = y + 1;
						gy = (double)(img[yp, x].Val0 - img[ym, x].Val0) / 2.0;
						GY[y, x]=gy;
					}
					GY[0, x]=0.0;
					GY[img.Height - 1, x]=0.0;
				}
				for (int y = 0; y < img.Height; y++)
				{
					for (int x = 0; x < img.Width; x++)
					{
						//                        int offset = (FG.Step * y)/8 +  x*2;
						gx = GX[y, x];
						gy = GY[y, x];
						if (opt == 0)
						{
							GXYGP.Set2D(y, x, new CvScalar(gx, gy));
						}
						else
						{
							gn = Math.Sqrt(gx * gx + gy * gy);
							ph = Math.Atan2(gy, gx) * 180.0 / 3.14159;
							GXYGP.Set2D(y, x, new CvScalar(gn, ph));
						}
					}
				}
				return GXYGP;
			}
		}
		/// <summary>
		/// 一階微分画像を作成（入力は、グレースケール)
		/// </summary>
		/// <param name="img"></param>
		/// <param name="GX">X方向微分</param>
		/// <param name="GY">Y方向微分</param>
		/// <param name="GN">振幅</param>
		/// <param name="PH">位相（摂</param>

		public static void imageGradient(IplImage img, out CvMat GX, out CvMat GY, out CvMat GN, out CvMat PH)
		{
			//CvMatの読み込み
			//		CvScalar scalar = GX.Get2D(y, x);
			//		Console.WriteLine(scalar.Val0 + " " + scalar.Val1 + " " + scalar.Val2);
			//書き込み
			//     CvScalar scalar;
			//     Scalar.
			//		GX.Set2D(y, x, scalar);
			unsafe
			{
				GX = new CvMat(img.Height, img.Width, MatrixType.F32C1);
				GY = new CvMat(img.Height, img.Width, MatrixType.F32C1);
				GN = new CvMat(img.Height, img.Width, MatrixType.F32C1);
				PH = new CvMat(img.Height, img.Width, MatrixType.F32C1);
				double gx, gy, gn, ph;
				//                float* ptrFG = (float*)FG.Data;
				for (int y = 0; y < img.Height; y++)
				{
					for (int x = 1; x < (img.Width - 1); x++)
					{
						int xm = x - 1;
						int xp = x + 1;
						gx = (double)(img[y, xp].Val0 - img[y, xm].Val0) / 2.0;
						GX.Set2D(y, x, new CvScalar(gx));
					}
					GX.Set2D(y, 0, new CvScalar(0.0));
					GX.Set2D(y, img.Width - 1, new CvScalar(0.0));
				}
				for (int x = 0; x < img.Width; x++)
				{
					for (int y = 2; y < (img.Height - 1); y++)
					{
						int ym = y - 1;
						int yp = y + 1;
						gy = (double)(img[yp, x].Val0 - img[ym, x].Val0) / 2.0;
						GY.Set2D(y, x, new CvScalar(gy));
					}
					GY.Set2D(0, x, new CvScalar(0.0));
					GY.Set2D(img.Height - 1, x, new CvScalar(0.0));
				}
				for (int y = 0; y < img.Height; y++)
				{
					for (int x = 0; x < img.Width; x++)
					{
						//                        int offset = (FG.Step * y)/8 +  x*2;
						gx = GX.Get2D(y, x).Val0;
						gy = GY.Get2D(y, x).Val0;
						gn = Math.Sqrt(gx * gx + gy * gy);
						ph = Math.Atan2(gy, gx) * 180.0 / 3.14159;
						GN.Set2D(y, x, new CvScalar(gn));
						PH.Set2D(y, x, new CvScalar(ph));
					}
				}
				return;
			}
		}
		/// <summary>
		/// 二階微分の計算（入力は、グレースケール) 
		/// </summary>
		/// <param name="img"></param>
		/// <returns>>gxx,gyy,gxyを３ｃｈのCvMatで戻す</returns>
		public static CvMat imageGradient2(IplImage img)
		{
			unsafe
			{
				CvMat GLP = new CvMat(img.Height, img.Width, MatrixType.F32C3);
				//                float* ptrFG = (float*)FG.Data;
				double[,] gxx = new double[img.Height, img.Width];
				double[,] gyy = new double[img.Height, img.Width];
				double[,] gxy = new double[img.Height, img.Width];
				for (int y = 0; y < img.Height; y++)
				{
					for (int x = 1; x < (img.Width - 1); x++)
					{
						int xm = x - 1;
						int xp = x + 1;
						gxx[y, x] = (double)(img[y, xp].Val0 + img[y, xm].Val0 - 2.0 * img[y, x].Val0);
					}
					gxx[y, 0] = (double)(img[y, 1].Val0 - img[y, 0].Val0);
					gxx[y, img.Width - 1] = (double)(img[y, img.Width - 2].Val0 - img[y, img.Width - 1].Val0);
				}
				for (int x = 0; x < img.Width; x++)
				{
					for (int y = 1; y < (img.Height - 1); y++)
					{
						int ym = y - 1;
						int yp = y + 1;
						gyy[y,x] = (double)(img[yp, x].Val0 + img[ym, x].Val0 - 2.0 * img[y, x].Val0);
					}
					gyy[0,x] = (double)(img[1, x].Val0 - img[0, x].Val0);
					gyy[img.Height-1,x] = (double)(img[img.Height - 2, x].Val0 - img[img.Height - 1, x].Val0);
				}
				for (int y = 1; y < img.Height - 1; y++)
				{
					for (int x = 1; x < img.Width - 1; x++)
					{
						int xm = x - 1;
						int xp = x + 1;
						int ym = y - 1;
						int yp = y + 1;
						gxy[y,x]= (img[yp,xp].Val0 - img[y,xp].Val0 - img[yp,x].Val0 + img[y,x].Val0
								  +img[y, x].Val0 - img[ym, x].Val0 - img[y, xm].Val0 + img[ym, xm].Val0)/2.0f;
					}
				}
				for (int y = 0; y < img.Height; y++)
				{
					for (int x = 0; x < img.Width; x++)
					{
						GLP.Set2D(y, x, new CvScalar(gxx[y,x],gyy[y,x],gxy[y,x]));
					}
				}
				return GLP;
			}
		}
		/// <summary>
		/// 二階微分画像のラプラシアンを作成（入力は、グレースケール) 
		/// </summary>
		/// <param name="img"></param>
		/// <returns>>ラプラシアンを一ｃｈCvMatで戻す</returns>
		public static CvMat imageLanlacian(IplImage img)
		{
			unsafe
			{
				CvMat GLP = new CvMat(img.Height, img.Width, MatrixType.F32C1);
				//                float* ptrFG = (float*)FG.Data;
				double[,] gxx = new double[img.Height, img.Width];
				for (int y = 0; y < img.Height; y++) 
				{
					for (int x = 1; x < (img.Width - 1); x++)
					{
						int xm = x - 1;
						int xp = x + 1;
						gxx[y,x] = (double)(img[y, xp].Val0 + img[y, xm].Val0 - 2.0 * img[y, x].Val0); 
					}
					gxx[y, 0] = (double)(img[y, 1].Val0 - img[y, 0].Val0);
					gxx[y, img.Width - 1] = (double)(img[y, img.Width - 2].Val0 - img[y, img.Width - 1].Val0);
				}
				for (int x = 0; x < img.Width; x++)
				{
					double gyy,gn;
					for (int y = 1; y < (img.Height - 1); y++)
					{
						int ym = y - 1;
						int yp = y + 1;
						gyy = (double)(img[yp, x].Val0 + img[ym, x].Val0 - 2.0 * img[y, x].Val0);
						gn = gxx[y,x] * gxx[y,x] + gyy * gyy;
						GLP.Set2D(y, x, new CvScalar(gn));
					}
					gyy = (double)(img[1,x].Val0 - img[0,x].Val0);
					gn = gxx[0, x] * gxx[0, x] + gyy * gyy;
					GLP.Set2D(0, x, new CvScalar(gn));
					gyy = (double)(img[img.Height - 2,x].Val0 - img[img.Height - 1,x].Val0);
					gn = gxx[img.Height-1, x] * gxx[img.Height-1, x] + gyy * gyy;
					GLP.Set2D(img.Height-1, x, new CvScalar(gn));
				}
				return GLP;
			}
		}
		/// <summary>
		/// ４CｈのCvMatから、exNoで指定したデータを取り出し、１ｃｈのCvMatで戻す
		/// </summary>
		/// <param name="G"></param>
		/// <param name="exNo">０、１，２，３で指定</param>
		/// <returns></returns>
		public static CvMat CvMatExtraction(CvMat G, int exNo)
		{
			CvMat GEx = new CvMat(G.Height, G.Width, MatrixType.F32C1);
			for (int y = 0; y < G.Height; y++)
			{
				for (int x = 0; x < G.Width; x++)
				{
					CvScalar W = G.Get2D(y, x);
					if( exNo==3)  GEx.Set2D(y, x, W.Val3);
					else if (exNo == 2) GEx.Set2D(y, x, W.Val2);
					else if (exNo == 1) GEx.Set2D(y, x, W.Val1);
					else  GEx.Set2D(y, x, W.Val0);
				}
			}
			return GEx;
		}
        /// <summary>
        /// 画像のブロックごとの分散または振幅を求める
        /// </summary>
        /// <param name="rightI"></param>
        /// <param name="halfBlockSize"></param>
        /// <param name="VorPP">’P’で振幅、それ以外は標準偏差</param>
        /// <returns>FloatArrayを返す</returns>
        public static CvMat BlockMatchTexture(IplImage rightI, int halfBlockSize, char VorPP)
        {
            int NX = rightI.Width;
            int NY = rightI.Height;
            int blockSize = 2 * halfBlockSize + 1;
            int nc = rightI.NChannels;
            CvMat var = new CvMat(NY, NX, MatrixType.F32C1);
            unsafe
            {
                float[] tmR = new float[blockSize * blockSize*nc];
                byte* Ptr = (byte*)rightI.ImageData;
                int rwd = rightI.WidthStep;

                //FloatArray RG = kaneUtility.IplImageToFloatArray(rightI);

                for (int m = 0; m < NY; m++)
                {
                    int minr = Math.Max(1, m + 1 - halfBlockSize);
                    int maxr = Math.Min(NY, m + 1 + halfBlockSize);

                    for (int n = 0; n < NX; n++)
                    {
                        int minc = Math.Max(1, n + 1 - halfBlockSize);
                        int maxc = Math.Min(NX, n + 1 + halfBlockSize);
                        int no = 0;
                        float vmax = float.MinValue;
                        float vmin = float.MaxValue;
                        float avr = 0.0f;
                        float rms = 0.0f;
                        float tmp;
                        for (int y = minr - 1; y <= maxr - 1; y++)
                        {
                            for (int x = minc - 1; x <= maxc - 1; x++)
                            {
                                for (int k = 0; k < nc; k++)
                                {
                                    tmp = (float)Ptr[y * rwd + nc*x + k];
                                    tmR[no] = tmp;
                                    avr += tmp;
                                    if (vmax < tmp) vmax = tmp;
                                    if (vmin > tmp) vmin = tmp;
                                    no++;
                                }
                            }
                        }
                        avr /= (float)no;

                        float xx;
                        if (VorPP == 'P' || VorPP == 'p')
                        {
                            xx = vmax - vmin;
                        }
                        else
                        {
                            for (int i = 0; i < no; i++)
                            {
                                rms += (tmR[i] - avr) * (tmR[i] - avr);
                            }
                            rms /= (float)no;
                            rms = (float)Math.Sqrt(rms);
                            xx = rms;
                        }
                        var[m, n] = xx;
                    }
                }
                return var;
            }
        }
		/// <summary>
		///右画像の(TopLeftX,TopLeftY）座標を起点に、水平方向にdisparityRange個だけブロックマッチを行う。DisparityCostの分布を見る際に用いる
		///入力画像をGrayでもRGBでも良いように改造。ただし、IplImageはU8(バイト）型でないといけない。
		/// </summary>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		/// <param name="halfBlockSize"></param>
		/// <param name="disparityRange"></param>
		/// <param name="TopLeftX"></param>
		/// <param name="TopLeftY"></param>
		/// <returns>disparityCost[]の一次元配列を戻り値として返す</returns>
		public static float[] HorizontalBlockMatch(IplImage Left, IplImage Right, int halfBlockSize, int disparityRange, int TopLeftX,int TopLeftY)
		{
			int NX = Left.Width;
			int NY = Left.Height;
			int nc = Right.NChannels;
			//IplImage Ddynamic = new IplImage(NX, NY, BitDepth.F32, 1);
			float[] disparityCost=new float[disparityRange+1];
			unsafe
			{
				int disparityRange1 = disparityRange + 1;//０－disparityrange＋１までを探索
				int blockSize = 2 * halfBlockSize + 1;
				int nRowsLeft = NY;

				float finf = float.MaxValue;
				IplImage tmR = new IplImage(blockSize, blockSize, BitDepth.U8, nc);
				IplImage tmL = new IplImage(blockSize, blockSize, BitDepth.U8, nc);
				byte* rightPtr = (byte*)Right.ImageData;
				byte* tmRPtr = (byte*)tmR.ImageData;
				byte* tmLPtr = (byte*)tmL.ImageData;
				byte* leftPtr = (byte*)Left.ImageData;
				int iwd = tmL.WidthStep;
				int rwd = Right.WidthStep;

				int m = TopLeftY;
				//for (int m = 0; m < nRowsLeft; m++)
				//{

					for (int i = 0; i < disparityRange1; i++) disparityCost[i] = finf;
					int minr = Math.Max(1, m + 1 - halfBlockSize);
					int maxr = Math.Min(nRowsLeft, m + 1 + halfBlockSize);

					int n=TopLeftX;
					//for (int n = 0; n < NX; n++)
					//{
						int minc = Math.Max(1, n + 1 - halfBlockSize);
						int maxc = Math.Min(NX, n + 1 + halfBlockSize);
						int mind = Math.Max(0, 1 - minc);
						int maxd = Math.Min(disparityRange, NX - maxc);

						//                    tmL = leftI.GetSlice(minr - 1, maxr - 1, minc + d - 1, maxc + d - 1);
						//                    tmR = rightI.GetSlice(minr - 1, maxr - 1, minc - 1, maxc - 1);

						for (int y = 0; y < blockSize * blockSize*nc; y++) tmRPtr[y] = 0;

						for (int y = minr - 1; y <= (maxr - 1); y++)
						{
							for (int x = minc - 1; x <= (maxc - 1); x++)
							{
								for (int k = 0; k < nc; k++)
								{
									tmRPtr[(y - minr + 1) * iwd + nc*(x - minc + 1)+k] = rightPtr[y * rwd + nc*x + k];
								}
							}
						}


						for (int d = mind; d <= maxd; d++)
						{
							for (int y = 0; y < blockSize * blockSize*nc; y++) tmLPtr[y] = 0;

							for (int y = minr - 1; y <= (maxr - 1); y++)
							{
								for (int x = minc + d - 1; x <= (maxc + d - 1); x++)
								{
									for (int k = 0; k < nc;k++ )
										tmLPtr[(y - minr + 1) * iwd + nc*(x - minc - d + 1)+k] = leftPtr[y * rwd + nc*x + k];
								}
							}

							int sum = 0;
							for (int y = 0; y < (blockSize * blockSize*nc); y++)
							{
								sum += (int)Math.Abs(tmLPtr[y] - tmRPtr[y]);
							}
							disparityCost[d] = sum;
						}

						float dismin = finf;
						float ix1 = 0;
						for (int i = 0; i < disparityRange1; i++)
						{
							if (disparityCost[i] < dismin)
							{
								ix1 = i;
								dismin = disparityCost[i];
							}
						}

	//                    dynamicPtr[m * dwd + n] = ix1;

						//Ddynamic[m, n] = ix1;
					//}
				//}
				tmL.Dispose();
				tmR.Dispose();
			}

			return disparityCost;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="leftPath"></param>
		/// <param name="rightPath"></param>
		/// <param name="halfBlockSize"></param>
		/// <param name="disparityRange"></param>
		/// <returns></returns>
		public static IplImage KanemotoBlockMatch(string leftPath, string rightPath, int halfBlockSize, int disparityRange)
		{
			using (IplImage grayLeft = new IplImage(leftPath, LoadMode.GrayScale))
			using (IplImage grayRight = new IplImage(rightPath, LoadMode.GrayScale))
			{
				return KanemotoBlockMatch(grayLeft, grayRight,  halfBlockSize, disparityRange);
			}
		}

	



		/// <summary>
		/// MATLABのBlockマッチング,入力画像は、GrayかRGBで、U8(バイト）型
		/// </summary>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		/// <param name="halfBlockSize"></param>
		/// <param name="disparityRange"></param>
		/// <returns></returns>
		public static IplImage KanemotoBlockMatch(IplImage Left, IplImage Right, int halfBlockSize, int disparityRange)
		{
			int NX = Left.Width;
			int NY = Left.Height;
			int nc=Left.NChannels;
			IplImage Ddynamic = new IplImage(NX, NY, BitDepth.F32, 1);
			unsafe
			{
				int disparityRange1 = disparityRange + 1;//０－disparityrange＋１までを探索
				int blockSize = 2 * halfBlockSize + 1;
				int nRowsLeft = NY;

				float finf = float.MaxValue;
				float[] disparityCost = new float[disparityRange1];
				IplImage tmR = new IplImage(blockSize, blockSize, BitDepth.U8, nc);
				IplImage tmL = new IplImage(blockSize, blockSize, BitDepth.U8, nc);
				byte* rightPtr = (byte*)Right.ImageData;
				byte* tmRPtr = (byte*)tmR.ImageData;
				byte* tmLPtr = (byte*)tmL.ImageData;
				byte* leftPtr = (byte*)Left.ImageData;
				float* dynamicPtr = (float*)Ddynamic.ImageData;
				int iwd = tmL.WidthStep;
				int rwd = Right.WidthStep;
				int dwd = Ddynamic.WidthStep / 4;


				for (int m = 0; m < nRowsLeft; m++)
				{

					for (int i = 0; i < disparityRange1; i++) disparityCost[i] = finf;
					int minr = Math.Max(1, m + 1 - halfBlockSize);
					int maxr = Math.Min(nRowsLeft, m + 1 + halfBlockSize);

					for (int n = 0; n < NX; n++)
					{
						int minc = Math.Max(1, n + 1 - halfBlockSize);
						int maxc = Math.Min(NX, n + 1 + halfBlockSize);
						int mind = Math.Max(0, 1 - minc);
						int maxd = Math.Min(disparityRange, NX - maxc);

						//                    tmL = leftI.GetSlice(minr - 1, maxr - 1, minc + d - 1, maxc + d - 1);
						//                    tmR = rightI.GetSlice(minr - 1, maxr - 1, minc - 1, maxc - 1);

						for (int y = 0; y < blockSize * blockSize*nc; y++) tmRPtr[y] = 0;

						for (int y = minr - 1; y <= (maxr - 1); y++)
						{
							for (int x = minc - 1; x <= (maxc - 1); x++)
							{
								for (int k = 0; k < nc;k++ )
									tmRPtr[(y - minr + 1) * iwd + nc*(x - minc + 1)+k] = rightPtr[y * rwd + nc*x + k];
							}
						}


						for (int d = mind; d <= maxd; d++)
						{
							for (int y = 0; y < blockSize * blockSize*nc; y++) tmLPtr[y] = 0;

							for (int y = minr - 1; y <= (maxr - 1); y++)
							{
								for (int x = minc + d - 1; x <= (maxc + d - 1); x++)
								{
									for (int k = 0; k < nc;k++ )
										tmLPtr[(y - minr + 1) * iwd + nc*(x - minc - d + 1)+k] = leftPtr[y * rwd + nc*x + k];
								}
							}

							int sum = 0;
							for (int y = 0; y < (blockSize * blockSize*nc); y++)
							{
								sum += (int)Math.Abs(tmLPtr[y] - tmRPtr[y]);
							}
							disparityCost[d] = sum;
						}

						float dismin = finf;
						float ix1 = 0;
						for (int i = 0; i < disparityRange1; i++)
						{
							if (disparityCost[i] < dismin)
							{
								ix1 = i;
								dismin = disparityCost[i];
							}
						}

						dynamicPtr[m * dwd  + n] = ix1;
						
						//Ddynamic[m, n] = ix1;
					}
				}
				tmL.Dispose();
				tmR.Dispose();
			}

			return Ddynamic;
		}
		/// <summary>
		/// 制約付きBlockMatch(Stencil）
		/// </summary>
		/// <param name="Left"></param>
		/// <param name="Right"></param>
		/// <param name="halfBlockSize"></param>
		/// <param name="stencil"></param>
		/// <param name="srcRange"></param>
		/// <returns></returns>
		public static IplImage KanemotoBlockMatch(IplImage Left, IplImage Right, int halfBlockSize, CvMat stencil, int srcRange)
		{
			int NX = Left.Width;
			int NY = Left.Height;
			int nc = Left.NChannels;
			IplImage Ddynamic = new IplImage(NX, NY, BitDepth.F32, 1);
			int[,] disparityRangeL = new int[NY, NX];
			int[,] disparityRangeH = new int[NY, NX];
			for (int y = 0; y < stencil.Height; y++)
			{
				for (int x = 0; x < stencil.Width; x++)
				{
					disparityRangeL[y, x] = (int)stencil[y, x] - srcRange;
					disparityRangeH[y, x] = (int)stencil[y, x] + srcRange;
					if (disparityRangeL[y, x] < 0) disparityRangeL[y, x] = 0;
					//if(disparityRangeH[y,x]>maxSearchRange) disparityRangeH[y,x]=maxSearchRange;
				}
			}

			unsafe
			{
				//                int disparityRange1 = disparityRange + 1;//０－disparityrange＋１までを探索
				int blockSize = 2 * halfBlockSize + 1;
				int nRowsLeft = NY;

				float finf = float.MaxValue;
				//                float[] disparityCost = new float[disparityRange1];
				IplImage tmR = new IplImage(blockSize, blockSize, BitDepth.U8, 1);
				IplImage tmL = new IplImage(blockSize, blockSize, BitDepth.U8, 1);
				byte* rightPtr = (byte*)Right.ImageData;
				byte* tmRPtr = (byte*)tmR.ImageData;
				byte* tmLPtr = (byte*)tmL.ImageData;
				byte* leftPtr = (byte*)Left.ImageData;
				float* dynamicPtr = (float*)Ddynamic.ImageData;
				int iwd = tmL.WidthStep;
				int rwd = Right.WidthStep;
				int dwd = Ddynamic.WidthStep / 4;


				for (int m = 0; m < nRowsLeft; m++)
				{
					int minr = Math.Max(1, m + 1 - halfBlockSize);
					int maxr = Math.Min(nRowsLeft, m + 1 + halfBlockSize);

					for (int n = 0; n < NX; n++)
					{
						int minc = Math.Max(1, n + 1 - halfBlockSize);
						int maxc = Math.Min(NX, n + 1 + halfBlockSize);
						int mind = Math.Max(disparityRangeL[m, n], 1 - minc);
						int maxd = Math.Min(disparityRangeH[m, n], NX - maxc);

						//                    tmL = leftI.GetSlice(minr - 1, maxr - 1, minc + d - 1, maxc + d - 1);
						//                    tmR = rightI.GetSlice(minr - 1, maxr - 1, minc - 1, maxc - 1);
						if (mind < maxd)
						{

							for (int y = 0; y < blockSize * blockSize*nc; y++) tmRPtr[y] = 0;

							for (int y = minr - 1; y <= (maxr - 1); y++)
							{
								for (int x = minc - 1; x <= (maxc - 1); x++)
								{
									for (int k = 0; k < nc;k++ )
										tmRPtr[(y - minr + 1) * iwd + nc*(x - minc + 1)+k] = rightPtr[y * rwd + nc*x+k];
								}
							}
							int md = maxd - mind + 1;
							float[] cp = new float[md];
							for (int i = 0; i < md; i++) cp[i] = finf;

							for (int d = mind; d <= maxd; d++)
							{
								for (int y = 0; y < blockSize * blockSize*nc; y++) tmLPtr[y] = 0;

								for (int y = minr - 1; y <= (maxr - 1); y++)
								{
									for (int x = minc + d - 1; x <= (maxc + d - 1); x++)
									{
										for (int k = 0; k < nc;k++ )
											tmLPtr[(y - minr + 1) * iwd + nc*(x - minc - d + 1)+k] = leftPtr[y * rwd + nc*x+k];
									}
								}

								int sum = 0;
								for (int y = 0; y < (blockSize * blockSize*nc); y++)
								{
									sum += (int)Math.Abs(tmLPtr[y] - tmRPtr[y]);
								}
								cp[d - mind] = sum;
							}

							float dismin = finf;
							float ix1 = 0;
							for (int i = 0; i < md; i++)
							{
								if (cp[i] < dismin)
								{
									ix1 = i;
									dismin = cp[i];
								}
							}

							dynamicPtr[m * dwd + n] = ix1 + mind;
						}
						else
						{
							dynamicPtr[m * dwd + n] = mind;
						}

						//Ddynamic[m, n] = ix1;
					}
				}
				tmL.Dispose();
				tmR.Dispose();
			}

			return Ddynamic;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="leftPath"></param>
		/// <param name="rightPath"></param>
		/// <param name="halfBlockSize"></param>
		/// <param name="disparityRange"></param>
		/// <returns></returns>
		public static IplImage KanemotoDPmatch(string leftPath, string rightPath, int halfBlockSize, int disparityRange)
		{
			using (IplImage grayLeft = new IplImage(leftPath, LoadMode.GrayScale))
			using (IplImage grayRight = new IplImage(rightPath, LoadMode.GrayScale))
			{
				return KanemotoBlockMatch(grayLeft, grayRight, halfBlockSize, disparityRange);
			}
		}


		/// <summary>
		/// MATLABのDPマッチング,入力画像はGrayでもRGBでもよい。
		/// </summary>
		/// <param name="leftI"></param>
		/// <param name="rightI"></param>
		/// <param name="halfBlockSize"></param>
		/// <param name="disparityRange"></param>
		/// <returns></returns>
		public static IplImage KanemotoDPmatch(IplImage leftI, IplImage rightI, int halfBlockSize, int disparityRange)
		{
			unsafe
			{
				int NX = leftI.Width;
				int NY = leftI.Height;
				int nc = leftI.NChannels;
				int disparityRange1 = disparityRange + 1;
				int blockSize = 2 * halfBlockSize + 1;
				int nRowsLeft = NY;
				byte* LeftPtr = (byte*)leftI.ImageData;
				byte* RightPtr = (byte*)rightI.ImageData;
				//FloatArray Ddynamic = new FloatArray(NY, NX);
				IplImage Ddynamic = new IplImage(NX, NY, BitDepth.F32, 1);
				

				IplImage tmR = new IplImage(blockSize, blockSize, BitDepth.U8, nc);
				IplImage tmL = new IplImage(blockSize, blockSize, BitDepth.U8, nc);
				byte* tmRPtr = (byte*)tmR.ImageData;
				byte* tmLPtr = (byte*)tmL.ImageData;
				float* dynamicPtr = (float*)Ddynamic.ImageData;
				int iwd = tmL.WidthStep;
				int rwd = rightI.WidthStep;
				int dwd = Ddynamic.WidthStep / 4;

				float finf = 1000.0f;
				float[,] disparityCost = new float[NX, disparityRange1]; ;
				float disparityPenalty = 0.5f;

				for (int m = 0; m < nRowsLeft; m++)
				{
					for (int j = 1; j < NX; j++)
						for (int i = 0; i < disparityRange1; i++) disparityCost[j, i] = finf;
					int minr = Math.Max(1, m + 1 - halfBlockSize);
					int maxr = Math.Min(nRowsLeft, m + 1 + halfBlockSize);

					for (int n = 0; n < NX; n++)
					{
						int minc = Math.Max(1, n + 1 - halfBlockSize);
						int maxc = Math.Min(NX, n + 1 + halfBlockSize);
						int mind = Math.Max(0, 1 - minc);
						int maxd = Math.Min(disparityRange, NX - maxc);
						for (int y = 0; y < blockSize * blockSize*nc; y++) tmRPtr[y] = 0;
						for (int y = minr - 1; y <= (maxr - 1); y++)
						{
							for (int x = minc - 1; x <= (maxc - 1); x++)
							{
								for (int k = 0; k < nc;k++ )
									tmRPtr[(y - minr + 1) * iwd + nc*(x - minc + 1)+k] = RightPtr[y * rwd + nc*x+k];
							}
						}
						for (int d = mind; d <= maxd; d++)
						{
							//  tmL = leftI.GetSlice(minr - 1, maxr - 1, minc + d - 1, maxc + d - 1);
							//  tmR = rightI.GetSlice(minr - 1, maxr - 1, minc - 1, maxc - 1);
							for (int y = 0; y < blockSize * blockSize*nc; y++) tmLPtr[y] = 0;
							for (int y = minr - 1; y <= (maxr - 1); y++)
							{
								for (int x = minc + d - 1; x <= (maxc + d - 1); x++)
								{
									for (int k = 0; k < nc;k++ )
										tmLPtr[(y - minr + 1) * iwd + nc*(x - minc - d + 1)+k] = LeftPtr[y * rwd + nc*x+k];
								}
							}

							int sum = 0;
							for (int y = 0; y < (blockSize * blockSize*nc); y++)
							{
								sum += (int)Math.Abs(tmLPtr[y] - tmRPtr[y]);
							}

							disparityCost[n, d] = (float)sum / 255.0f;

						}
					}

					float[,] optimalIndices = new float[NX, disparityRange1];
					float[] cp = new float[disparityRange1];
					int[] minI = new int[disparityRange1 - 2];
					float[] minV = new float[disparityRange1 - 2];

					for (int i = 0; i < disparityRange1; i++) cp[i] = disparityCost[NX - 1, i];
					//FloatArray cp = disparityCost.GetSlice(NX - 1, NX - 1, 0, disparityCost.size1 - 1);


					for (int j = NX - 2; j > -1; j--)
					{
						float cfinf = (NX - j) * finf;
						float[,] cpc = new float[7, disparityRange1 - 2];
						//cpc.FillValue(cfinf);

						cpc[0, 0] = cpc[0, 1] = cfinf;
						for (int i = 2; i < disparityRange1 - 2; i++) cpc[0, i] = cp[i - 2] + (3 * disparityPenalty);
						cpc[1, 0] = cfinf;
						for (int i = 1; i < disparityRange1 - 2; i++) cpc[1, i] = cp[i - 1] + (2 * disparityPenalty);
						for (int i = 0; i < disparityRange1 - 2; i++) cpc[2, i] = cp[i] + disparityPenalty;
						for (int i = 0; i < disparityRange1 - 2; i++) cpc[3, i] = cp[i + 1];
						for (int i = 0; i < disparityRange1 - 2; i++) cpc[4, i] = cp[i + 2] + disparityPenalty;
						cpc[5, disparityRange1 - 3] = cfinf;
						for (int i = 0; i < disparityRange1 - 3; i++) cpc[5, i] = cp[i + 3] + (2 * disparityPenalty);
						cpc[6, disparityRange1 - 3] = cpc[6, disparityRange1 - 4] = cfinf;
						for (int i = 0; i < disparityRange1 - 4; i++) cpc[6, i] = cp[i + 4] + (3 * disparityPenalty);

						//最小の値とIndexを求め、リストに入れる
						for (int k = 0; k < disparityRange1 - 2; k++)
						{
							int minIndex = 0;
							float minValue = float.MaxValue;
							for (int u = 0; u < 7; u++)
							{
								if (cpc[u, k] < minValue)
								{
									minValue = cpc[u, k];
									minIndex = u;
								}
							}
							minI[k] = minIndex;
							minV[k] = minValue;
						}

						cp[0] = cfinf;
						cp[disparityRange1 - 1] = cfinf;
						for (int k = 0; k < disparityRange1 - 2; k++)
						{
							cp[k + 1] = disparityCost[j, k + 1] + minV[k];
						}
						for (int kk = 1; kk < disparityRange1 - 1; kk++)
						{
							optimalIndices[j, kk] = kk + (minI[kk - 1] - 4) + 2;
						}
					}

					float cpmin = float.MaxValue;
					int ix1 = 0;
					for (int i = 0; i < disparityRange1; i++)
					{
						if (cp[i] < cpmin)
						{
							ix1 = i;
							cpmin = cp[i];
						}
					}

					
					//Ddynamic[m, 0] = ix1;
					dynamicPtr[m * dwd + 0] = ix1;

					//for (int k = 0; k < Ddynamic.size1 - 1; k++)
					for (int k = 0; k < Ddynamic.Width - 1; k++)
					{
						//int kkk = Math.Max(0, Math.Min(disparityRange, (int)Math.Round(Ddynamic[m, k])));
						int kkk = Math.Max(0, Math.Min(disparityRange, (int)Math.Round(dynamicPtr[m * dwd + k])));
						
						//Ddynamic[m, k + 1] = optimalIndices[k, kkk] - 1;
						dynamicPtr[m * dwd + k + 1] = optimalIndices[k, kkk] - 1; ;
					}
				}
				return Ddynamic;
			}
		}


		/// <summary>
		/// OpenCV版ブロックマッチ、、戻り値はU8,1ch
		/// 視差は16倍されている（距離変換後16倍する必要がある)
		/// </summary>
		/// <param name="leftPath"></param>
		/// <param name="rightPath"></param>
		/// <param name="bmsize"></param>
		/// <param name="srcRange"></param>
		/// <returns></returns>
		public static IplImage BlockMatch(string leftPath, string rightPath, int bmsize, int srcRange)
		{
			using(IplImage grayLeft = new IplImage(leftPath, LoadMode.GrayScale))
			using (IplImage grayRight = new IplImage(rightPath, LoadMode.GrayScale))
			{
				return BlockMatch(grayLeft, grayRight, bmsize, srcRange);
			}
		}

		/// <summary>
		/// OpenCV版ブロックマッチ、戻り値はU8,1ch
		/// 視差は16倍されている（距離変換後16倍する必要がある)
		/// </summary>
		/// <param name="grayLeft"></param>
		/// <param name="grayRight"></param>
		/// <param name="bmsize">ブロックマッチングサイズ（2*bmsiz+1が実際のサイズ）</param>
		/// <param name="srcRange">探索範囲の最大値（零から最大値までを探索）</param>
		/// <returns></returns>
		/// 
		///
		public static IplImage BlockMatch(IplImage grayLeft, IplImage grayRight, int bmsize, int srcRange)
		{
			CvStereoBMState stateBM = new CvStereoBMState(StereoBMPreset.Basic, srcRange);
			stateBM.SADWindowSize = bmsize * 2 + 1;
			IplImage dispBM = new IplImage(grayLeft.Size, BitDepth.S16, 1);
			Cv.FindStereoCorrespondenceBM(grayLeft, grayRight, dispBM, stateBM);


			//dst = kaneUtility.IplImageToFloatArray(dispBM); //下記とほぼ同じだが、零の扱いが異なるので、要確認
			IplImage dstBM = new IplImage(grayLeft.Size, BitDepth.U8, 1);
			Cv.ConvertScale(dispBM, dstBM);
			IplImage dst = new IplImage(grayLeft.Size, BitDepth.U8, 1);
			dstBM.Copy(dst);
			dstBM.Dispose();
			dispBM.Dispose();
			return dst;
		}


		
		/// <summary>
		/// OpenCVのDPマッチング関数、戻り値はU8,1ch
		/// 視差は16倍されている（距離変換後16倍する必要がある)
		/// </summary>
		/// <param name="leftFilePath"></param>
		/// <param name="rightFlePath"></param>
		/// <param name="bmsize"></param>
		/// <param name="srcRange"></param>
		/// <returns></returns>
		public static IplImage DPMatch(string leftFilePath, string rightFlePath, int bmsize, int srcRange)
		{
			using (Mat leftMat = new Mat(leftFilePath, LoadMode.GrayScale))
			using (Mat rightMat = new Mat(rightFlePath, LoadMode.GrayScale))
			using (Mat disp = new Mat())
			{
				StereoSGBM sgbm = new StereoSGBM(0, 16, 5, 1, 1, 32, 3, 10, 3, 16, true);
				sgbm.SADWindowSize = 2 * bmsize + 1;
				sgbm.NumberOfDisparities = srcRange;
                
				//sgbm.FindCorrespondence(leftMat, rightMat, disp);
                sgbm.Compute(leftMat, rightMat, disp);//2.4.8ではComputeに変更されたみたい
				IplImage dst = new IplImage(leftMat.Cols, leftMat.Rows, BitDepth.U8, 1);
				Cv.Convert(disp.ToCvMat(), dst);
				return dst;
			}
		}



		/// <summary>
		/// OpenCV GraphCut、戻り値はU8,1ch
		///  視差は16倍されている（距離変換後16倍する必要がある)
		/// </summary>
		/// <param name="leftPath"></param>
		/// <param name="rightPath"></param>
		/// <param name="bmsize"></param>
		/// <param name="searchRange"></param>
		/// <returns></returns>
		public static IplImage GraphCut(string leftPath, string rightPath, int bmsize, int searchRange)
		{
			using (IplImage grayLeft = new IplImage(leftPath, LoadMode.GrayScale))
			using (IplImage grayRight = new IplImage(rightPath, LoadMode.GrayScale))
			{
				return GraphCut(grayLeft, grayRight, bmsize, searchRange);
			}
		}

		/// <summary>
		/// OpenCV GraphCut、戻り値はU8,1ch
		///  視差は16倍されている（距離変換後16倍する必要がある)
		/// </summary>
		/// <param name="grayLeft"></param>
		/// <param name="grayRight"></param>
		/// <param name="bmsize"></param>
		/// <param name="searchRange"></param>
		/// <returns></returns>
		public static IplImage GraphCut(IplImage grayLeft, IplImage grayRight, int bmsize, int searchRange)
		{
			IplImage result = new IplImage(grayLeft.Size, BitDepth.U8, 1);
			using (IplImage dispLeft = new IplImage(grayLeft.Size, BitDepth.S16, 1))
			using (IplImage dispRight = new IplImage(grayLeft.Size, BitDepth.S16, 1))
			using (IplImage dstGC = new IplImage(grayLeft.Size, BitDepth.U8, 1))
			{
				using (CvStereoGCState stateGC = new CvStereoGCState(searchRange, 2))
				{
					Cv.FindStereoCorrespondenceGC(grayLeft, grayRight, dispLeft, dispRight, stateGC, false);
					Cv.ConvertScale(dispLeft, dstGC, -16);
					dstGC.Copy(result);
				}
			}
			return result;
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
                        int offsetImg = x + y * img.WidthStep;
                        float disp = (float)imgPtr[offsetImg];
                        try
                        {
                            checked
                            {
                                CvMat cameraXYZ = DisparityToDistance((float)Q[2, 3], (float)Q[3, 2], (int)img.Width, (int)img.Height, disp, x, y);
                                returnImagePtr[offsetImg] = (float)cameraXYZ[2, 0];
                            }
                        }
                        catch (OverflowException ex)
                        {
                            continue;
                        }

                    }
                }

            }

            return returnImage;
        }






        /// <summary>
        /// Disparity画像からDepthMap画像を生成する.
        /// </summary>
        /// <param name="img">Disparity画像</param>
        /// <param name="ymlPath">ymlファイルのパス</param>
        ///<param name="minDepth">最小値の制限</param>
        ///<param name="maxDepth">最大値の制限</param>
        /// <returns>Depth情報を格納したIplImage</returns>
        public static IplImage DisparityToDepthMap(IplImage img, String ymlPath,float minDepth, float maxDepth)
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
                        int offsetImg = x + y * img.WidthStep;
                        float disp = (float)imgPtr[offsetImg];
                        try
                        {
                            checked
                            {
                                CvMat cameraXYZ = DisparityToDistance((float)Q[2, 3], (float)Q[3, 2], (int)img.Width, (int)img.Height, disp, x, y);
                                if ((float)cameraXYZ[2,0] < minDepth)
                                {
                                    returnImagePtr[offsetImg] = minDepth;
                                }
                                else if ((float)cameraXYZ[2, 0] > maxDepth)
                                {
                                    returnImagePtr[offsetImg] = maxDepth;
                                }
                                else
                                {
                                    returnImagePtr[offsetImg] = (float)cameraXYZ[2, 0];
                                }
                            }
                        }
                        catch (OverflowException ex)
                        {
                            continue;
                        }

                    }
                }

            }

            return returnImage;
        }






        /// <summary>
        /// デプスマップ画像をノーマライズして返す。表示用
        /// </summary>
        /// <param name="depth">デプスマップ画像</param>
        /// <param name="cMin" >コントラストの下限幅</param>
        /// <param name="cMax">コントラストの上限幅</param>
        /// <returns>cMin, cMaxでコントラスト調整された画像を返す</returns>
        public static IplImage normalizeIplDepth(IplImage depth, int cMin, int cMax)
        {
            //float maxValue = kaneUtility.maxIplImageVal(depth);
            //float minValue = kaneUtility.minIplImageVal(depth);
            IplImage returnImage = depth.Clone();

            unsafe
            {
                float* returnImagePtr = (float*)returnImage.ImageData;
                //正規化
                for (int y = 0; y < depth.Height; y++)
                {
                    for (int x = 0; x < depth.Width; x++)
                    {
                        int offsetImg = x + y * depth.Width;
                        //コントラストを調整する
                        returnImagePtr[offsetImg] = AdjustContrast(returnImagePtr[offsetImg], cMin, cMax);

                    }
                }
            }

            return returnImage;

        }


        /// <summary>
        /// ピクセルのコントラストを調整する
        /// </summary>
        /// <param name="pixel">float型の画素値</param>
        /// <param name="d">切り捨て部分,0~255まで</param>
        /// <returns>コントラスト調整された画素値</returns>
        private static float AdjustContrast(float pixel, int d)
        {
            if (pixel < d) return 0;
            else if (pixel > 255 - d) return 255;
            else
            {
                float a = 255 / (255 - 2 * d);
                float b = -a * d;

                return a * pixel + b;
            }
        }
        /// <summary>
        /// コントラストを上限、下限を指定して調整する。
        /// </summary>
        /// <param name="pixel"></param>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        private static float AdjustContrast(float pixel, int d1, int d2)
        {

            if (d1 == d2)
            {
                return AdjustContrast(pixel, d1);
            }
            else
            {

                if (pixel < d1) return 0;
                else if (pixel > 255 - d2) return 255;

                float a = (255 + 255 * d2) / (255 - d1);
                float b = -a * d1;

                float value = a * pixel + b;
                return value;
            }
        }
	}
}
