# セットアップ手順
1. (Chocolateyがインストールされていない場合) コマンドプロンプトを管理者権限で開き、下記のコマンドを実行する。
```
@"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -InputFormat None -ExecutionPolicy Bypass -Command "iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))" && SET "PATH=%PATH%;%ALLUSERSPROFILE%\chocolatey\bin"
```
2. コマンドプロンプトを管理者権限で開き、ffmpeg をインストールする
```
choco install ffmpeg -Y
```