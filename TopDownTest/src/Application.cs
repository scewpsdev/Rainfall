using Rainfall;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;


internal class Application : Game
{
	const int VERSION_MAJOR = 0;
	const int VERSION_MINOR = 0;
	const int VERSION_PATCH = 1;
	const char VERSION_SUFFIX = 'a';


	public static new Application instance { get => (Application)Game.instance; }


	public Level level;
	Player player;
	Camera camera;

	Font font, smallFont;


	public override void init()
	{
		Renderer.Init(graphics);

		Tile.Init();

		LevelGenerator generator = new LevelGenerator(32, 32, 123456);
		level = generator.run();

		level.addEntity(camera = new Camera());
		level.addEntity(player = new Player(camera));

		font = Resource.GetFontData("res/fonts/dpcomic.ttf").createFont(14, false);
		smallFont = Resource.GetFontData("res/fonts/PublicPixel-z84yD.ttf").createFont(8, false);
	}

	public override void destroy()
	{
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
	}

	public override void draw()
	{
		Renderer.Begin(camera);

		//Renderer.DrawHorizontalSprite(new Vector3(0.0f), new Vector2(5.0f), new Vector4(0.4f, 0.4f, 1.0f, 1.0f));
		//Renderer.DrawVerticalSprite(new Vector3(0.0f), new Vector2(5.0f), new Vector4(1.0f, 0.4f, 0.4f, 1.0f));

		level.draw(graphics);

		int playerX = (int)(camera.position.x / LevelGenerator.TILE_SIZE);
		int playerZ = (int)(camera.position.z / LevelGenerator.TILE_SIZE);
		for (int y = 0; y < level.height; y++)
		{
			for (int x = 0; x < level.width; x++)
			{
				int scale = 4;
				int xx = Display.viewportSize.x - level.width - 300 + x * scale;
				int yy = 50 + y * scale;
				int height = level.heightmap[x + y * level.width];
				uint tile = level.tileTypes[x + y * level.width];
				if (tile == LevelGenerator.CID_CORRIDOR)
					tile = tile - 0x00A0A0A0 + (uint)(height - 1) * 0x00505050;
				uint color = x == playerX && y == playerZ ? 0xFF00FF00 : tile;
				Renderer.DrawUIRect(xx, yy, scale, scale, color);
			}
		}

		//Renderer.DrawUIRect(100, 100, 100, 100, 0xFFFF0000);
		Renderer.DrawText(10, 10, 5, "abcABC", font, 0xFFFF77FF);

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

		/*
		StringUtils.WriteInteger(str, Renderer.meshRenderCounter);
		StringUtils.AppendString(str, " meshes");
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteInteger(str, Renderer.meshCulledCounter);
		StringUtils.AppendString(str, " culled");
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);
		*/

		line++;

		/*
		StringUtils.WriteString(str, "grounded=");
		StringUtils.AppendBool(str, player.isGrounded);
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteString(str, "speed=");
		StringUtils.AppendInteger(str, (int)(player.velocity.xz.length * 100));
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);
		*/

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
		string projectDir = "D:\\Dev\\Rainfall\\TopDownTest";
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

		Game game = new Application();
		game.run(launchParams);
	}
}
