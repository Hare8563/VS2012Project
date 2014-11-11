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
using System.Collections;

namespace LabImg
{
	/// <summary>
	/// よく使う関数群をまとめたクラス
	/// </summary>
	public static class CommonUtility
	{

        /// <summary>
        /// 位相限定相関によるシフト量推定
        /// </summary>
        /// <param name="RSimg"></param>
        /// <param name="LSimg"></param>
        /// <param name="fitRange">-fitRange～＋fitRangeのデータを使ってピークを内挿</param>
        /// <param name="fitOpt">fitOpt=0.0で二次関数Fitting、=1.0でSinc関数Fitting</param>
        /// <param name="filterOpt">2以下ではフィルターをかけない。周波数が0-nfft/2の範囲で指定（４－８程度を指定）</param>
        /// <param name="sftX"></param>
        /// <param name="sftY"></param>
        /// <returns></returns>
        public static IplImage pocDisplacement(IplImage RSimg, IplImage LSimg, int fitRange, float fitOpt, int filtetOpt, out float sftX, out float sftY)
        {
            int wd = RSimg.Width; int ht = RSimg.Height;
            IplImage icfRe = new IplImage(RSimg.Size, BitDepth.F64, 1);
            IplImage icf = new IplImage(RSimg.Size, BitDepth.F64, 2);

            using (IplImage realImg = new IplImage(RSimg.Size, BitDepth.F64, 1))
            using (IplImage imagImg = new IplImage(RSimg.Size, BitDepth.F64, 1))
            using (IplImage RSfftImg = new IplImage(RSimg.Size, BitDepth.F64, 2))
            using (IplImage LSfftImg = new IplImage(RSimg.Size, BitDepth.F64, 2))
            using (IplImage CfImg = new IplImage(RSimg.Size, BitDepth.F64, 2))
            using (IplImage srcRe1 = new IplImage(RSimg.Size, BitDepth.F64, 1))
            using (IplImage srcIm1 = new IplImage(RSimg.Size, BitDepth.F64, 1))
            using (IplImage srcRe2 = new IplImage(RSimg.Size, BitDepth.F64, 1))
            using (IplImage srcIm2 = new IplImage(RSimg.Size, BitDepth.F64, 1))
            using (IplImage dstRe = new IplImage(RSimg.Size, BitDepth.F64, 1))
            using (IplImage dstIm = new IplImage(RSimg.Size, BitDepth.F64, 1))
            using (IplImage icfIm = new IplImage(RSimg.Size, BitDepth.F64, 1))
            {
                // Set input of FFT
                IplImage hwin = hanningWindow(wd, ht);
                RSimg = hanningWindowMultiple(RSimg, hwin);
                LSimg = hanningWindowMultiple(LSimg, hwin);
                Cv.CvtScale(RSimg, realImg, 1, 0);
                Cv.Zero(imagImg);
                Cv.Merge(realImg, imagImg, null, null, RSfftImg);
                Cv.DFT(RSfftImg, RSfftImg, DFTFlag.Forward);
                Cv.CvtScale(LSimg, realImg, 1, 0);
                Cv.Zero(imagImg);
                Cv.Merge(realImg, imagImg, null, null, LSfftImg);
                Cv.DFT(LSfftImg, LSfftImg, DFTFlag.Forward);
                Cv.Split(RSfftImg, srcRe1, srcIm1, null, null);
                Cv.Split(LSfftImg, srcRe2, srcIm2, null, null);
                for (int i = 0; i < ht; i++)
                {
                    for (int j = 0; j < wd; j++)
                    {
                        // 画像をかけあわせる F×G*
                        dstRe[i, j] = (srcRe1[i, j] * srcRe2[i, j] + srcIm1[i, j] * srcIm2[i, j]) ;
                        dstIm[i, j] = (srcRe2[i, j] * srcIm1[i, j] - srcRe1[i, j] * srcIm2[i, j]) ;
                        // 絶対値を計算しその値で割る |F×G*|
                        double spectrum = Math.Sqrt(dstRe[i, j] * dstRe[i, j] + dstIm[i, j] * dstIm[i, j]);
                        //double spectrum1 = Math.Sqrt(srcRe1[i, j] * srcRe1[i, j] + srcIm1[i, j] * srcIm1[i, j]);
                        //double spectrum2 = Math.Sqrt(srcRe2[i, j] * srcRe2[i, j] + srcIm2[i, j] * srcIm2[i, j]);
                        dstRe[i, j] = dstRe[i, j] / spectrum;
                        dstIm[i, j] = dstIm[i, j] / spectrum;
                    }
                }
                // 計算結果を統合する
                Cv.Merge(dstRe, dstIm, null, null, icf);
                if (filtetOpt > 2)
                {
                    icf = lpfImage(icf, icf.Width / filtetOpt, icf.Height / filtetOpt); //LowPassFilter
                    //                icf = bpfImage(icf, 0,icf.Width / 4, 0,icf.Height / 4); //BandPassFilter
                }
                Cv.DFT(icf, icf, DFTFlag.Inverse);
                Cv.Split(icf, icfRe, icfIm, null, null);
            }
            float icfMax; int xi; int yi;
            maxIplImage(icfRe, out icfMax, out  xi, out yi);
            float[] resX = new float[wd];
            float[] resY = new float[ht];
            for (int i = 0; i < wd; i++) resX[i] = (float)icfRe[yi, i];
            for (int i = 0; i < ht; i++) resY[i] = (float)icfRe[i, xi];
            if (fitRange <= 0)
            {
                if (xi <= wd / 2) sftX = -xi + 1;  //modiy 2014/03/04
                else sftX = wd - xi + 1;
                if (yi <= ht / 2) sftY = -yi + 1; //%modiy 2014/03/04
                else sftY = ht - yi + 1;
            }
            else
            {
                sftX = delayFit(resX, fitRange, fitOpt);
                sftY = delayFit(resY, fitRange, fitOpt);// %二次関数フィッティング　左上コーナーを(0,0)として、ピーク位置を算出
                if (xi <= wd / 2) sftX = -sftX;
                else sftX = wd - sftX; // %mod at 2014/3/4
                if (yi <= ht / 2) sftY = -sftY;
                else sftY = ht - sftY; // %mod at 2014/3/4
            }

            //rearrange quadrant
            double tmp13, tmp24;
            var cy = ht / 2;
            var cx = wd / 2;

            for (int j = 0; j < cy; j++)
            {
                for (int i = 0; i < cx; i++)
                {
                    tmp13 = icfRe[j, i].Val0;
                    Cv.Set2D(icfRe, j, i, icfRe[j + cy, i + cx].Val0);
                    Cv.Set2D(icfRe, j + cy, i + cx, tmp13);
                    tmp24 = icfRe[j, i + cx].Val0;
                    Cv.Set2D(icfRe, j, i + cx, icfRe[j + cy, i].Val0);
                    Cv.Set2D(icfRe, j + cy, i, tmp24);
                }
            }
            return icfRe;
        }

