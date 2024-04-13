using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

internal class OntoNewHorizons : Game
{
	const int VERSION_MAJOR = 0;
	const int VERSION_MINOR = 1;
	const int VERSION_PATCH = 4;
	const char VERSION_SUFFIX = 'a';


	public static new OntoNewHorizons instance { get => (OntoNewHorizons)Game.instance; }


	public World world { get; private set; }

	Camera camera;
	Player player;

	GameState gameState;


	public override void init()
	{
		InputManager.Init();
		GraphicsManager.Init();

		Renderer.Init(graphics);
		Physics.Init();
		AudioManager.Init();

		Item.LoadContent();

		//RoomType.Init();

		gameState = new GameState();

		world = new World();
		WorldGenerator.Generate(world, 123);

		//LevelGenerator.GenerateLevel(level, 0);
		world.addEntity(camera = new Camera());
		world.addEntity(player = new Player(camera), new Vector3(50, world.getTerrainHeight(50, 50) + 1.0f, 50), Quaternion.Identity);
		player.resetPoint = new Vector3(-3.5f, 0.0f, -5.0f);
		//player.queueAction(new SpawnAction());

		AudioManager.SetReverb(false);
	}

	public override void destroy()
	{
		Physics.Shutdown();
	}

	public override void update()
	{
		if (Input.IsKeyPressed(KeyCode.F10))
		{
			Debug.debugStatsOverlayEnabled = !Debug.debugStatsOverlayEnabled;
		}
		if (Input.IsKeyPressed(KeyCode.F11))
		{
			Display.ToggleFullscreen();
		}

		if (Input.IsKeyPressed(KeyCode.KeyP))
		{
			//player.setPosition(new Vector3(0.0f, 1.0f, 0.0f));
			player.setPosition(player.resetPoint);
			player.setRotation(0.0f);
			player.pitch = 0.0f;
		}
		if (player.position.y < -100.0f)
		{
			player.velocity.y = 0.0f;
			player.setPosition(player.resetPoint);
			player.setRotation(0.0f);
			player.pitch = 0.0f;
		}

		world.update();

		Physics.Update();
		AudioManager.Update();
	}

	public override void draw()
	{
		Renderer.Begin();
		Renderer.SetCamera(camera);

		GraphicsManager.Draw();

		world.draw(graphics);

		Renderer.End();


#if !DISTRIBUTION
		int line = 0;

		Span<byte> str = stackalloc byte[64];

		StringUtils.WriteString(str, "Test Build 0.0.0a");
		str[11] = '0' + VERSION_MAJOR;
		str[13] = '0' + VERSION_MINOR;
		str[15] = '0' + VERSION_PATCH;
		str[16] = (byte)VERSION_SUFFIX;
		Debug.DrawDebugText(Debug.debugTextSize.x - 20, line++, 0xB, str);

		line++;

		StringUtils.WriteInteger(str, Time.fps);
		StringUtils.AppendString(str, " fps");
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteFloat(str, Time.ms, 2);
		StringUtils.AppendString(str, " ms");
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteFloat(str, Time.memory / 1e6f, 2);
		StringUtils.AppendString(str, " MB");
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteFloat(str, Time.nativeMemory / 1e6f, 2);
		StringUtils.AppendString(str, " MB");
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteInteger(str, Time.numAllocations);
		StringUtils.AppendString(str, " allocations");
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteInteger(str, Renderer.meshRenderCounter);
		StringUtils.AppendString(str, " meshes");
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteInteger(str, Renderer.meshCulledCounter);
		StringUtils.AppendString(str, " culled");
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		line++;

		StringUtils.WriteString(str, "grounded=");
		StringUtils.AppendBool(str, player.isGrounded);
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteString(str, "speed=");
		StringUtils.AppendInteger(str, (int)(player.velocity.xz.length * 100));
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteString(str, "x=");
		StringUtils.AppendInteger(str, (int)(player.position.x * 100));
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteString(str, "y=");
		StringUtils.AppendInteger(str, (int)(player.position.y * 100));
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteString(str, "z=");
		StringUtils.AppendInteger(str, (int)(player.position.z * 100));
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);
#endif
	}

	public static void Main(string[] args)
	{
#if DEBUG
		string outDir = "bin\\x64\\Debug";
		string projectDir = "D:\\Dev\\Rainfall\\OntoNewHorizons";
		string resCompilerDir = "D:\\Dev\\Rainfall\\" + outDir;
		//string projectDir = "C:\\Users\\faris\\Documents\\Dev\\Rainfall";
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
		startInfo.FileName = "cmd.exe";
		startInfo.Arguments = "/C " + resCompilerDir + "\\RainfallResourceCompiler.exe res " + projectDir + "\\" + outDir + "\\net7.0\\res gltf fbx png hdr ogg dat shader ttf";
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
		//launchParams.vsync = false;

		Game game = new OntoNewHorizons();
		game.run(launchParams);
	}
}
