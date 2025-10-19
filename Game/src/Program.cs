#if DEBUG
#define COMPILE_RESOURCES
#else
#define COMPILE_RESOURCES
#endif

using Rainfall;

public class Program : Game3D<Program>
{
	const int VERSION_MAJOR = 0;
	const int VERSION_MINOR = 0;
	const int VERSION_PATCH = 1;
	const char VERSION_SUFFIX = 'a';


	protected Program()
		: base(VERSION_MAJOR, VERSION_MINOR, VERSION_PATCH, VERSION_SUFFIX, false)
	{
	}

	public override void init()
	{
		base.init();

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
		game.compileResources("D:\\Dev\\Rainfall\\Game");
		//game.packageResources();
#endif

		game.run(launchParams);
	}
}