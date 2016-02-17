# ごちうさ全動画検索エンジン Gochiusearch
ご注文はうさぎですか？1期・2期の全画像を対象とした検索エンジンです。
アプリケーションにキャプ画像をブラウザやフォルダからDrag&Dropすると、
何羽のどのあたりかを返します。
また、自動的にニコニコ動画の該当部分に移動することもできます。

![実行イメージ](https://raw.githubusercontent.com/wiki/ksasao/Gochiusearch/image1.png)

## ダウンロード
Windows 専用アプリです。Linux版、Mac版もリリースを予定しています。

- [Gochiusearch 1.1.5892.635](https://github.com/ksasao/Gochiusearch/blob/master/Release/Gochiusearch-1.1.5892.635.zip?raw=true) (2016/2/18)

## 技術詳細
1期、2期の動画に含まれる約100万枚のすべての画像が検索対象です。1枚の画像をわずか8バイト程度まで圧縮することで、
データベース込みで3MB程度とコンパクトになっています。また、画像の検索時間も数十ミリ秒程度と高速です。
画像は、9x8ピクセルに縮小したものをグレースケール化し、横方向に隣接するピクセル間の輝度差をビットベクトルとしています。
コア部分のみ抜き出したコードは https://gist.github.com/ksasao/e625d590801dce98c5e0 を参照してください。
