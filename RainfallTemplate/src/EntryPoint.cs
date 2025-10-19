using Rainfall;


public class EntryPoint : Game3D<EntryPoint>
{
	const string PROJECT_PATH = "C:\\RainfallTemplate"; // TODO fill in

	const int VERSION_MAJOR = 0;
	const int VERSION_MINOR = 0;
	const int VERSION_PATCH = 1;
	const char VERSION_SUFFIX = 'a';

#if DISTRIBUTION_BUILD
	const bool LOAD_PACKAGES = true;
#else
	const bool LOAD_PACKAGES = false;
#endif


	public EntryPoint()
		: base(VERSION_MAJOR, VERSION_MINOR, VERSION_PATCH, VERSION_SUFFIX, LOAD_PACKAGES)
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
#if DISTRIBUTION_BUILD
		launchParams.width = 1280;
		launchParams.height = 720;
		launchParams.fullscreen = true;
#else
		launchParams.width = 800;
		launchParams.height = 600;
#endif

		EntryPoint game = new EntryPoint();

#if !DISTRIBUTION_BUILD
#if DEBUG
		string config = "Debug";
#else
		string config = "Release";
#endif
		Utils.RunCommand("xcopy", $"/y \"{PROJECT_PATH}\\lib\\Rainfall\\{config}\\RainfallNative.dll\" \"{PROJECT_PATH}\\bin\\{config}\\net8.0\"");
		int exitCode = game.compileResources(PROJECT_PATH, PROJECT_PATH + $"\\bin\\{config}\\net8.0\\", "lib\\Rainfall\\ResourceCompiler\\RainfallResourceCompiler.exe");
		if (exitCode != 0)
			Debug.Error("Resource compilation exited with code " + exitCode);
#endif

		game.run(launchParams);
	}
}
