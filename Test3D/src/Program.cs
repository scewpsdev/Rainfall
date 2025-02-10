#if DEBUG
#define COMPILE_RESOURCES
#else
//#define COMPILE_RESOURCES
#endif


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
using static System.Formats.Asn1.AsnWriter;


public class Program : Game
{
	const int VERSION_MAJOR = 0;
	const int VERSION_MINOR = 0;
	const int VERSION_PATCH = 1;
	const char VERSION_SUFFIX = 'a';


	public static new Program instance { get => (Program)Game.instance; }
	static readonly string ASSEMBLY_NAME = Assembly.GetAssembly(typeof(Program))?.GetName().Name;


	public bool debugStats = false;

	Stack<State> stateMachine = new Stack<State>();


	public override void init()
	{
		Resource.LoadPackageHeader("datas.dat");
		Resource.LoadPackageHeader("datat.dat");
		Resource.LoadPackageHeader("datag.dat");
		Resource.LoadPackageHeader("dataa.dat");
		Resource.LoadPackageHeader("datam.dat");

		Renderer.Init(Display.width, Display.height, graphics);

		Physics.Init();
		Audio.Init();
		AudioManager.Init();

		pushState(new GameState());
	}

	public override void destroy()
	{
		while (stateMachine.Count > 0)
			stateMachine.Pop().destroy();

		Audio.Shutdown();
		Physics.Shutdown();
	}

	public void pushState(State state)
	{
		stateMachine.Push(state);
		state.game = this;
		state.init();
	}

	public void popState()
	{
		stateMachine.Pop().destroy();
		if (stateMachine.Count == 0)
			terminate();
	}

	protected override void onViewportSizeEvent(int width, int height)
	{
		Renderer.Resize(width, height);
	}

	protected override void onKeyEvent(KeyCode key, KeyModifier modifiers, bool down)
	{
		if (stateMachine.TryPeek(out State state))
			state.onKeyEvent(key, modifiers, down);
	}

	protected override void onMouseButtonEvent(MouseButton button, bool down)
	{
		if (stateMachine.TryPeek(out State state))
			state.onMouseButtonEvent(button, down);
	}

	protected override void onCharEvent(byte length, uint value)
	{
		if (stateMachine.TryPeek(out State state))
			state.onCharEvent(length, value);
	}

	public override unsafe void update()
	{
		for (int i = 0; i < 15; i++)
		{
			if (Input.gamepadCurrent.buttons[i] != Input.gamepadLast.buttons[i])
			{
				if (stateMachine.TryPeek(out State _state))
					_state.onGamepadButtonEvent((GamepadButton)i, Input.gamepadCurrent.buttons[i] != 0);
			}
		}

		if (Input.IsKeyPressed(KeyCode.F11) || ImGui.IsKeyPressed(KeyCode.F11, false))
			Display.ToggleFullscreen();

#if DEBUG
		if (Input.IsKeyPressed(KeyCode.F10) || ImGui.IsKeyPressed(KeyCode.F10, false))
			debugStats = !debugStats;
#endif

		Audio.Update();
		AudioManager.Update();

		if (stateMachine.TryPeek(out State state))
			state.update();

		Physics.Update();
	}

	public override void draw()
	{
		Renderer.Begin();

		GraphicsManager.Draw();

		if (stateMachine.TryPeek(out State state))
			state.draw(graphics);

		Renderer.End();

		if (debugStats)
			drawDebugStats();
		else
			drawVersion();
	}

	void drawVersion()
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

		graphics.drawDebugText(0, y++, 0x1F, str);
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
#if DEBUG
		StringUtils.AppendString(str, " Debug");
#endif
		graphics.drawDebugText(0, y++, 0x1F, str);

		y = graphics.drawDebugInfo(0, y, 0x1F);

		y++;

		if (stateMachine.TryPeek(out State state))
			state.drawDebugStats(y, 0x1F, graphics);
	}

	static void CompileFolder(string folder, string outDir)
	{
		string projectDir = folder;
		string resCompilerDir = "D:\\Dev\\Rainfall\\RainfallResourceCompiler\\bin\\x64\\Debug";
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
		startInfo.FileName = "cmd.exe";
		startInfo.Arguments = $"/C {resCompilerDir}\\RainfallResourceCompiler.exe res {outDir} png ogg vsh fsh csh ttf rfs gltf --preserve-scenegraph";
		startInfo.WorkingDirectory = projectDir;
		process.StartInfo = startInfo;
		process.Start();
		process.WaitForExit();
	}

	static void PackageFolder(string dir)
	{
		string resCompilerDir = "D:\\Dev\\Rainfall\\RainfallResourceCompiler\\bin\\x64\\Debug";
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
		startInfo.FileName = "cmd.exe";
		startInfo.Arguments = "/C " + resCompilerDir + "\\RainfallResourceCompiler.exe --package --compress " + dir;
		process.StartInfo = startInfo;
		process.Start();
		process.WaitForExit();
	}

	public static void Main(string[] args)
	{
#if COMPILE_RESOURCES
#if DEBUG
		string config = "Debug";
#else
		string config = "Release";
#endif

		CompileFolder($"D:\\Dev\\Rainfall\\{ASSEMBLY_NAME}", $"D:\\Dev\\Rainfall\\{ASSEMBLY_NAME}\\bin\\{config}\\net8.0\\res");
		CompileFolder("D:\\Dev\\Rainfall\\RainfallNative", $"D:\\Dev\\Rainfall\\{ASSEMBLY_NAME}\\bin\\{config}\\net8.0\\res");
		PackageFolder($"D:\\Dev\\Rainfall\\{ASSEMBLY_NAME}\\bin\\{config}\\net8.0\\res");

		Utils.RunCommand("xcopy", "/y \"D:\\Dev\\Rainfall\\RainfallNative\\bin\\x64\\" + config + "\\RainfallNative.dll\" \"D:\\Dev\\Rainfall\\" + ASSEMBLY_NAME + "\\bin\\" + config + "\\net8.0\\\"");
#endif

		LaunchParams launchParams = new LaunchParams(args);
		launchParams.fpsCap = 120;
#if DEBUG
		launchParams.width = 1600; //1280;
		launchParams.height = 900; //720;
		launchParams.maximized = true;
#else
		launchParams.width = 1280;
		launchParams.height = 720;
		//launchParams.maximized = false;
		launchParams.fullscreen = true;
#endif

		Game game = new Program();
		game.run(launchParams);
	}
}
