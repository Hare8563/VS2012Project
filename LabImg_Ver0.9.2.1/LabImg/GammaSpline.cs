using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Drawing;
using ShoNS.Array;


namespace LabImg
{
	/// <summary>
	/// スプライン補間関連クラス
	/// </summary>
	public static class GammaSpline
	{
		/// <summary>
		/// ptsファイルからfloat[240,320]データを返す
		/// </summary>
		/// <param name="pointCloudPath">ptsファイルパス</param>
		/// <returns>float[240,320]の距離データ</returns>
		public static float[,] PointCloudDataRead(string pointCloudPath)
		{
			//距離データの読み込み
			//ステレオカメラの場合240x320全てデータが取れないので
			//内挿後のデータを初期化する必要がある
			float[,] depth = new float[240, 320];

			//行ごとに読み込み
			string[] lines = File.ReadAllLines(pointCloudPath);
			for (int i = 0; i < lines.Length; i++)
			{
				//読み込んだ行を空白区切りにする
				string[] temp = lines[i].Split(" ".ToArray(), StringSplitOptions.RemoveEmptyEntries);

				//temp[6]がY座標　temp[7]がx座標　
				//temp[2]が距離 単位はm
				int x = (int)float.Parse(temp[7]) - 1;
				int y = (int)float.Parse(temp[6]) - 1;
				float z = float.Parse(temp[2]);
				depth[y, x] = z;
			}
			return depth;
		}



		/// <summary>
		/// ガンマデータと感度データを補正したものを返す
		/// </summary>
		/// <param name="gammaLogFilePath">ガンマデータのファイルパス</param>
		/// <param name="senFilePath">感度データのファイルパス</param>
		/// <returns>float[8,16]のデータ</returns>
		static public float[,] GammaDataRead(string gammaLogFilePath, string senFilePath)
		{
			//感度ファイルの読み込み
			string[] sensors = File.ReadAllText(senFilePath).Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);
			var sensorSet = sensors.Select((x, i) => new { value = float.Parse(x) });

			//ガンマログの読み込み
			var data = File.ReadAllLines(gammaLogFilePath);