        /// <summary>
        /// Rimgを基準にターゲットが,Limgで、（sftX、sftY）ピクセル移動している
        /// </summary>
        /// <param name="Rimg">右画像の選択ブロック</param>
        /// <param name="Limg">左画像の選択ブロック</param>
        /// <param name="mindx">探索範囲（X）</param>
        /// <param name="maxdx">探索範囲</param>
        /// <param name="mindy">探索範囲（Y）</param>
        /// <param name="maxdy">探索範囲</param>
        /// <param name="fitRange">fitRange=0でfitなし、その他は、-fitrange..fitRangeでの二次関数補間</param>
        /// <param name="fitOpt">fitOpt=0.0で二次関数fit、=1.0でSinc関数Fit</param>
        /// <param name="sftX">出力（X方向シフト）</param>
        /// <param name="sftY">出力（Y方向シフト）</param>
        public static IplImage blockmatchDisplacement(IplImage Rimg, IplImage Limg, int mindx, int maxdx, int mindy, int maxdy, int fitRange, float fitOpt, out float sftX, out float sftY)
        {
            int mx0 = Rimg.Width;
            int my0 = Rimg.Height;
            int dlxmax = maxdx - mindx + 1;
            int dlymax = maxdy - mindy + 1;
            IplImage cor = new IplImage(dlxmax, dlymax, BitDepth.F64, 1);
            for (int dlyX = 0; dlyX < dlxmax; dlyX++)
            {
                int dlx = dlyX + mindx;//-1;
                for (int dlyY = 0; dlyY < dlymax; dlyY++)
                {
                    int dly = dlyY + mindy;//-1;
                    double sum = 0.0;
                    int no = 0;
                    for (int i = 0; i < mx0; i++)
                    {
                        for (int j = 0; j < my0; j++)
                        {
                            int ii = i + dlx; int jj = j + dly;
                            int flag = 0;
                            if (ii < 0) flag = 1;
                            if (ii >= mx0) flag = 1;
                            if (jj < 0) flag = 1;
                            if (jj >= my0) flag = 1;
                            if (flag == 0)
                            {
                                sum += Math.Abs(Rimg[j, i] - Limg[jj, ii]);
                                no++;
                            }
                        }
                    }
                    if (no > 0) cor[dlyY, dlyX] = sum / (double)no;
                }
            }
            if (fitRange == 0)
            {
                float wmin; int xi; int yi;
                minIplImage(cor, out wmin, out  xi, out yi);//[w,ix,iy]=max2(-sim);
                sftX = xi + mindx;//-1;
                sftY = yi + mindy;//-1;
            }
            else
            {
                float wmin; int xi; int yi;
                minIplImage(cor, out wmin, out  xi, out yi);//[w,ix,iy]=max2(-sim);
                float[] resX = new float[dlxmax];
                float[] resY = new float[dlymax];
                for (int i = 0; i < dlxmax; i++) resX[i] = -(float)cor[yi, i].Val0;
                for (int i = 0; i < dlymax; i++) resY[i] = -(float)cor[i, xi].Val0;
                sftX = delayFit(resX, fitRange, fitOpt) + mindx;
                sftY = delayFit(resY, fitRange, fitOpt) + mindy;// %二次関数フィッティング　左上コーナーを(0,0)として、ピーク位置を算出
            }
            return cor;
        }
        /// <summary>
        /// 矩形領域を抽出する。左上と右下座標を指定。IplImageは、Byte単位のRGBかGray
        /// </summary>
        /// <param name="img"></param>
        /// <param name="xc"></param>
        /// <param name="yc"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        public static IplImage getRectCenter(IplImage img, int xc, int yc, int w, int h)
        {
            int w2 = w / 2;
            int h2 = h / 2;
            int xs = xc - w2;
            int xe = xc + w2;
            int ys = yc - h2;
            int ye = yc + h2;
            int xb = 0;
            int yb = 0;
            if (xs < 0) { xb = -xs; xs = 0; }
            if (xe > img.Width) xe = img.Width;
            if (ys < 0) { yb = -ys; ys = 0; }
            if (ye > img.Height) ye = img.Height;
            int nc = img.NChannels;
            IplImage rimg = new IplImage(w, h, img.Depth, nc);
            Cv.Zero(rimg);
            /*  float[,] xxOrg = new float[h,w];
              float[,] xx = new float[h,w];
              for (int j = 0; j < h; j++)
                  for (int i = 0; i <w; i++)
                  {
                      xx[j, i] = (float)rimg[j, i];
                      xxOrg[j, i] = (float)img[j + yc, i + xc];
                  }
              int ww = w;*/
            unsafe
            {
                byte* ptrSrc = (byte*)img.ImageData;
                byte* ptrDst = (byte*)rimg.ImageData;
                for (int x = xs; x < xe; x++)
                {
                    for (int y = ys; y < ye; y++)
                    {
                        int offsetSrc = (img.WidthStep * y) + nc * x;
                        int offsetDst = (rimg.WidthStep * (y - ys + yb)) + nc * (x - xs + xb);
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
        /// 複素フーリエ変換の結果のLowPassFilter
        /// </summary>
        /// <param name="fimgOrg">2chのIplImage</param>
        /// <param name="fhx">Width以内の整数</param>
        /// <param name="fhy">Height以内の整数</param>
        /// <returns>IplImage</returns>
        public static IplImage lpfImage(IplImage fimgOrg, int fhx, int fhy)
        {
            int wd = fimgOrg.Width;
            int ht = fimgOrg.Height;
            if (fhx > (wd / 2)) fhx = (wd / 2);
            if (fhy > (ht / 2)) fhy = (ht / 2);
            IplImage fimg = new IplImage(wd, ht, BitDepth.F64, 2);
            fimg = fimgOrg;
            //一括代入可能か？２ｃｈに同時にゼロが入るか
            for (int i = fhx; i < wd - fhx - 1; i++)
            {
                for (int j = 0; j < fhy; j++)
                {
                    fimg[j, i] = 0.0;
                }
                for (int j = ht - fhy - 1; j < ht; j++)
                {
                    fimg[j, i] = 0.0;
                }
            }
            for (int i = 0; i < wd; i++)
            {
                for (int j = fhy; j < ht - fhy - 1; j++)
                {
                    fimg[j, i] = 0.0;
                }
            }
            //fimg(flx:fhx,fly:fhy)=fimgOrg(flx:fhx,fly:fhy);
            //fimg((nx-fhx):(nx-flx+1),fly:fhy)=fimgOrg((nx-fhx):(nx-flx+1),fly:fhy);
            //fimg(flx:fhx,(ny-fhy):(ny-fly+1))=fimgOrg(flx:fhx,(ny-fhy):(ny-fly+1));
            //fimg((nx-fhx):(nx-flx+1),(ny-fhy):(ny-fly+1))=fimgOrg((nx-fhx):(nx-flx+1),(ny-fhy):(ny-fly+1));
            return fimg;
        }

        /// <summary>
        /// 周波数領域でのバンドパスフィルタリング
        /// </summary>
        /// <param name="imgOrg">複素領域の画像FFT結果</param>
        /// <param name="flx"></param>
        /// <param name="fhx"></param>
        /// <param name="fly"></param>
        /// <param name="fhy"></param>
        /// <returns></returns>
        public static IplImage bpfImage(IplImage imgOrg, int flx, int fhx, int fly, int fhy)
        {
            //%array  (1,2:nx/2, nx/2+1, nx/2+2:nx)  ---> 1,(nx/2-1), 1, (nx/2-1)ww
            int wd = imgOrg.Width;
            int ht = imgOrg.Height;
            int wd2 = wd / 2;
            int ht2 = ht / 2;
            IplImage img = new IplImage(wd, ht, BitDepth.F64, 2);
            if (flx < 0) flx = 0;
            if (fly < 0) fly = 0;
            if (fhx > wd2) fhx = 2;
            if (fhy > ht2) fhy = ht2;
            for (int j = fly; j < fhy; j++)
            {
                for (int i = flx; i < fhx; i++) img[j, i] = imgOrg[j, i];
                for (int i = wd - fhx; i < wd - flx; i++) img[j, i] = imgOrg[j, i];
            }
            for (int j = ht - fhy; j < ht - fly; j++)
            {
                for (int i = flx; i < fhx; i++) img[j, i] = imgOrg[j, i];
                for (int i = wd - fhx; i < wd - flx; i++) img[j, i] = imgOrg[j, i];
            }
            //fimg(flx:fhx,fly:fhy)=fimgOrg(flx:fhx,fly:fhy);
            //fimg((nx-fhx):(nx-flx+1),fly:fhy)=fimgOrg((nx-fhx):(nx-flx+1),fly:fhy);
            //fimg(flx:fhx,(ny-fhy):(ny-fly+1))=fimgOrg(flx:fhx,(ny-fhy):(ny-fly+1));
            //fimg((nx-fhx):(nx-flx+1),(ny-fhy):(ny-fly+1))=fimgOrg((nx-fhx):(nx-flx+1),(ny-fhy):(ny-fly+1));
            return img;
        }

        /// <summary>
        /// ハニングウィンドう（２乗）
        /// </summary>
        /// <param name="wd"></param>
        /// <param name="ht"></param>
        /// <returns></returns>
        public static IplImage hanningWindow(int wd, int ht)
        {
            //% hannig window
            IplImage hwin = new IplImage(wd, ht, BitDepth.F64, 1);
            for (int i = 0; i < wd; i++)
            {
                for (int j = 0; j < ht; j++)
                {
                    double ww = (1.0 + Math.Cos((double)(i - wd / 2) * Math.PI / wd)) * (1.0 + Math.Cos((double)(j - ht / 2) * Math.PI / ht)) / 4.0;
                    hwin[j, i] = ww;
                }
            }
            return hwin;
        }
        /// <summary>
        /// 画像にハニングウィンドウをかける
        /// </summary>
        /// <param name="Img"></param>
        /// <param name="hwin"></param>
        /// <returns></returns>
        public static IplImage hanningWindowMultiple(IplImage Img, IplImage hwin)
        {
            IplImage Himg = new IplImage(Img.Size, BitDepth.F64, 1);
            for (int i = 0; i < Img.Height; i++)
            {
                for (int j = 0; j < Img.Width; j++)
                {
                    Himg[i, j] = Img[i, j].Val0 * hwin[i, j].Val0;
                }
            }
            return Himg;
        }
        /// <summary>
        /// IplImage.Val0　の最大値とその場所を求める
        /// </summary>
        /// <param name="I"></param>
        /// <param name="fmax"></param>
        /// <param name="xi"></param>
        /// <param name="yi"></param>
        public static void maxIplImage(IplImage I, out float fmax, out int xi, out int yi)
        {
            int NY = I.Height;
            int NX = I.Width;
            fmax = float.MinValue; xi = 0; yi = 0;
            for (int y = 0; y < NY; y++)
            {
                for (int x = 0; x < NX; x++)
                {
                    if (fmax < I[y, x].Val0)
                    {
                        fmax = (float)I[y, x].Val0; xi = x; yi = y;
                    }
                }
            }
            return;
        }
        /// <summary>
        /// IplImage.Val0　の最小値とその場所を求める
        /// </summary>
        /// <param name="I"></param>
        /// <param name="fmin"></param>
        /// <param name="xi"></param>
        /// <param name="yi"></param>
        public static void minIplImage(IplImage I, out float fmin, out int xi, out int yi)
        {
            int NY = I.Height;
            int NX = I.Width;
            fmin = float.MaxValue; xi = 0; yi = 0;
            for (int y = 0; y < NY; y++)
            {
                for (int x = 0; x < NX; x++)
                {
                    if (fmin > I[y, x].Val0)
                    {
                        fmin = (float)I[y, x].Val0; xi = x; yi = y;
                    }
                }
            }
            return;
        }
        /// <summary>
        /// 配列r(0..mmm-1)の最大値を探して、Sinc関数または二次関数で最大位置を求める。
        /// </summary>
        /// <param name="r">mmmの配列r(0..mmm-1)</param>
        /// <param name="d">Peak-d～Peak+dの2d+1点でFitting</param>
        /// <param name="VN">VN=0.0で二次関数Fitting、=1.0でSinc関数Fitting</param>
        /// <returns>戻り値は、始点からの距離（始点＝０で一点のズレ＝１．０）</returns>
        public static float delayFit(float[] r, int d, float VN)
        {
            float sft = 0.0f;
            int n = r.Length;
            float rmax = r[0];
            int mi = 0;
            for (int i = 1; i < n; i++)
            {
                if (r[i] > rmax)
                {
                    rmax = r[i]; mi = i;
                }
            }
            int mp = mi + d;
            int mm = mi - d;
            int md = 2 * d + 1;
            float[] z = new float[md];
            if (mp >= n)
            {
                for (int i = 0; i < (n - mm); i++) z[i] = r[mm + i];
                for (int i = 0; i <= (mp - n); i++) z[i + n - mm] = r[i];

            }
            else if (mm < 0)
            {
                for (int i = 0; i <= mp; i++) z[i - mm] = r[i];
                for (int i = n + mm; i < n; i++) z[i - n - mm] = r[i];
            }
            else
            {
                for (int i = 0; i < md; i++) z[i] = r[mm + i];
            }
            if (VN > 0.0)
            { // sinc function assumption
                float rm = z[0]; float ri = z[d]; float rp = z[2 * d];
                float uu = rm + rp - (2.0f * (float)Math.Cos(VN * (float)d * (float)Math.PI) * ri);
                float vv = (float)d * (rm - rp);
                sft = (float)mi - (vv / uu);
            }
            else
            {  // second order function assumption
                float morg = (float)mm;
                FloatArray X = new FloatArray(md, 3);
                FloatArray ZZ = new FloatArray(md);
                for (int i = 0; i < md; i++)
                {
                    X[i, 0] = (float)(i * i);
                    X[i, 1] = (float)i;
                    X[i, 2] = 1.0f;
                    ZZ[i] = z[i];
                }
                FloatArray XT = X.Transpose();
                FloatArray XI = (XT * X).Inv();
                FloatArray C = XI * XT * ZZ.Transpose();
                sft = -C[1] / C[0] / 2.0f + morg;
            }
            return sft;
        }

       /// <summary>
		///     指定した精度の数値に切り捨てします。</summary>
		/// <param name="dValue">
		///     丸め対象の倍精度浮動小数点数。</param>
		/// <param name="iDigits">
		///     戻り値の有効桁数の精度。</param>
		/// <returns>
		///     iDigits に等しい精度の数値に切り捨てられた数値。</returns>
		public static double ToRoundDown(double dValue, int iDigits)
		{
			double dCoef = System.Math.Pow(10, iDigits);

			return dValue > 0 ? System.Math.Floor(dValue * dCoef) / dCoef :
								System.Math.Ceiling(dValue * dCoef) / dCoef;
		}


		/// <summary>
		/// BitmapをPictureBox内に収める関数
		/// </summary>
		/// <param name="src">bitmap</param>
		/// <param name="renderBox">描写するPictureBox</param>
		public static void FillPicBox(Bitmap src, PictureBox renderBox)
		{
			using (src)
			{
				if (renderBox.Image != null) renderBox.Image.Dispose();
				var bmp2 = new Bitmap(renderBox.Width, renderBox.Height);
				using (var g = Graphics.FromImage(bmp2))
				{
					if (renderBox.Image != null) renderBox.Image.Dispose();

					g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
					g.DrawImage(src, new Rectangle(Point.Empty, bmp2.Size));
					renderBox.Image = bmp2;
				}
			}
		}


		/// <summary>
		/// 2点間の距離を求める
		/// </summary>
		/// <param name="p1">座標　1</param>
		/// <param name="p2">座標　2</param>
		/// <returns>2点間の距離</returns>
		public static float GetDist(PVector p1, PVector p2)
		{
			float width = p1.X - p2.X;
			float height = p1.Y - p2.Y;
			float depth = p1.Z - p2.Z;

			float dist = (float)(Math.Sqrt(Math.Pow(width, 2) +
												Math.Pow(height, 2) +
												Math.Pow(depth, 2)));

			return dist;
		}
		//-----　General Library for Gamma-camera image processing Prepared by Kanenmoto(2013-3-11
		/// <summary>
		/// 未完成の２次元メディアンフィルタ
		/// </summary>
		/// <param name="Din"></param>
		/// <param name="Dout"></param>
		/// <param name="M"></param>
		public static void medianFilter(DoubleArray Din, DoubleArray Dout, int M)
		{
			int ni = Din.size0;
			int nj = Din.size1;

			for (int i = 0; i < ni; i++)
			{
				for (int j = 0; j < nj; j++)
				{
					int ii1 = i - M;
					int ii2 = i + M;
					int jj1 = j - M;
					int jj2 = j + M;
					if (ii1 < 0) ii1 = 0;
					if (ii2 >= ni) ii2 = ni - 1;
					if (jj1 < 0) jj1 = 0;
					if (jj2 >= nj) jj2 = nj - 1;
					DoubleArray D = Din.GetSlice(ii1, ii2, jj1, jj2);
					double Dm = D.Median();
					Dout[i, j] = Dm;
				}
			}
		}
		/// <summary>
		/// 未完成
		/// </summary>
		/// <param name="Din"></param>
		/// <param name="Dout"></param>
		/// <param name="M"></param>
		public static void meanFilter(DoubleArray Din, DoubleArray Dout, int M)
		{
			int ni = Din.size0;
			int nj = Din.size1;
			for (int i = 0; i < ni; i++)
			{
				for (int j = 0; j < nj; j++)
				{
					int ii1 = i - M;
					int ii2 = i + M;
					int jj1 = j - M;
					int jj2 = j + M;
					if (ii1 < 0) ii1 = 0;
					if (ii2 >= ni) ii2 = ni - 1;
					if (jj1 < 0) jj1 = 0;
					if (jj2 >= nj) jj2 = nj - 1;
					DoubleArray D = Din.GetSlice(ii1, ii2, jj1, jj2);
					double Dm = D.Mean();
					Dout[i, j] = Dm;
				}
			}
		}
		/// <summary>
		/// 多角形の面積計算
		/// </summary>
		/// <param name="S">s(i,j)：i=1-3；３次元空間の座標、j=0-np-1　頂点の数</param>
		/// <returns>戻り値は面積</returns>
		public static double PolygonArea(DoubleArray S)
		{
			int np = S.size1;
			double Area = 0;

			for (int i = 2; i < np; i++)
			{
				double s12 = Math.Sqrt(Math.Pow(S[0, 0] - S[0, i - 1], 2) + Math.Pow(S[1, 0] - S[1, i - 1], 2) + Math.Pow(S[2, 0] - S[2, i - 1], 2));
				double s23 = Math.Sqrt(Math.Pow(S[0, i - 1] - S[0, i], 2) + Math.Pow(S[1, i - 1] - S[1, i], 2) + Math.Pow(S[2, i - 1] - S[2, i], 2));
				double s13 = Math.Sqrt(Math.Pow(S[0, 0] - S[0, i], 2) + Math.Pow(S[1, 0] - S[1, i], 2) + Math.Pow(S[2, 0] - S[2, i], 2));
				double ss = (s12 + s23 + s13) / 2;
				Area = Area + Math.Sqrt(ss * (ss - s12) * (ss - s23) * (ss - s13));
			}
			return Area;
		}

        /// <summary>
        /// 一次元配列の中央値の計算
        /// </summary>
        /// <param name="data">一次元配列</param>
        /// <returns>戻り値は中央値</returns>
       public static float median(float[] data)
        {
            Array.Sort(data);

            if (data.Length % 2 == 0) return (data[data.Length / 2] + data[(data.Length - 2) / 2]) / 2;
            else return data[(data.Length - 1) / 2];
        }
        /// <summary>
        /// 一次元配列の最頻値の計算
        /// </summary>
        /// <param name="data">一次元配列</param>
        /// <returns>戻り値は最頻値</returns>
        public static float mode(float[] data)
        {

            Hashtable ht = new Hashtable();
            int max = 0;
            float result = 0.0f;
            for (int i = 0; i < data.Length; i++)
            {
                if (ht.ContainsKey(data[i]))
                {
                    object val = 0;
                    val = ht[data[i]];
                    ht[data[i]] = (object)((int)val + 1);
                    if (max < (int)ht[data[i]])
                    {
                        max = (int)ht[data[i]];
                        result = data[i];
                    }
                }
                else
                {
                    ht[data[i]] = 1;
                }
            }

            return result;
        }

        /// <summary>
        /// 一次元配列の平均値の計算
        /// </summary>
        /// <param name="data">一次元配列</param>
        /// <returns>戻り値は平均値</returns>
       public static float average(float[] data)
        {
            int i;
            float sum = 0;

            for (i = 0; i < data.Length; i++)
            {
                sum += data[i];
            }

            return sum / data.Length;
        }

	}
}
