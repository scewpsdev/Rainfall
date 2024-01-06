using Rainfall;
using System.Net.Security;
using System.Reflection;

internal class Gaem : Game
{
	const int VERSION_MAJOR = 0;
	const int VERSION_MINOR = 1;
	const int VERSION_PATCH = 1;
	const char VERSION_SUFFIX = 'a';


	public static new Gaem instance { get => (Gaem)Game.instance; }
	static readonly string ASSEMBLY_NAME = Assembly.GetAssembly(typeof(Gaem))?.GetName().Name;


	public GameManager manager;

	public Level level;
	Camera camera;
	public Player player;


	public override void init()
	{
		Display.windowTitle = ASSEMBLY_NAME;

		Renderer.Init(graphics);

		manager = new GameManager();

		level = new Level("res/levels/level1.png");
		manager.level = level;

		level.addEntity(camera = new Camera(16, 16 / Display.aspectRatio));
		level.addEntity(player = new Player(level.spawnPoint, camera));

		manager.player = player;

		manager.resetGameState();

		Input.mouseLocked = false;
	}

	public override void destroy()
	{
	}

	public override void update()
	{
		if (Input.IsKeyPressed(KeyCode.F9))
		{
			Debug.debugWireframeEnabled = !Debug.debugWireframeEnabled;
		}
		if (Input.IsKeyPressed(KeyCode.F10))
		{
			Debug.debugStatsEnabled = !Debug.debugStatsEnabled;
		}
		if (Input.IsKeyPressed(KeyCode.F11))
		{
			Display.ToggleFullscreen();
		}

		manager.update();
		level.update();
	}

	public override void draw()
	{
		Renderer.Begin();
		Renderer.SetCamera(camera.getProjectionMatrix(), camera.getViewMatrix());

		int x0 = (int)MathF.Floor(camera.left);
		int x1 = (int)MathF.Floor(camera.right);
		int y0 = (int)MathF.Floor(camera.bottom) - 3;
		int y1 = (int)MathF.Floor(camera.top);
		level.draw(x0, x1, y0, y1);

		manager.draw();

		Renderer.End();

#if DEBUG
		drawDebugStats();
#endif
	}

	void drawDebugStats()
	{
		int line = 0;

		Span<byte> str = stackalloc byte[64];

		StringUtils.WriteString(str, ASSEMBLY_NAME);
		StringUtils.AppendString(str, " Test Build ");
		StringUtils.AppendInteger(str, VERSION_MAJOR);
		StringUtils.AppendCharacter(str, '.');
		StringUtils.AppendInteger(str, VERSION_MINOR);
		StringUtils.AppendCharacter(str, '.');
		StringUtils.AppendInteger(str, VERSION_PATCH);
		StringUtils.AppendCharacter(str, VERSION_SUFFIX);
		Debug.DrawDebugText(Debug.debugTextSize.x - StringUtils.StringLength(str) - 1, line++, 0xB, str);

		line++;

		StringUtils.WriteInteger(str, Time.fps);
		StringUtils.AppendString(str, " fps");
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteFloat(str, Time.ms, 2);
		StringUtils.AppendString(str, " ms");
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		long mem = Time.memory;
		if (mem >= 1 << 20)
		{
			StringUtils.WriteFloat(str, mem / (float)(1 << 20), 2);
			StringUtils.AppendString(str, " MB");
		}
		else if (mem >= 1 << 10)
		{
			StringUtils.WriteFloat(str, mem / (float)(1 << 10), 2);
			StringUtils.AppendString(str, " KB");
		}
		else
		{
			StringUtils.WriteFloat(str, mem, 2);
			StringUtils.AppendString(str, " By");
		}
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		long nativeMem = Time.nativeMemory;
		if (nativeMem >= 1 << 20)
		{
			StringUtils.WriteFloat(str, nativeMem / (float)(1 << 20), 2);
			StringUtils.AppendString(str, " MB");
		}
		else if (nativeMem >= 1 << 10)
		{
			StringUtils.WriteFloat(str, nativeMem / (float)(1 << 10), 2);
			StringUtils.AppendString(str, " KB");
		}
		else
		{
			StringUtils.WriteFloat(str, nativeMem, 2);
			StringUtils.AppendString(str, " By");
		}
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteInteger(str, Time.numAllocations);
		StringUtils.AppendString(str, " allocations");
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		line++;

		StringUtils.WriteString(str, "x=");
		StringUtils.AppendFloat(str, player.position.x);
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteString(str, "y=");
		StringUtils.AppendFloat(str, player.position.y);
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);
	}

	public static void Main(string[] args)
	{
#if DEBUG
		string outDir = "bin\\x64\\Debug";
		string projectDir = "D:\\Dev\\Rainfall\\" + ASSEMBLY_NAME;
		string resCompilerDir = "D:\\Dev\\Rainfall\\RainfallResourceCompiler\\" + outDir;
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
		startInfo.FileName = "cmd.exe";
		startInfo.Arguments = "/C " + resCompilerDir + "\\RainfallResourceCompiler.exe res " + projectDir + "\\" + outDir + "\\net8.0\\res gltf fbx png hdr ogg dat shader ttf";
		startInfo.WorkingDirectory = projectDir;
		process.StartInfo = startInfo;
		process.Start();
		process.WaitForExit();
#endif

		LaunchParams launchParams = new LaunchParams(args);
		launchParams.width = 1600;
		launchParams.height = 900;
		launchParams.maximized = true;
		//launchParams.fullscreen = false;
		launchParams.fpsCap = 60;
		//launchParams.vsync = 1;

		Game game = new Gaem();
		game.run(launchParams);
	}
}
