﻿using Rainfall;
using System.Net.Security;
using System.Reflection;


enum GameState
{
	Menu,
	Ingame,
}

internal class TopDownTest : Game
{
	const int VERSION_MAJOR = 0;
	const int VERSION_MINOR = 1;
	const int VERSION_PATCH = 1;
	const char VERSION_SUFFIX = 'a';


	public static new TopDownTest instance { get => (TopDownTest)Game.instance; }
	static readonly string ASSEMBLY_NAME = Assembly.GetAssembly(typeof(TopDownTest))?.GetName().Name;


	GameState state;

	Texture splashscreen;
	Texture button0, button1;
	int selectedButton = 0;

	public GameManager manager;

	public Level level;
	Camera camera;
	public Player player;

	Sound sfxSelect;


	public override void init()
	{
		Display.windowTitle = ASSEMBLY_NAME;

		Renderer.Init(graphics);

		//Display.fullscreen = true;
		Input.mouseLocked = false;

		//initMenu();
		initGameplay();
	}

	void initMenu()
	{
		state = GameState.Menu;

		splashscreen = Resource.GetTexture("res/sprites/splashscreen.png", false);
		button0 = Resource.GetTexture("res/sprites/button0.png", false);
		button1 = Resource.GetTexture("res/sprites/button1.png", false);

		sfxSelect = Resource.GetSound("res/sounds/select.ogg");
	}

	void initGameplay()
	{
		state = GameState.Ingame;

		Item.Init();

		manager = new GameManager();

		level = new Level("res/levels/level1.png");
		manager.level = level;

		level.addEntity(camera = new Camera(16, 16 / Display.aspectRatio));
		level.addEntity(player = new Player(level.spawnPoint, camera));

		manager.player = player;

		CollisionDetection.Init(level);

		//Audio.PlayBackgroundLooping(Resource.GetSound("res/sounds/ambience.ogg"), 1.0f, 1.0f);
		//Audio.SetEffect(AudioEffect.Reverb);

		manager.resetGameState();

		level.addEntity(new ItemPickup(Item.Get("staff"), level.spawnPoint + new Vector2(4, 0)));
	}

	public override void destroy()
	{
	}

	protected override void onViewportSizeEvent(int width, int height)
	{
		Renderer.Resize(width, height);
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
		if (Input.IsKeyPressed(KeyCode.Esc))
		{
			terminate();
		}

		if (state == GameState.Menu)
		{
			if (Input.IsKeyPressed(KeyCode.Down) || Input.IsKeyPressed(KeyCode.S))
			{
				selectedButton = Math.Min(selectedButton + 1, 1);
				Audio.PlayBackground(sfxSelect);
			}
			else if (Input.IsKeyPressed(KeyCode.Up) || Input.IsKeyPressed(KeyCode.W))
			{
				selectedButton = Math.Max(selectedButton - 1, 0);
				Audio.PlayBackground(sfxSelect);
			}

			if (Input.IsKeyPressed(KeyCode.E) || Input.IsKeyPressed(KeyCode.Return))
			{
				if (selectedButton == 0)
				{
					initGameplay();
				}
				else if (selectedButton == 1)
				{
					terminate();
				}
			}
		}
		else if (state == GameState.Ingame)
		{
			CollisionDetection.Update(level);

			manager.update();
			level.update();
		}
	}

	public override void draw()
	{
		Renderer.Begin();

		if (state == GameState.Menu)
		{
			Renderer.DrawUISprite(0, 0, Display.width, Display.height, splashscreen, 0, 0, 320, 180);

			Renderer.DrawUISprite(100, Display.height / 3 * 2, 256, 64, button0, 0, 0, 256, 64, selectedButton == 0 ? 0xFFFFFFFF : 0xFF777777);
			if (Input.IsHovered(100, Display.height / 3 * 2, 256, 64))
			{
				if (selectedButton != 0)
				{
					selectedButton = 0;
					Audio.PlayBackground(sfxSelect);
				}
				if (Input.IsMouseButtonPressed(MouseButton.Left))
				{
					state = GameState.Ingame;
					initGameplay();
				}
			}

			Renderer.DrawUISprite(100, Display.height / 3 * 2 + 82, 256, 64, button1, 0, 0, 256, 64, selectedButton == 1 ? 0xFFFFFFFF : 0xFF777777);
			if (Input.IsHovered(100, Display.height / 3 * 2 + 82, 256, 64))
			{
				if (selectedButton != 1)
				{
					selectedButton = 1;
					Audio.PlayBackground(sfxSelect);
				}
				if (Input.IsMouseButtonPressed(MouseButton.Left))
					terminate();
			}
		}
		else if (state == GameState.Ingame)
		{
			Renderer.SetCamera(camera.getProjectionMatrix(), camera.getViewMatrix(), camera.left, camera.right, camera.bottom, camera.top);

			int x0 = (int)MathF.Floor(camera.left);
			int x1 = (int)MathF.Floor(camera.right);
			int y0 = (int)MathF.Floor(camera.bottom) - 3;
			int y1 = (int)MathF.Floor(camera.top);
			level.draw(x0, x1, y0, y1);

			manager.draw();
		}

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
		graphics.drawDebugText(Debug.debugTextSize.x - StringUtils.StringLength(str) - 1, line++, 0xB, str);

		line++;

		StringUtils.WriteInteger(str, Time.fps);
		StringUtils.AppendString(str, " fps");
		graphics.drawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteFloat(str, Time.ms, 2);
		StringUtils.AppendString(str, " ms");
		graphics.drawDebugText(Debug.debugTextSize.x - 16, line++, str);

		long mem = Time.nativeMemory;
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
		graphics.drawDebugText(Debug.debugTextSize.x - 16, line++, str);

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
		graphics.drawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteInteger(str, Time.numAllocations);
		StringUtils.AppendString(str, " allocations");
		graphics.drawDebugText(Debug.debugTextSize.x - 16, line++, str);

		if (state == GameState.Ingame)
		{
			line++;

			StringUtils.WriteString(str, "x=");
			StringUtils.AppendFloat(str, player.position.x);
			graphics.drawDebugText(Debug.debugTextSize.x - 16, line++, str);

			StringUtils.WriteString(str, "y=");
			StringUtils.AppendFloat(str, player.position.y);
			graphics.drawDebugText(Debug.debugTextSize.x - 16, line++, str);
		}
	}

	public static void Main(string[] args)
	{
#if DEBUG
		string outDir = "bin\\x64\\Debug";
		string projectDir = "D:\\Dev\\2023\\Rainfall\\" + ASSEMBLY_NAME;
		string resCompilerDir = "D:\\Dev\\2023\\Rainfall\\RainfallResourceCompiler\\" + outDir;
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
		startInfo.FileName = "cmd.exe";
		startInfo.Arguments = "/C " + resCompilerDir + "\\RainfallResourceCompiler.exe res " + projectDir + "\\" + outDir + "\\net8.0\\res png ogg dat shader ttf";
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

		Game game = new TopDownTest();
		game.run(launchParams);
	}
}
