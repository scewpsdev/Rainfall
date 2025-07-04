﻿#if DEBUG
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
		game.compileResources("D:\\Dev\\Rainfall\\Soulslike");
		//game.packageResources();
#endif

		game.run(launchParams);
	}
}
