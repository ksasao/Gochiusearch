SET OUTPUT=..\..\Tools
mkdir %OUTPUT%
copy /y bin\debug\readme.txt %OUTPUT%
copy /y bin\debug\Gochiusearch.exe %OUTPUT%
copy /y bin\debug\index.db %OUTPUT%
copy /y bin\debug\index.txt %OUTPUT%

copy /y ..\CreateIndex\bin\debug\CreateIndex.exe %OUTPUT%
copy /y ..\CreateIndex\Script\avi2jpg.bat %OUTPUT%
copy /y ..\CreateIndex\Script\repeat.bat %OUTPUT%
copy /y ..\CreateIndex\Script\readme-index.txt %OUTPUT%
copy /y ..\CreateDb\bin\debug\CreateDb.exe %OUTPUT%

copy /y ..\ImageSearchEngine\bin\debug\ImageSearchEngine.dll %OUTPUT%
