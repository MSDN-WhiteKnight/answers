"%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe" tools\src\StackOverflowAnswers.sln
cd tools\src\StackOverflowAnswers\bin\Debug\
StackOverflowAnswers.exe generate
cd ..\..\..\..\..\
%DOCFX_PATH%\docfx.exe .\docfx.json
%DOCFX_PATH%\docfx.exe serve .\html
pause