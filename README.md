# ごちうさ全動画シーン検索エンジン Gochiusearch
ご注文はうさぎですか？1期・2期・Dear My Sister の全画像100万枚以上を対象とした検索エンジン ごちうサーチ です。Windows/Mac/Linux で動作します。
アプリケーションにキャプチャ画像をブラウザやフォルダからDrag&Dropすると、
何羽のどのあたりかを返します。
また、自動的にニコニコ動画/dアニメストア/Amazon prime の該当部分に移動することもできます。

画像を特定するための情報のみが含まれているため軽量で高速に動作します。

![実行イメージ](https://raw.githubusercontent.com/wiki/ksasao/Gochiusearch/image1.png)

## ダウンロード
配布中のアプリは Windows/Mac 向けですが、Linuxでも以下の手順で動作させることができます。

### Windows 版
- [ごちうサーチ／Microsoft Store](https://www.microsoft.com/ja-jp/p/%E3%81%94%E3%81%A1%E3%81%86%E3%82%B5%E3%83%BC%E3%83%81/9njb7xgz6tk5?activetab=pivot%3Aoverviewtab) (2019/6/7)
- [Gochiusearch-1.2.7907.1331.zip](https://github.com/ksasao/Gochiusearch/files/3266334/Gochiusearch-1.2.7907.1331.zip) (2019/6/7)
- [Gochiusearch-1.1.5902.28922_1_2_dms.zip](https://github.com/ksasao/Gochiusearch/files/2540296/Gochiusearch-1.1.5902.28922_1_2_dms.zip) (2018/11/02)

### Mac 版
- [ごちうサーチ／Mac App Store](https://itunes.apple.com/jp/app/gochiusachi/id1110637036?mt=12) (2016/5/6)

## Gochiusearch Tools
手持ちの動画を Gochiusearch で検索できるようにしたい方向けツール
### Windows 版
- [GochiusearchTools 1.1.5902.28922](https://raw.githubusercontent.com/wiki/ksasao/Gochiusearch/Release/Windows/GochiusearchTools-1.1.5902.28922.zip?raw=true) (2016/2/28)
- [作成方法](https://github.com/ksasao/Gochiusearch/blob/master/src/CreateIndex/Script/readme-index.txt)
- ファイル形式は今後も変更する可能性があります。

## 技術詳細
1期、2期の動画に含まれる約100万枚のすべての画像が検索対象です。1枚の画像をわずか8バイト程度まで圧縮することで、
データベース込みで3MB程度とコンパクトになっています。また、画像の検索時間も数十ミリ秒程度と高速です。
画像は、9x8ピクセルに縮小したものをグレースケール化し、横方向に隣接するピクセル間の輝度差をビットベクトルとしています。
コア部分のみ抜き出したコードは https://gist.github.com/ksasao/e625d590801dce98c5e0 を参照してください。

アルゴリズムについては下記も参照してください。
- [ごちうサーチ - SlideShare](https://www.slideshare.net/ksasao/ss-72025009)

### ベクトル化
![ベクトル化](https://user-images.githubusercontent.com/179872/28755112-2fd68226-758e-11e7-97b3-6c4e4fbda5d4.png)
### 検索
![検索](https://user-images.githubusercontent.com/179872/28755120-63d81b34-758e-11e7-8098-fca759a13ea5.png)

## ビルド方法
### Windows
左上の「Download ZIP」からソースコードをダウンロードし、Visual Studio 2013 または Visual Studio 2015 で src\Gochiusearch.sln を開き、Gochiusearch プロジェクトをビルド、実行してください。
無料の Visual Studio Community でもビルドできます。
https://www.visualstudio.com/downloads/download-visual-studio-vs 
Mono / Xamarin は不要です。

### Mac
Xamarin Studio を利用して Mac上でビルド・実行することが可能です。
Xamarin Studio は http://www.monodevelop.com/download/ からダウンロードしてください。Mono + GTK# もインストールが必要です。
左上の「Download ZIP」からソースコードをダウンロードし、Xamarin Studio で、src/Gochiusearch.Mac.sln を開いて実行してください。

![Macでの動作](https://raw.githubusercontent.com/wiki/ksasao/Gochiusearch/mac.png)

### Linux
MonoDevelop を利用して Linux上でビルド・実行することが可能です。
Ubuntu の場合は、Ubuntu Software Center から MonoDevelop をインストールしたのち、
左上の「Download ZIP」からソースコードをダウンロードし、MonoDevelop で、src/Gochiusearch.sln を開いて実行してください。

その他のプラットフォームでは、http://www.monodevelop.com/download/ を参照してください。

![Ubuntuでの動作](https://raw.githubusercontent.com/wiki/ksasao/Gochiusearch/ubuntu.png)

