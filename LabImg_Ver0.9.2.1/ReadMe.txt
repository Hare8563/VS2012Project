Application Development Environment
Visual Studio 2012 Professional

作成者　
兼本　茂　　　 kanemoto@u-aizu.ac.jp
丹野　嘉信　　 s1130151@gmail.com
晴佐久 哲士　　haresaku1024@gmail.com
山田　国広　　 s1190203@u-aizu.ac.jp


設定方法
(注意)
ver0.9.0から使用するOpenCVとOpenCVSharpを2.4.8に変更しています。
使用する際はパスの設定等を適宜変更してから使用してください。

VisualStudio
アンセーフコードを許可すること
プラットフォームターゲット x64

システムパス
Opencv2.4.8のbin以下の内容を登録すること

その他
Sho をインストール、設定すること
SharpGLの設定をすること

Program Language
C#

URL

OpenCV2.4.8
http://sourceforge.net/projects/opencvlibrary/files/opencv-win/2.4.8/


OpenCVSharp 2.4.8(x64)
https://github.com/shimat/opencvsharp/releases/download/2.4.8.20140510/OpenCvSharp-2.4.8-x64-20140510.zip

OpenCVSharp 2.4.8(x86)Any CPUでビルドする場合はこちら。XtionLibと併用する場合はこちらでないとだめです
https://github.com/shimat/opencvsharp/releases/download/2.4.8.20140510/OpenCvSharp-2.4.8-x86-20140510.zip

Sho
http://msdn.microsoft.com/en-us/devlabs/gg585581.aspx

SharpGL
http://sharpgl.codeplex.com/


OpenCV Algorithm

BlockMatch
http://opencv.jp/opencv-2svn/c/camera_calibration_and_3d_reconstruction.html

DPMatch
http://opencv.jp/opencv-2.1/cpp/camera_calibration_and_3d_reconstruction.html

GraphCut 
http://opencv.willowgarage.com/documentation/python/camera_calibration_and_3d_reconstruction.html



VersionL Log
2014/07/15 ver0.9.2.1
追加者　晴佐久
正規乱数を発生させるクラスNormalRandomを追加
線分の交差判定を修正

2014/07/14 ver 0.9.2
追加者 晴佐久
三角形と線分の交差判定を追加


2014/06/17 ver0.9.1
追加者　山田国広
一次元配列の中央値、平均値、最頻値の計算を追加

2014/05/16　ver0.9.0
追加者　晴佐久
OpenCVのバージョンを2.4.8へとアップデート
晴佐久のライブラリHTUtilityを追加
プログレスダイアログを使用するProgressFormクラスを追加

2014/04/29
追加者　晴佐久 ver0.8.5
ChartUtility内に複数本のグラフを描けるようにした
ChartUtility.PlotChart(chart, object型にキャストしたfloat型一次元配列を複数指定)
で実行可能



2013/11/26
追加者　晴佐久　ver0.8.2
Disparity内にDisparity画像からDepthMap画像を作成する
DisparityToDepthMapを追加
DepthMap画像をノーマライズして返すnormalizeIplDepthを追加


2013/10/30
追加者　晴佐久 ver0.8.1
ChartUtility内のPlotChartを一本だけも引けるように
オーバーライドを作成


2013/09/04
追加者　丹野　ver0.8.0
GammaSplineクラスに
PointCloudDataReadとGammaDataReadの追加


2013/07/18
追加者　丹野　ver0.6.7
CDPの引数を更新

2013/06/07
追加者　丹野　ver0.6.7
kaneUtilityのSetDisparityを修正

2013/06/07
追加者　丹野　ver0.6.5
ShapeReconstructクラスのFundamentalMatrixとShapeReconstruction関数に
CvMat用のオーバーライド追加


2013/06/05
追加者　丹野　ver0.6.4
kanemotoDPMatchのオーバーライド追加
kanemotoDPMatch、kanemotoBlockMatchの返り値をFloatArrayからIplImageに変更
distTester（サンプルプログラム）の修正,DisparitySampleの削除
資料に「ステレオカメラ作成の道」を追加

2013/06/05
追加者　兼本　ver0.6.3
KanemotoDPMatchの修正

2013/06/04
追加者　丹野　ver0.6.2
Disparityクラスの各関数のオーバーライド
DistTesterを追加

2013/06/03
追加者　兼本　ver0.6.1
DisparityクラスのkanemotoBlockMatchを修正

2013/5/27 ver0.6.0
追加者　丹野
兼本先生が書いたmatlabのShapeReconstructionを追加
ShapeReconstructionのテスト用プログラム　ShapeReconstructTestを追加

2013/5/16 晴佐久修正
ImgLabのDisparityにtwoDCDPによるDisparityの算出をおこなうTwoDCDP()メソッドを追加
kaneUtility内のグレイスケール変換におけるアルゴリズムを修正

2013/5/10
MATLABのBlockMatch、DPmatchを実装(Disparity.cs）入力はFloatArrayに統一
FloatArrayとIPLImageの相互変換を実装、FloatArrayは、グレイスケール（0-255)ないし色コードを用いてRGB（24bit)に変換される
ただし、FloatArrayグレイスケールは０－１に規格化
FloatArrayー＞IplImageー＞.ToBitmap()で、PictureBoxに表示される。
ステレオマッチングは全て、floatImageRead（FloatArrayとして読み込み）かgrayImageRead(IplImageとして読み込み）で、
データ入力を統一すること。この際、γ変換も実施する。

2013/5/9
ChartUtilityクラスを追加して、時系列プロット、相関プロットなどを独立させた。
カラー変換コードなど、Doubleでなく、Floatをデフォルトとした。
Ver5.2kとして格納

2013/05/6
kaneUtilityに、画像の平滑化、面積計算、画像拡大(PyramiddUp）、カラーコード変換を追加。
diaparityクラスの引数を、FloatMatrixに統一
ver5.1kとして格納
３dcdpを、ほかと同じ形式で呼べるように改造したい
[3]単純ブロックマッチングの実装がしたい

2013/04/16
TomasMollerの交差判定を追加
線分同士の交差判定を追加　（データ検証が必要)

2013/04/13
Triangleクラスに面積を求める関数　GetAreaを追加
PvectorクラスにSub（自身ー引数のXYZ）関数を追加

2013/04/10
無限平面上の衝突判定を追加
ラプラシアンスムージングを追加（アルゴリズムの検証が必要）

2013/04/09 
Triangleクラスの追加　
Delaunayクラス周りを整理　
PVectorクラスに内積・外積関数追加

2013/04/04 
ドロネー三角分割を求めるプログラムを追加

2013/04/03 
視差を求めるプログラムの追加

