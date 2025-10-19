#if DEBUG
#define COMPILE_RESOURCES
#else
#define COMPILE_RESOURCES
#endif


using Rainfall;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;

internal class Program : Game3D<Program>
{
	const string PROJECT_PATH = "D:\\Dev\\Rainfall\\Soulslike"; // TODO fill in

	const int VERSION_MAJOR = 0;
	const int VERSION_MINOR = 0;
	const int VERSION_PATCH = 1;
	const char VERSION_SUFFIX = 'a';


	public Program()
		: base(VERSION_MAJOR, VERSION_MINOR, VERSION_PATCH, VERSION_SUFFIX, false)
	{
	}

	public override void init()
	{
		base.init();

		//GraphicsManager.bloomStrength = 0.05f;
		//GraphicsManager.bloomEnabled = false;
		//GraphicsManager.ssaoEnabled = false;
		//GraphicsManager.exposure = 8;

		FontManager.LoadFont("default", "font/libre-baskerville.regular.ttf");

		Item.Init();
		DungeonGenerator.Init();

		pushState(new GameState());
	}

	public static void Main(string[] args)
	{
		LaunchParams launchParams = new LaunchParams(args);
		launchParams.fpsCap = 60;
#if DEBUG
		launchParams.width = 800;
		launchParams.height = 600;
		//launchParams.maximized = true;
#else
		launchParams.width = 1280;
		launchParams.height = 720;
		//launchParams.maximized = false;
		launchParams.fullscreen = true;
#endif

		Program game = new Program();

#if COMPILE_RESOURCES
#if DEBUG
		string config = "Debug";
#else
		string config = "Release";
#endif

		Utils.RunCommand("xcopy", $"/y \"D:\\Dev\\Rainfall\\RainfallNative\\bin\\x64\\{config}\\RainfallNative.dll\" \"{PROJECT_PATH}\\bin\\{config}\\net8.0\"");
		//int exitCode = game.compileResources(PROJECT_PATH, PROJECT_PATH + $"\\bin\\{config}\\net8.0\\", "lib\\Rainfall\\ResourceCompiler\\RainfallResourceCompiler.exe");
		int exitCode = game.compileResources(PROJECT_PATH, PROJECT_PATH + $"\\bin\\{config}\\net8.0\\", "D:\\Dev\\Rainfall\\RainfallResourceCompiler\\bin\\x64\\Debug\\RainfallResourceCompiler.exe");
		if (exitCode != 0)
			Debug.Error("Resource compilation exited with code " + exitCode);
		game.compileResources("D:\\Dev\\Rainfall\\RainfallNative", PROJECT_PATH + $"\\bin\\{config}\\net8.0\\", "D:\\Dev\\Rainfall\\RainfallResourceCompiler\\bin\\x64\\Debug\\RainfallResourceCompiler.exe");
		//game.packageResources();
#endif

		game.run(launchParams);
	}
}
