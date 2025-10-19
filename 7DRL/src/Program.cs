#if DEBUG
#define COMPILE_RESOURCES
#else
//#define COMPILE_RESOURCES
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


public class Program : GamePixel<Program>
{
	public const int VERSION_MAJOR = 0;
	public const int VERSION_MINOR = 0;
	public const int VERSION_PATCH = 1;
	public const char VERSION_SUFFIX = 'a';


	const int idealScale = 3;


	public Program()
		: base(VERSION_MAJOR, VERSION_MINOR, VERSION_PATCH, VERSION_SUFFIX, false, idealScale)
	{
	}

	public override void init()
	{
		base.init();

		Renderer.bloomEnabled = false;
		Renderer.vignetteEnabled = false;

		pushState(new GameState());
	}

	public static void Main(string[] args)
	{
		LaunchParams launchParams = new LaunchParams(args);
		launchParams.fpsCap = 60;
#if DEBUG
		launchParams.width = 1280;
		launchParams.height = 720;
		launchParams.maximized = true;
#else
		launchParams.width = 1280;
		launchParams.height = 720;
		//launchParams.maximized = false;
		launchParams.fullscreen = true;
#endif

		Program game = new Program();

#if COMPILE_RESOURCES
		game.compileResources();
		//game.packageResources();
#endif

		game.run(launchParams);
	}
}
