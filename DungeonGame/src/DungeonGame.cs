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
	const int VERSION_MINOR = 4;
	const int VERSION_PATCH = 5;
	const char VERSION_SUFFIX = 'a';


	public static new DungeonGame instance { get => (DungeonGame)Game.instance; }


	public Level level { get; private set; }

	Camera camera;
	Player player;

	public GameManager gameManager;
	GameState gameState;

	float cpuTimeAcc = 0.0f;
	float gpuTimeAcc = 0.0f;
	float cpuTime, gpuTime;
	int numFrames = 0;
	long lastSecond = 0;


	public override void init()
	{
		InputManager.Init();
		GraphicsManager.Init();

		Renderer.Init(graphics);
		Physics.Init();
		AudioManager.Init();

		Item.LoadContent();

		//RoomType.Init();

		gameManager = new GameManager();
		gameState = new GameState();

		//level.addEntity(new StartingRoom(graphics));

		level = new Level();

		int seed = int.Parse(File.ReadAllText("seed.txt"));
		LevelGenerator levelGenerator = new LevelGenerator(seed, level);
		levelGenerator.generateLevel();

		level.addEntity(camera = new Camera());
		level.addEntity(player = new Player(camera, graphics), level.spawnPoint);
		player.resetPoint = level.spawnPoint;

		gameManager.level = level;
		gameManager.player = player;
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
		if (Input.IsKeyPressed(KeyCode.F8))
		{
			GraphicsManager.cinematicMode = !GraphicsManager.cinematicMode;
		}
		if (Input.IsKeyPressed(KeyCode.F11))
		{
			Display.ToggleFullscreen();
		}

#if DEBUG
		if (Input.IsKeyPressed(KeyCode.F9))
		{
			Debug.debugWireframeEnabled = !Debug.debugWireframeEnabled;
		}
		if (Input.IsKeyPressed(KeyCode.F10))
		{
			Debug.debugStatsEnabled = !Debug.debugStatsEnabled;
		}
#endif

		if (Input.IsKeyPressed(KeyCode.KeyP))
		{
			//player.setPosition(new Vector3(0.0f, 1.0f, 0.0f));
			player.setPosition(player.resetPoint.translation);
			player.setRotation(player.resetPoint.rotation);
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

		gameManager.update();

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

		gameManager.draw();

#if !DISTRIBUTION
		if (!GraphicsManager.cinematicMode)
			drawDebugStats();
#endif

		Renderer.End();


		if (Time.currentTime - lastSecond >= 1e9)
		{
			cpuTime = cpuTimeAcc / numFrames;
			gpuTime = gpuTimeAcc / numFrames;
			cpuTimeAcc = 0;
			gpuTimeAcc = 0;
			numFrames = 0;
			lastSecond = Time.currentTime;
		}
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
		Debug.DrawDebugText(0, line++, 0xB, str);

		line++;

		StringUtils.WriteInteger(str, Display.viewportSize.x);
		StringUtils.AppendString(str, "x");
		StringUtils.AppendInteger(str, Display.viewportSize.y);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteInteger(str, Time.fps);
		StringUtils.AppendString(str, " fps");
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteFloat(str, Time.ms, 2);
		StringUtils.AppendString(str, " ms");
		Debug.DrawDebugText(0, line++, str);

		long mem = Time.memory;
		str[0] = 0;
		WriteMemoryString(str, mem);
		Debug.DrawDebugText(0, line++, str);

		long nativeMem = Time.nativeMemory;
		str[0] = 0;
		WriteMemoryString(str, nativeMem);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteInteger(str, Time.numAllocations);
		StringUtils.AppendString(str, " allocations");
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "Meshes: ");
		StringUtils.AppendInteger(str, Renderer.meshRenderCounter);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "Culled: ");
		StringUtils.AppendInteger(str, Renderer.meshCulledCounter);
		Debug.DrawDebugText(0, line++, str);

		line++;

		graphics.getRenderStats(out RenderStats renderStats);
		cpuTimeAcc += renderStats.cpuTime;
		gpuTimeAcc += renderStats.gpuTime;
		numFrames++;

		StringUtils.WriteString(str, "CPU time: ");
		StringUtils.AppendFloat(str, cpuTime * 1000);
		StringUtils.AppendString(str, "ms");
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "GPU time: ");
		StringUtils.AppendFloat(str, gpuTime * 1000);
		StringUtils.AppendString(str, "ms");
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "GPU latency: ");
		StringUtils.AppendInteger(str, (int)renderStats.maxGpuLatency);
		StringUtils.AppendString(str, "ms");
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "GPU Memory: ");
		WriteMemoryString(str, renderStats.gpuMemoryUsed);
		StringUtils.AppendString(str, "/");
		WriteMemoryString(str, renderStats.gpuMemoryMax);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "Texture Memory: ");
		WriteMemoryString(str, renderStats.textureMemoryUsed);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "RT Memory: ");
		WriteMemoryString(str, renderStats.rtMemoryUsed);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "Draw calls: ");
		StringUtils.AppendInteger(str, (int)renderStats.numDraw);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "Computes: ");
		StringUtils.AppendInteger(str, (int)renderStats.numCompute);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "Blits: ");
		StringUtils.AppendInteger(str, (int)renderStats.numBlit);
		Debug.DrawDebugText(0, line++, str);

		line++;

		StringUtils.WriteString(str, "grounded=");
		StringUtils.AppendBool(str, player.isGrounded);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "speed=");
		StringUtils.AppendInteger(str, (int)(player.velocity.xz.length * 100));
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "x=");
		StringUtils.AppendInteger(str, (int)(player.position.x * 100));
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "y=");
		StringUtils.AppendInteger(str, (int)(player.position.y * 100));
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "z=");
		StringUtils.AppendInteger(str, (int)(player.position.z * 100));
		Debug.DrawDebugText(0, line++, str);
	}

	static void WriteMemoryString(Span<byte> str, long mem)
	{
		if (mem >= 1 << 30)
		{
			StringUtils.AppendFloat(str, mem / (float)(1 << 30), 2);
			StringUtils.AppendString(str, "GB");
		}
		else if (mem >= 1 << 20)
		{
			StringUtils.AppendFloat(str, mem / (float)(1 << 20), 2);
			StringUtils.AppendString(str, "MB");
		}
		else if (mem >= 1 << 10)
		{
			StringUtils.AppendFloat(str, mem / (float)(1 << 10), 2);
			StringUtils.AppendString(str, "KB");
		}
		else if (mem >= 0)
		{
			StringUtils.AppendInteger(str, mem);
			StringUtils.AppendString(str, "By");
		}
		else
		{
			StringUtils.AppendString(str, "0By");
		}
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
#if DEBUG
		launchParams.width = 1050;
		launchParams.height = 500;
#else
		launchParams.width = 1600;
		launchParams.height = 900;
		//launchParams.maximized = false;
		launchParams.fullscreen = true;
#endif

		Game game = new DungeonGame();
		game.run(launchParams);
	}
}
