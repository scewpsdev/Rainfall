set appversion=0.2.4a


if exist bin\Release\net8.0\publish del /s /q bin\Release\net8.0\publish
if exist bin\Release\net8.0\publish rmdir /s /q bin\Release\net8.0\publish
mkdir bin\Release\net8.0\publish

dotnet publish -r win-x64 -c Release --self-contained -o bin/Release/net8.0/publish /p:DefineConstants=DISTRIBUTION
del bin\Release\net8.0\publish\*.pdb

del res\asset_table

cmd /C ..\RainfallResourceCompiler\bin\x64\Debug\RainfallResourceCompiler.exe res bin\Release\net8.0\publish\assets png ogg vsh fsh csh ttf rfs gltf --preserve-scenegraph
cmd /C ..\RainfallResourceCompiler\bin\x64\Debug\RainfallResourceCompiler.exe bin\Release\net8.0\publish\assets --package --compress
move bin\Release\net8.0\publish\assets\dataa.dat bin\Release\net8.0\publish
move bin\Release\net8.0\publish\assets\datag.dat bin\Release\net8.0\publish
move bin\Release\net8.0\publish\assets\datam.dat bin\Release\net8.0\publish
move bin\Release\net8.0\publish\assets\datas.dat bin\Release\net8.0\publish
move bin\Release\net8.0\publish\assets\datat.dat bin\Release\net8.0\publish

del /s /q bin\Release\net8.0\publish\assets\*
rmdir /s /q bin\Release\net8.0\publish\assets
mkdir bin\Release\net8.0\publish\assets

move bin\Release\net8.0\publish\dataa.dat bin\Release\net8.0\publish\assets
move bin\Release\net8.0\publish\datag.dat bin\Release\net8.0\publish\assets
move bin\Release\net8.0\publish\datam.dat bin\Release\net8.0\publish\assets
move bin\Release\net8.0\publish\datas.dat bin\Release\net8.0\publish\assets
move bin\Release\net8.0\publish\datat.dat bin\Release\net8.0\publish\assets

xcopy /y ..\RainfallNative\bin\x64\ReleaseLight\RainfallNative.dll bin\Release\net8.0\publish\

mkdir bin\Release\net8.0\publish\saves

echo cmd /k IvoryKeep.exe > bin\Release\net8.0\publish\launch.bat


cd bin\Release\net8.0\publish
tar -a -cf ..\roguep-%appversion%.zip *
move ..\roguep-%appversion%.zip .

:: xcopy /y /e /q .\ ..\..\..\..\builds\%appversion%\
xcopy /y roguep-%appversion%.zip ..\..\..\..\builds\%appversion%\

butler push roguep-%appversion%.zip scewps/ivory-keep:%appversion%-windows --userversion=%version%

pause
