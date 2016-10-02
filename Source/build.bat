@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)
 
set version=1.0.0
if not "%PackageVersion%" == "" (
   set version=%PackageVersion%
)

set nuget=
if "%nuget%" == "" (
	set nuget=nuget
)

echo -- Build ArduinoLibCSharp...

%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Source/ArduinoLibCSharp.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=diag /nr:false

mkdir Build
mkdir Build\lib
mkdir Build\lib\net40

echo -- Packaging...

echo -- Package ArduinoUploader...
%nuget% pack "Source\ArduinoLibCSharp.ArduinoUploader\ArduinoLibCSharp.ArduinoUploader.nuspec" -NoPackageAnalysis -verbosity detailed -o Build -Version %version% -p Configuration="%config%"

echo -- Package IntelHexFormatReader...
%nuget% pack "Source\ArduinoLibCSharp.IntelHexFormatReader\ArduinoLibCSharp.IntelHexFormatReader.nuspec" -NoPackageAnalysis -verbosity detailed -o Build -Version %version% -p Configuration="%config%"
