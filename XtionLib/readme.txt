XtionLibrary
開発者: m5181145 晴佐久 哲士
使用ライブラリ: 
Zigfu Browser Plugin & OpenNI http://zigfu.com/en/downloads/browserplugin/

※Any CPU, x86でのコンパイル時のみ使えます。OpenCVSharpやLablibraryと併用する場合はOpenCVのパスをx86のものにし
32bit用のものにして使用してください

【概要】
OpenNI & Xtionを使用した距離計測を簡単に行うためのライブラリです

【使い方】
1, 参照にxtionLibを追加
2, usingでXtionLibを使用
3, XtionUtilityクラスをインスタンス化
4.1, doNormalDepth()で距離データを取得
4.2, doColor()でRGB画像を取得
5, timerやスレッドで連続して呼び出す場合context.WaitAndUpdateAll()を使用した後に4.1, 4.2を行う
6, RGB画像と距離画像を一致させる際はalterNate()を使用する
7, WPFアプリケーションではSystem.Drawing.Bitmapは使えない？ため4.1, 4.2の戻り値であるBitmapをImageSourceに変換する。
　その際、ConvertImageSourceFromBitmap(Bitmap)で変換させる

【バージョン履歴】
4/8  v0.3 ConvertImageSourceFromBitmap追加
3/18 v0.2、 WaitAndUpdateを外部で呼び出すように変更
3/14 v0.1作成