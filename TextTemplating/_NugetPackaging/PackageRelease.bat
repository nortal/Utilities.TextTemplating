@ECHO OFF

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Msbuild.exe /verbosity:m /nologo /p:Configuration=Release ..\Nortal.Utilities.TextTemplating.csproj
pause
..\..\.nuget\nuget.exe pack -Properties Configuration=Release -Outputdirectory output Nortal.Utilities.TextTemplating.nuspec
pause