			//感度ファイルとガンマデータの補正
			var gammaDataSet = data.Skip(2)
							.Select((x, i) => new
							{
								time = x.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries)[1].Replace("/", "-"),
								values = x.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries).Skip(2).Take(128)
										  .Zip(sensorSet, (num1, num2) => float.Parse(num1) * num2.value)
										  .Select((y, j) => new
										  {
											  value = y,
											  index = j
										  }),
								index = i
							});

			//128個のデータを取得分足す
			float[] sumValues = new float[128];
			foreach (var dataSet in gammaDataSet)
			{
				foreach (var values in dataSet.values)
				{
					sumValues[values.index] += values.value;
				}
			}

			//平均値を8x16の配列に入れる
			float[,] rawData = new float[8, 16];

			for (int y = 0; y < 8; y++)
				for (int x = 0; x < 16; x++)
					rawData[y, x] = sumValues[y * 16 + x] / (float)(gammaDataSet.Last().index + 1);


			//データを返す
			return rawData;
		}


		/// <summary>
		/// 東芝のデータを読み込んで2次元配列を返す(配列は[y,x]の順)
		/// </summary>
		/// <param name="path">東芝のデータ(.csv)</param>
		/// <returns>[8,16]の配列</returns>
		public static float[,] ReadGamma(string path)
		{
			var lines = File.ReadAllLines(path);
			float[,] data = new float[8, 16];
			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i].Split(",".ToCharArray());
				for (int j = 0; j < line.Length; j++)
					data[i, j] = float.Parse(line[j]);
			}
			return data;
		}

		/// <summary>
		/// 東芝のデータを[16,16]に内挿する
		/// </summary>
		/// <param name="rawData">東芝のデータ[8,16]</param>
		/// <returns>[16,16]の配列</returns>
		public static float[,] GammaSimpleInterp(float[,] rawData)
		{
			float[,] tData = new float[16, 16];

			//初期化
			for (int i = 0; i < 8; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					tData[i * 2, j] = rawData[i, j];
				}
			}

			//東芝の補正
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					if (i % 2 == 1)
					{
						if (i + 1 > 15)
						{
							//左下端
							if (j == 0) tData[i, j] = (tData[i - 1, j] + tData[i - 1, j + 1]) / 2.0F;
							//右下端
							else if (j == 15) tData[i, j] = (tData[i - 1, j - 1] + tData[i - 1, j]) / 2.0F;
							//左右下端以外の下端
							else tData[i, j] = (tData[i - 1, j - 1] + tData[i - 1, j] + tData[i - 1, j + 1]) / 3.0F;
						}
						else
						{
							tData[i, j] = (tData[i - 1, j] + tData[i + 1, j]) / 2.0F;
						}
					}
				}
			}
			return tData;
		}


		/// <summary>
		/// スプライン補間
		/// </summary>
		/// <param name="rawData">東芝のデータ</param>
		/// <param name="ny">縦方向の分割区分数</param>
		/// <param name="nx">横方向の分割区分数</param>
		/// <param name="degree">補間に利用する次数</param>
		/// <returns>[320,320]の配列データ(初期値はfloat.MinValue)</returns>
		public static float[,] GammaSplineInterp(float[,] rawData, int ny, int nx, int degree)
		{
			int mny = ny * rawData.GetLength(0);
			int mnx = nx * rawData.GetLength(1);
			float[,] result = new float[mny,mnx];

			//初期化
			for (int i = 0; i < result.GetLength(0); i++)
				for (int j = 0; j < result.GetLength(1); j++)
					result[i, j] = float.MinValue;

			//縦方向の定義
			CurveData s = new CurveData(rawData.GetLength(0), degree, ny);

			//横方向の定義
			CurveData t = new CurveData(rawData.GetLength(1), degree, nx);

			//X,Y,Z(value) の定義
			List<XYZ> xyzList = new List<XYZ>();
			for (int y = 0; y < rawData.GetLength(0); y++)
			{
				for (int x = 0; x < rawData.GetLength(1); x++)
				{
					xyzList.Add(new XYZ(x * nx + (nx/2), y * ny + (ny/2), rawData[y, x]));
				}
			}

			//スプライン計算
			BSplineCurve bC = new BSplineCurve(s, t, xyzList);

			//出力データ作成
			for (int tIndex = 0; tIndex < bC.Pos2.GetLength(0); tIndex++)
			{
				for (int sIndex = 0; sIndex < bC.Pos2.GetLength(1); sIndex++)
				{
					int yIndex = (int)Math.Round(bC.Pos2[tIndex, sIndex].Y, MidpointRounding.AwayFromZero);
					int xIndex = (int)Math.Round(bC.Pos2[tIndex, sIndex].X, MidpointRounding.AwayFromZero);
					result[yIndex, xIndex] = bC.Pos2[tIndex, sIndex].Z;
				}
			}
			return result;
		}


		/// <summary>
		/// スプライン補間したデータの面積を求める
		/// </summary>
		/// <param name="splineArray">スプライン補間された配列[320,320]</param>
		/// <returns>面積の配列[8,16]</returns>
		public static float[,] GammaAreaCal(float[,] splineArray)
		{
			int mny = splineArray.GetLength(0);
			int mnx = splineArray.GetLength(1);
			int kny = mny / 8;
			int knx = mnx / 16;
			float[,] result = new float[8, 16];

			for (int y = 0; y < result.GetLength(0); y++)
			{
				for (int x = 0; x < result.GetLength(1); x++)
				{
					//平均値計算
					float sum = 0.0f;
					float vCount = 0.0f;

					//領域指定
					int yyArea = y * kny + kny;
					int xxArea = x * knx + knx;

					for (int yy = y * kny; yy < yyArea; yy++)
					{
						for (int xx = x * knx; xx < xxArea; xx++)
						{
							if (splineArray[yy, xx] != float.MinValue)
							{
								sum += splineArray[yy, xx];
								vCount++;
							}
						}
					}
					//平均値の設定
					result[y, x] = sum / vCount;
				}
			}
			return result;
		}

  
		/// <summary>
		/// 配列[N,M]からBitmap[mny,mnx]を作成する
		/// </summary>
		/// <param name="mapData"></param>
		/// <param name="mny"></param>
		/// <param name="mnx"></param>
		/// <returns>ColorMap(初期値float.MinValueの時はBlack)</returns>
		public static Bitmap GammaBmpConv(float[,] mapData, int mny, int mnx)
		{
			Bitmap bmp = new Bitmap(mnx, mny);

			//縦のブロックサイズ
			int vBlcokSize = bmp.Height / mapData.GetLength(0);

			//横のブロックサイズ
			int hBlockSize = bmp.Width / mapData.GetLength(1);

			//最小、最大値を求める
			float min = float.MaxValue;
			float max = float.MinValue;
			for (int y = 0; y < mapData.GetLength(0); y++)
			{
				for (int x = 0; x < mapData.GetLength(1); x++)
				{
					if (mapData[y, x] != float.MinValue)
					{
						if (mapData[y, x] > max) max = mapData[y, x];
						if (mapData[y, x] < min) min = mapData[y, x];
					}
				}
			}

			//bmp作成
			for (int y = 0; y < mapData.GetLength(0); y++)
			{
				for (int x = 0; x < mapData.GetLength(1); x++)
				{
					int yyArea = vBlcokSize * y + vBlcokSize;
					int xxArea = hBlockSize * x + hBlockSize;
					for (int yy = vBlcokSize * y; yy < yyArea; yy++)
					{
						for (int xx = hBlockSize * x; xx < xxArea; xx++)
						{
							if (mapData[y, x] != float.MinValue)
							{
								float v = (mapData[y, x] - min) / (max - min);
								bmp.SetPixel(xx, yy, kaneUtility.GetScaleColor(v));
							}
							else bmp.SetPixel(xx, yy, Color.Black);
						}
					}
				}
			}
			return bmp;
		}


