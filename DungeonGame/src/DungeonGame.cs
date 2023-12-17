using Rainfall;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

internal class DungeonGame : Game
{
	const int VERSION_MAJOR = 0;
	const int VERSION_MINOR = 1;
	const int VERSION_PATCH = 4;
	const char VERSION_SUFFIX = 'a';


	public static new DungeonGame instance { get => (DungeonGame)Game.instance; }


	public Level level { get; private set; }

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

		//level.addEntity(new StartingRoom(graphics));

		level = new Level();

		int floor = 0;
		int seed = 123456 * (floor + 1);
		LevelGenerator levelGenerator = new LevelGenerator(seed, level);
		levelGenerator.generateLevel();

		level.addEntity(camera = new Camera());
		level.addEntity(player = new Player(camera, graphics), level.spawnPoint, Quaternion.Identity);
		player.resetPoint = level.spawnPoint;
		//level.addEntity(player = new Player(camera), new Vector3(0, -37, 54), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI));
		//player.queueAction(new SpawnAction());

		//level.addEntity(new Bob(), new Vector3(-2.0f, 0.0f, 0.0f), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI));
		//level.addEntity(new Bob(), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI));
		//level.addEntity(new Bob(), new Vector3(2.0f, 0.0f, 0.0f), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI));

		//level.addEntity(new SkeletonEnemy(), new Vector3(-2.0f, 0.0f, 15.0f), Quaternion.Identity);
		//level.addEntity(new SkeletonEnemy(), new Vector3(0.0f, 0.0f, 15.0f), Quaternion.Identity);
		//level.addEntity(new SkeletonEnemy(), new Vector3(2.0f, 0.0f, 15.0f), Quaternion.Identity);

		//AudioManager.SetReverb(true);
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
		if (Input.IsKeyPressed(KeyCode.F9))
		{
			GraphicsManager.cinematicMode = !GraphicsManager.cinematicMode;
		}

		if (Input.IsKeyPressed(KeyCode.KeyP))
		{
			//player.setPosition(new Vector3(0.0f, 1.0f, 0.0f));
			player.setPosition(player.resetPoint);
			player.setRotation(0.0f);
			player.pitch = 0.0f;
		}
		/*
		if (player.position.y < -100.0f)
		{
			player.velocity.y = 0.0f;
			player.setPosition(player.resetPoint);
			player.setRotation(0.0f);
			player.pitch = 0.0f;
		}
		*/

		level.update();

		Physics.Update();
		AudioManager.Update();
	}

	public override void draw()
	{
		Renderer.Begin();
		Renderer.SetCamera(camera);

		GraphicsManager.Draw();

		level.draw(graphics);

#if !DISTRIBUTION
		if (!GraphicsManager.cinematicMode)
			drawDebugStats();
#endif

		Renderer.End();
	}

	void drawDebugStats()
	{
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
	}

	public static void Main(string[] args)
	{
#if DEBUG
		string outDir = "bin\\x64\\Debug";
		string projectDir = "D:\\Dev\\Rainfall\\DungeonGame";
		string resCompilerDir = "D:\\Dev\\Rainfall\\RainfallResourceCompiler\\" + outDir;
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

		Game game = new DungeonGame();
		game.run(launchParams);
	}
}
