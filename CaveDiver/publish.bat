
if exist bin\Release\net8.0\publish del /s /q bin\Release\net8.0\publish
if exist bin\Release\net8.0\publish rmdir /s /q bin\Release\net8.0\publish
mkdir bin\Release\net8.0\publish

dotnet publish -r win-x64 -c Release --self-contained -o bin/Release/net8.0/publish
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

echo cmd /k CaveDiver.exe > bin\Release\net8.0\publish\launch.bat

set version=0.0.3a

cd bin\Release\net8.0\publish
tar -a -cf ..\cavediver-%version%.zip *
move ..\cavediver-%version%.zip .

xcopy /y cavediver-%version%.zip ..\..\..\..\builds\%version%\

pause
