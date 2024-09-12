using Rainfall;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;

internal class Program : Game
{
	const int VERSION_MAJOR = 0;
	const int VERSION_MINOR = 1;
	const int VERSION_PATCH = 1;
	const char VERSION_SUFFIX = 'a';


	public static new Program instance { get => (Program)Game.instance; }
	static readonly string ASSEMBLY_NAME = Assembly.GetAssembly(typeof(Program))?.GetName().Name;


	bool debugStats = false;


	public override void init()
	{
		Display.windowTitle = ASSEMBLY_NAME;

		InputManager.LoadBindings();
		GraphicsManager.Init();

		Renderer.Init(Display.width, Display.height, graphics);
		Physics.Init();
		Audio.Init();
		AudioManager.Init();
	}

	public override void destroy()
	{
		Audio.Shutdown();
		Physics.Shutdown();
	}

	public override void update()
	{
		if (Input.IsKeyPressed(KeyCode.F11))
			Display.ToggleFullscreen();
		if (Input.IsKeyPressed(KeyCode.F10))
			debugStats = !debugStats;

		// update

		Physics.Update();
		AudioManager.Update();
	}

	public override void draw()
	{
		Renderer.Begin();

		GraphicsManager.Draw();

		// draw

		Renderer.End();

		if (debugStats)
			drawDebugStats();

		Audio.Update();
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

		y = DebugStats.Draw(0, y, 0xB, graphics);
	}

	static void CompileFolder(string folder, string outDir)
	{
		string projectDir = folder;
		string resCompilerDir = "D:\\Dev\\2023\\Rainfall\\RainfallResourceCompiler\\bin\\x64\\Debug";
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
		startInfo.FileName = "cmd.exe";
		startInfo.Arguments = "/C " + resCompilerDir + "\\RainfallResourceCompiler.exe res " + outDir + "\\res gltf fbx png hdr ogg dat shader ttf rfs";
		startInfo.WorkingDirectory = projectDir;
		process.StartInfo = startInfo;
		process.Start();
		process.WaitForExit();
	}

	public static void Main(string[] args)
	{
#if DEBUG
		CompileFolder("D:\\Dev\\2024\\" + ASSEMBLY_NAME + "\\" + ASSEMBLY_NAME, "D:\\Dev\\2024\\" + ASSEMBLY_NAME + "\\" + ASSEMBLY_NAME + "\\bin\\Debug\\net8.0");
		CompileFolder("D:\\Dev\\2023\\Rainfall\\Rainfall3D", "D:\\Dev\\2024\\" + ASSEMBLY_NAME + "\\" + ASSEMBLY_NAME + "\\bin\\Debug\\net8.0");
#endif

		LaunchParams launchParams = new LaunchParams(args);
#if DEBUG
		launchParams.width = 1600;
		launchParams.height = 900;
		launchParams.maximized = false;
#else
		launchParams.width = 1600;
		launchParams.height = 900;
		//launchParams.maximized = false;
		launchParams.fullscreen = true;
#endif

		Game game = new Program();
		game.run(launchParams);
	}
}
