
if exist bin\Release\net8.0\publish del /s /q bin\Release\net8.0\publish

dotnet publish -r win-x64 -c Release --self-contained -o bin/Release/net8.0/publish
del bin\Release\net8.0\publish\*.pdb

del res\asset_table

cmd /C ..\RainfallResourceCompiler\bin\x64\Debug\RainfallResourceCompiler.exe res bin\Release\net8.0\publish\assets png ogg vsh fsh csh ttf rfs gltf --preserve-scenegraph

xcopy /y ..\RainfallNative\bin\x64\ReleaseLight\RainfallNative.dll bin\Release\net8.0\publish\

ren bin\Release\net8.0\publish\PixelEngine.exe IvoryKeep.exe

mkdir bin\Release\net8.0\publish\saves

echo cmd /k IvoryKeep.exe > bin\Release\net8.0\publish\launch.bat

set version=0.2.0a

cd bin\Release\net8.0\publish
tar -a -cf ..\roguep-%version%.zip *
move ..\roguep-%version%.zip .

:: xcopy /y /e /q .\ ..\..\..\..\builds\%version%\
xcopy /y roguep-%version%.zip ..\..\..\..\builds\%version%\

butler push roguep-%version%.zip scewps/ivory-keep:%version%-windows --userversion=%version%

pause
