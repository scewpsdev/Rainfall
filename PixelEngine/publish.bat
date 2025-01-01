if exist bin\Release\net8.0\publish del /s /q bin\Release\net8.0\publish

dotnet publish -r win-x64 -c Release --self-contained -o bin/Release/net8.0/publish
del bin\Release\net8.0\publish\*.pdb

del res\asset_table
del ..\RainfallNative\res\asset_table
del ..\Rainfall2D\res\asset_table

cmd /C ..\RainfallResourceCompiler\bin\x64\Debug\RainfallResourceCompiler.exe res bin\Release\net8.0\publish\assets png ogg vsh fsh csh ttf rfs
:: cmd /C ..\RainfallResourceCompiler\bin\x64\Debug\RainfallResourceCompiler.exe ..\RainfallNative\res bin\Release\net8.0\publish\assets png ogg vsh fsh csh ttf rfs
cmd /C ..\RainfallResourceCompiler\bin\x64\Debug\RainfallResourceCompiler.exe ..\Rainfall2D\res bin\Release\net8.0\publish\assets png ogg vsh fsh csh ttf rfs

xcopy /y ..\RainfallNative\bin\x64\ReleaseLight\RainfallNative.dll bin\Release\net8.0\publish\

ren bin\Release\net8.0\publish\PixelEngine.exe IvoryKeep.exe

echo cmd /k IvoryKeep.exe > bin\Release\net8.0\publish\launch.bat

pause
