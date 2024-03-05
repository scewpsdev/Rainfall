using Rainfall;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

internal class DungeonGame : Game
{
	const int VERSION_MAJOR = 0;
	const int VERSION_MINOR = 4;
	const int VERSION_PATCH = 5;
	const char VERSION_SUFFIX = 'a';


	public static new DungeonGame instance { get => (DungeonGame)Game.instance; }
	static readonly string ASSEMBLY_NAME = Assembly.GetAssembly(typeof(DungeonGame))?.GetName().Name;


	public Level level { get; private set; }

	Camera camera;

	public GameManager gameManager;
	GameState gameState;

	float cpuTimeAcc = 0.0f;
	float gpuTimeAcc = 0.0f;
	float physicsTimeAcc = 0.0f;
	float cpuTime, gpuTime, physicsTime;
	int numFrames = 0;
	long lastSecond = 0;


	public override void init()
	{
		Display.windowTitle = ASSEMBLY_NAME;

		InputManager.Init();
		GraphicsManager.Init();

		Renderer.Init(graphics);
		Physics.Init();
		Audio.Init();
		AudioManager.Init();

		Item.LoadContent();
		Tile.Init();

		//Debug.debugTextEnabled = true;

		FontManager.LoadFont("baskerville", "res/fonts/libre-baskerville.regular.ttf");

		gameManager = new GameManager();
		gameState = new GameState();

		level = new Level();

		camera = new Camera();

		gameManager.level = level;
		gameManager.camera = camera;

		gameManager.resetGameState();
	}

	public override void destroy()
	{
		Audio.Shutdown();
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
			gameManager.resetGameState();
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

		Renderer.End();

		if (!GraphicsManager.cinematicMode)
			drawDebugStats();

		//ImGui.ShowDemoWindow();
		//ImGui.ShowUserGuide();

		Audio.Update();


		if (Time.currentTime - lastSecond >= 1e9)
		{
			cpuTime = cpuTimeAcc / numFrames;
			gpuTime = gpuTimeAcc / numFrames;
			physicsTime = physicsTimeAcc / numFrames;
			cpuTimeAcc = 0;
			gpuTimeAcc = 0;
			physicsTimeAcc = 0;
			numFrames = 0;
			lastSecond = Time.currentTime;
		}
	}

	void drawDebugStats()
	{
		Span<byte> str = stackalloc byte[128];

		int y = 0;

		StringUtils.WriteString(str, "Test Build ");
		StringUtils.AppendInteger(str, VERSION_MAJOR);
		StringUtils.AppendCharacter(str, '.');
		StringUtils.AppendInteger(str, VERSION_MINOR);
		StringUtils.AppendCharacter(str, '.');
		StringUtils.AppendInteger(str, VERSION_PATCH);
		StringUtils.AppendCharacter(str, VERSION_SUFFIX);
		graphics.drawDebugText(0, y++, 0xB, str);

		y = DebugStats.Draw(0, y, graphics);

		y++;

		StringUtils.WriteString(str, "grounded=");
		StringUtils.AppendBool(str, gameManager.player.isGrounded);
		graphics.drawDebugText(0, y++, str);
		
		StringUtils.WriteString(str, "speed=");
		StringUtils.AppendInteger(str, (int)(gameManager.player.velocity.xz.length * 100));
		graphics.drawDebugText(0, y++, str);
		
		StringUtils.WriteString(str, "x=");
		StringUtils.AppendInteger(str, (int)(gameManager.player.position.x * 100));
		graphics.drawDebugText(0, y++, str);

		StringUtils.WriteString(str, "y=");
		StringUtils.AppendInteger(str, (int)(gameManager.player.position.y * 100));
		graphics.drawDebugText(0, y++, str);

		StringUtils.WriteString(str, "z=");
		StringUtils.AppendInteger(str, (int)(gameManager.player.position.z * 100));
		graphics.drawDebugText(0, y++, str);

		y++;
		y++;
		y++;

		/*
		const int numAllocators = 10;
		byte[] allocatorFiles = new byte[numAllocators * 128];
		long[] allocatorSizes = new long[numAllocators];
		Time.GetTopAllocators(numAllocators, allocatorFiles, allocatorSizes);
		
		for (int i = 0; i < numAllocators; i++)
		{
			Span<byte> file = MemoryExtensions.AsSpan(allocatorFiles, i * 128, 128);
			long size = allocatorSizes[i];
			StringUtils.WriteString(str, file, StringUtils.StringLength(file));
			StringUtils.AppendString(str, ": ");
			WriteMemoryString(str, size);
			Debug.DrawDebugText(0, line++, str);
		}
		*/
	}

	public static void Main(string[] args)
	{
#if DEBUG
		string outDir = "bin\\x64\\Debug";
		string projectDir = "D:\\Dev\\2023\\Rainfall\\DungeonGame";
		string resCompilerDir = "D:\\Dev\\2023\\Rainfall\\RainfallResourceCompiler\\" + outDir;
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
		launchParams.width = 1600;
		launchParams.height = 900;
		launchParams.maximized = true;
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
