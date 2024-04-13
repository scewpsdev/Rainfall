using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

internal class Program : Game
{
	public static new Program instance { get => (Program)Game.instance; }


	Level level;

	//Entity levelEntity;
	//Model levelMesh;
	//Model levelColliderMesh;
	//RigidBody levelCollider;

	//Dummy dummy;

	Player player;
	Camera camera;


	public override void init()
	{
		Renderer.Init(graphics);
		Physics.Init();

		Item.LoadContent();

		//RoomType.Init();

		level = new Level();

		level.addEntity(new SpawnRoom(graphics));

		//level.addEntity(new StartingRoom(graphics));

		level.addEntity(player = new Player());
		level.addEntity(camera = new PlayerCamera(player));

		//LevelGenerator.GenerateLevel(level, 0);
		//level.addEntity(camera = new Camera());
		//level.addEntity(player = new Player(camera), new Vector3(0.0f, 1.0f, 0.0f), Quaternion.Identity);
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

		level.update();

		Physics.Update();
	}

	public override void draw()
	{
		Renderer.Begin();
		Renderer.SetCamera(camera);

		level.draw(graphics);

		Renderer.End();

		Debug.DrawDebugText(Debug.debugTextSize.x - 16, 0, Time.fps + "fps");
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, 1, string.Format("{0:0.00}ms", Time.ms));
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, 2, string.Format("{0:0.00}MB", Time.memory / 1e6f));
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, 3, string.Format("{0:0.00}MB", Time.nativeMemory / 1e6f));
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, 4, string.Format("{0} allocations", Time.numAllocations));
	}

	public static void Main(string[] args)
	{
#if DEBUG
		string compilerDir = "bin\\x64\\Debug";
		string outDir = "bin\\x64\\Debug";
		string projectDir = "D:\\Dev\\Rainfall";
		//string projectDir = "C:\\Users\\faris\\Documents\\Dev\\Rainfall";
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
		startInfo.FileName = "cmd.exe";
		startInfo.Arguments = "/C " + projectDir + "\\" + compilerDir + "\\RainfallResourceCompiler.exe res " + projectDir + "\\" + outDir + "\\net7.0\\res gltf glb png hdr ogg dat glsl shader ttf";
		startInfo.WorkingDirectory = projectDir + "\\DDB";
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

		Game game = new Program();
		game.run(launchParams);
	}
}