/*		/// <summary>
		/// mapData[N,M]から、320x320の配列を作成する
		/// </summary>
		/// <param name="mapData">配列データ</param>
		/// <returns>320x320の配列(空白の時は0)</returns>*/
		/// <summary>
		/// mapData[N,M]から、mny x mnxの配列を作成する
		/// </summary>
		/// <param name="mapData"></param>
		/// <param name="mny"></param>
		/// <param name="mnx"></param>
		/// <returns></returns>
		public static float[,] GammaConv(float[,] mapData, int mny, int mnx)
		{

			float[,] result = new float[mny,mnx];

			//縦のブロックサイズ
			int vBlcokSize = result.GetLength(0) / mapData.GetLength(0);

			//横のブロックサイズ
			int hBlockSize = result.GetLength(1) / mapData.GetLength(1);

			for (int y = 0; y < mapData.GetLength(0); y++)
			{
				for (int x = 0; x < mapData.GetLength(1); x++)
				{
					int yyArea = vBlcokSize * y + vBlcokSize;
					int xxArea = hBlockSize * x + hBlockSize;
					for (int yy = vBlcokSize * y; yy < yyArea; yy++)
					{
						for (int xx = hBlockSize * x; xx < xxArea; xx++)
						{
							if (mapData[y, x] != float.MinValue)
							{
								result[yy, xx] = mapData[y, x];
							}
							else result[yy, xx] = 0.0f;
						}
					}
				}
			}
			return result;
		}
		/// <summary>
		/// 面積調整のための収束計算、rawData[8,16]計測値を入れ、面積が同じになるスプライン補間データspData[320,320]を返す
		/// </summary>
		/// <param name="rawData"></param>
		/// <param name="maxIteration">最大繰り返し数５０が標準</param>
		/// <param name="thre">収束判定閾値０．０１くらいがよい</param>
		/// <returns></returns>
		public static float[,] splineAreaFit(float[,] rawData, int maxIteration, float thre)
		{
			//面積調整のための収束計算---------------
			float[,] g = new float[8, 16];
			float[] etrend = new float[maxIteration];
			float[,] spData = new float[320, 320];
			float[,] Er = new float[8, 16];
			float[,] SA = new float[8, 16];

			for (int j = 0; j < rawData.GetLength(0); j++)
				for (int i = 0; i < rawData.GetLength(1); i++) g[j, i] = rawData[j, i];
			int ki = 0;
//            float thre = float.Parse(textBox13.Text);
			for (int k = 0; k < maxIteration; k++)
			{
				spData = GammaSpline.GammaSplineInterp(g, 40, 20, 3);
				SA = GammaSpline.GammaAreaCal(spData);
				Er = kaneUtility.matrixDiff(rawData, SA);
				etrend[k] = kaneUtility.matrixAbsSum(Er);
				for (int y = 0; y < g.GetLength(0); y++)
				{
					for (int x = 0; x < g.GetLength(1); x++)
					{
						g[y, x] = g[y, x] + 1.0f * Er[y, x];
					}
				}
				ki = k + 1;
				if (etrend[k] < thre) break;
			}
			return spData;
		}
	}



	/// <summary>
	/// 曲面を求めるクラス
	/// </summary>
	class BSplineCurve
	{
		/// <summary>
		/// S曲線のデータ
		/// </summary>
		CurveData _s;

		/// <summary>
		/// t曲線のデータ
		/// </summary>
		CurveData _t;

		/// <summary>
		/// データの総数
		/// </summary>
		int _allPointNum;

		/// <summary>
		/// 各座標ごとの配列
		/// </summary>
		FloatArray _xArray, _yArray, _zArray;


		/// <summary>
		/// st上のXYZデータ
		/// </summary>
		public XYZ[,] Pos2;



		/// <summary>
		/// BSpline定義
		/// 曲面についてのみ実装
		/// </summary>
		/// <param name="s">縦軸データ</param>
		/// <param name="t">横軸データ</param>
		/// <param name="xyzList">x,y,zのデータリスト</param>
		public BSplineCurve(CurveData s, CurveData t, List<XYZ> xyzList)
		{
			SetData(s, t, xyzList);
			FloatArray Bd = MakeSE();
			FloatArray alp, bet, gam;
			SolveSE(Bd, out alp, out bet, out gam);
			CalFuncValue(alp, bet, gam);
		}


		/// <summary>
		/// 関数値計算
		/// Pos2を求める
		/// </summary>
		private void CalFuncValue(FloatArray alp, FloatArray bet, FloatArray gam)
		{
			float ds = 1.0f / (float)_s.DivNum;
			float dt = 1.0f / (float)_t.DivNum;
			Pos2 = new XYZ[_s.Npx, _t.Npx];
			float x_n, y_n, z_n, s, t;

			for (int n = 0; n < _s.Npx; n++)
			{
				s = n * ds;
				float[] sArray = _s.CalBSpline(s);
				for (int l = 0; l < _t.Npx; l++)
				{
					t = l * dt;
					float[] tArray = _t.CalBSpline(t);
					x_n = 0.0f;
					y_n = 0.0f;
					z_n = 0.0f;

					for (int js = 0; js < _s.PointNum; js++)
					{
						for (int jt = 0; jt < _t.PointNum; jt++)
						{
							int j = jt + js * _t.PointNum;
							x_n += alp[j] * sArray[js] * tArray[jt];
							y_n += bet[j] * sArray[js] * tArray[jt];
							z_n += gam[j] * sArray[js] * tArray[jt];
						}
					}
					Pos2[n, l] = new XYZ(x_n, y_n, z_n);
				}
			}


		}

		/// <summary>
		/// 連立方程式をXYZについてそれぞれ解く
		/// </summary>
		private void SolveSE(FloatArray Bd, out FloatArray alp, out FloatArray bet, out FloatArray gam)
		{
			LUFloat lu = new LUFloat(Bd);
			alp = lu.Solve(_xArray);
			bet = lu.Solve(_yArray);
			gam = lu.Solve(_zArray);
		}

		/// <summary>
		/// 連立方程式の係数行列作成
		/// </summary>
		private FloatArray MakeSE()
		{
			int colRow = _s.PointNum * _t.PointNum;
			FloatArray Bd = new FloatArray(colRow, colRow);
			for (int ls = 0; ls < _s.PointNum; ls++)
			{
				float s = (float)ls;
				float[] sbc = _s.CalBSpline(s);
				for (int js = 0; js < _s.PointNum; js++)
				{
					for (int lt = 0; lt < _t.PointNum; lt++)
					{
						float t = (float)lt;
						float[] tbc = _t.CalBSpline(t);
						int ltmx = lt + ls * _t.PointNum;
						for (int jt = 0; jt < _t.PointNum; jt++)
						{
							int jtmx = jt + js * _t.PointNum;
							Bd[ltmx, jtmx] = sbc[js] * tbc[jt];
						}
					}
				}
			}
			return Bd;
		}




		/// <summary>
		/// データ登録
		/// </summary>
		/// <param name="s">縦軸データ</param>
		/// <param name="t">横軸データ</param>
		/// <param name="xyzList">x,y,zのデータリスト</param>
		private void SetData(CurveData s, CurveData t, List<XYZ> xyzList)
		{
			_s = s;
			_t = t;
			_allPointNum = xyzList.Count;


			if (_allPointNum != _s.PointNum * _t.PointNum)
				throw new Exception("データの総数と曲線のデータ個数の積が合いません");


			_xArray = new FloatArray(_allPointNum, 1);
			_yArray = new FloatArray(_allPointNum, 1);
			_zArray = new FloatArray(_allPointNum, 1);

			for (int i = 0; i < xyzList.Count; i++)
			{
				_xArray[i, 0] = xyzList[i].X;
				_yArray[i, 0] = xyzList[i].Y;
				_zArray[i, 0] = xyzList[i].Z;
			}
		}
	}

	/// <summary>
	/// 3次元の点データを登録するクラス
	/// </summary>
	public class XYZ
	{
		/// <summary>
		/// X座標
		/// </summary>
		public float X { private set; get; }

		/// <summary>
		/// Y座標
		/// </summary>
		public float Y { private set; get; }

		/// <summary>
		/// Z座標
		/// </summary>
		public float Z { private set; get; }

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public XYZ(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}
	}



	/// <summary>
	/// 曲線データを登録するクラス
	/// </summary>
	class CurveData
	{
		/// <summary>
		/// 曲線上のデータ個数
		/// </summary>
		public int PointNum { private set; get; }

		/// <summary>
		/// スプラインの次数
		/// </summary>
		public int SplineDegree { private set; get; }

		/// <summary>
		/// 階数
		/// </summary>
		public int Rank { private set; get; }

		/// <summary>
		/// 分割区分数
		/// </summary>
		public int DivNum { private set; get; }

		/// <summary>
		/// 分割点の個数
		/// </summary>
		public int Npx;

		/// <summary>
		/// 節点の配列
		/// </summary>
		float[] NodeArray;

		/// <summary>
		/// 曲線のデータ定義
		/// </summary>
		/// <param name="pointNum">曲線上のデータ個数</param>
		/// <param name="splineDegree">スプラインの次数</param>
		/// <param name="divNum">分割区分数</param>
		public CurveData(int pointNum, int splineDegree, int divNum)
		{
			PointNum = pointNum;
			SplineDegree = splineDegree;
			DivNum = divNum;
			Rank = SplineDegree + 1;
			Npx = DivNum * (PointNum - 1) + 1;
			SetNodeSW();
		}

		/// <summary>
		/// パラメトリックスプラインの場合の
		/// シェーンバーグ・ホイットニの条件を満たす節点の設定
		/// </summary>
		private void SetNodeSW()
		{
			NodeArray = new float[Rank + PointNum];

			for (int i = 0; i < PointNum - Rank; i++)
				NodeArray[i + Rank] = i + Rank / 2.0f;

			for (int i = 0; i < Rank; i++)
				NodeArray[i + PointNum] = PointNum - 1;
		}


		/// <summary>
		/// de Boor Coxの漸化式からBスプライン値の計算
		/// </summary>
		public float[] CalBSpline(float t)
		{
			float[,] b = new float[PointNum, 2];
			float[] bc = new float[PointNum];
			int kset = 0;
			float a1 = 0.0f;
			float a2 = 0.0f;

			for (int i = 0; i < PointNum; i++)
			{
				if (NodeArray[i] <= t && t < NodeArray[i + 1])
				{
					b[i, 0] = 1.0f;
					kset = i;
				}
			}

			if (NodeArray[PointNum - 1] <= t && t <= NodeArray[PointNum] + 0.000001f)
			{
				b[PointNum - 1, 0] = 1.0f;
				kset = PointNum - 1;
			}

			for (int k = 1; k < Rank; k++)
			{
				for (int i = 0; i < PointNum; i++) b[i, F01(k)] = 0.0f;
				for (int i = kset - k; i <= kset; i++)
				{
					a1 = 0.0f;
					a2 = 0.0f;
					if (NodeArray[i + 1] != NodeArray[i + k + 1])
					{
						a1 = (NodeArray[i + k + 1] - t) * b[i + 1, F01(k - 1)] /
							(NodeArray[i + k + 1] - NodeArray[i + 1]);
					}
					if (NodeArray[i] != NodeArray[i + k])
					{
						a2 = (t - NodeArray[i]) * b[i, F01(k - 1)]
							/ (NodeArray[i + k] - NodeArray[i]);
					}
					b[i, F01(k)] = a1 + a2;
				}
			}

			for (int i = 0; i < PointNum; i++)
				bc[i] = b[i, F01(Rank - 1)];

			return bc;
		}


		private int F01(int k)
		{
			return k % 2;
		}



	}
}
