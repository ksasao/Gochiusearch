# GochusearchCmd
動画ファイルとその titleId, episodeId を指定して、index.db に追加するコマンドです。
既存の index.db は上書きされます。上書きしたくない場合は、```-o``` オプションで
出力ファイル名を指定できます。 

```GochiusearchCmd.exe -m "ご注文はうさぎですか？ BLOOM 第12羽「その一歩は君を見ているから踏み出せる」.mp4" -t 4 -e 12```

|オプション|意味|デフォルト値|
|---|---|---|
|-t, --title|titleId(作品名毎に異なるId)||
|-e, --episode|episodeId(≒話数)||
|-i, --input|入力dbファイル名(省略時は index.db)|
|-o, --output|出力dbファイル名(省略時は index.db。上書き保存。)|
|-m, --movie|入力動画ファイル名(.mp4, .aviなど)|
