set name=Reverse
set appversion=0.0.1a
set installPath=lib\Rainfall


if exist bin\Release\net8.0\publish del /s /q bin\Release\net8.0\publish
if exist bin\Release\net8.0\publish rmdir /s /q bin\Release\net8.0\publish
mkdir bin\Release\net8.0\publish

dotnet publish -r win-x64 -c Release --self-contained -o builds\%appversion% /p:DefineConstants=DISTRIBUTION_BUILD
del builds\%appversion%\*.pdb

del res\asset_table

cmd /C %installPath%\ResourceCompiler\RainfallResourceCompiler.exe res builds\%appversion%\assets png ogg vsh fsh csh ttf rfs gltf glb
cmd /C %installPath%\ResourceCompiler\RainfallResourceCompiler.exe builds\%appversion%\assets --package --compress
move builds\%appversion%\assets\dataa.dat builds\%appversion%
move builds\%appversion%\assets\datag.dat builds\%appversion%
move builds\%appversion%\assets\datam.dat builds\%appversion%
move builds\%appversion%\assets\datas.dat builds\%appversion%
move builds\%appversion%\assets\datat.dat builds\%appversion%

del /s /q builds\%appversion%\assets\*
rmdir /s /q builds\%appversion%\assets
mkdir builds\%appversion%\assets

move builds\%appversion%\dataa.dat builds\%appversion%\assets
move builds\%appversion%\datag.dat builds\%appversion%\assets
move builds\%appversion%\datam.dat builds\%appversion%\assets
move builds\%appversion%\datas.dat builds\%appversion%\assets
move builds\%appversion%\datat.dat builds\%appversion%\assets

xcopy /y %installPath%\Release\RainfallNative.dll builds\%appversion%

echo cmd /k %name%.exe > builds\%appversion%\launch.bat

cd builds\%appversion%
tar -a -cf ..\%name%-%appversion%.zip *

pause
