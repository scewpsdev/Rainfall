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


public class PixelEngine : Game
{
	const int VERSION_MAJOR = 0;
	const int VERSION_MINOR = 1;
	const int VERSION_PATCH = 4;
	const char VERSION_SUFFIX = 'a';


	public static new PixelEngine instance { get => (PixelEngine)Game.instance; }
	static readonly string ASSEMBLY_NAME = Assembly.GetAssembly(typeof(PixelEngine))?.GetName().Name;


	public bool debugStats = false;

	Stack<State> stateMachine = new Stack<State>();


	public override void init()
	{
		Display.windowTitle = ASSEMBLY_NAME;

		float w = 1920 / 5.0f / 16.0f;
		int scale = (int)MathF.Round(Display.width / w / 16.0f);
		scale = 1;
		Renderer.Init(graphics, Display.width / scale, Display.height / scale);

		Physics.Init();
		Audio.Init();

		Item.InitTypes();

		InputManager.Init();

		pushState(new MainMenuState());
	}

	public override void destroy()
	{
		while (stateMachine.Count > 0)
			stateMachine.Pop().destroy();

		Audio.Shutdown();
		Physics.Shutdown();
	}

	protected override void onViewportSizeEvent(int width, int height)
	{
		float w = 1920 / 5.0f / 16.0f;
		int scale = (int)MathF.Round(Display.width / w / 16.0f);
		scale = 1;
		Renderer.Resize(width / scale, height / scale);
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

	protected override void onKeyEvent(KeyCode key, KeyModifier modifiers, bool down)
	{
		if (stateMachine.TryPeek(out State state))
			state.onKeyEvent(key, modifiers, down);
	}

	protected override void onCharEvent(byte length, uint value)
	{
		if (stateMachine.TryPeek(out State state))
			state.onCharEvent(length, value);
	}

	public override void update()
	{
		if (Input.IsKeyPressed(KeyCode.F11) || ImGui.IsKeyPressed(KeyCode.F11, false))
			Display.ToggleFullscreen();
		if (Input.IsKeyPressed(KeyCode.F10) || ImGui.IsKeyPressed(KeyCode.F10, false))
			debugStats = !debugStats;

		if (stateMachine.TryPeek(out State state))
			state.update();

		Physics.Update();
	}

	public override void draw()
	{
		Renderer.Begin();

		if (stateMachine.TryPeek(out State state))
			state.draw(graphics);

		Renderer.End();

		if (debugStats)
			drawDebugStats();
		else
			drawVersion();

		Audio.Update();
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
		startInfo.Arguments = "/C " + resCompilerDir + "\\RainfallResourceCompiler.exe res " + outDir + "\\res gltf fbx png hdr ogg dat vsh fsh csh ttf rfs";
		startInfo.WorkingDirectory = projectDir;
		process.StartInfo = startInfo;
		process.Start();
		process.WaitForExit();
	}

	static void RunCommand(string file, string args)
	{
		System.Diagnostics.Process process = System.Diagnostics.Process.Start(file, args);
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

		CompileFolder("D:\\Dev\\Rainfall\\" + ASSEMBLY_NAME, "D:\\Dev\\Rainfall\\" + ASSEMBLY_NAME + "\\bin\\" + config + "\\net8.0");
		CompileFolder("D:\\Dev\\Rainfall\\RainfallNative", "D:\\Dev\\Rainfall\\" + ASSEMBLY_NAME + "\\bin\\" + config + "\\net8.0");
		CompileFolder("D:\\Dev\\Rainfall\\Rainfall2D", "D:\\Dev\\Rainfall\\" + ASSEMBLY_NAME + "\\bin\\" + config + "\\net8.0");

		RunCommand("xcopy", "/y \"D:\\Dev\\Rainfall\\RainfallNative\\bin\\x64\\" + config + "\\RainfallNative.dll\" \"D:\\Dev\\Rainfall\\" + ASSEMBLY_NAME + "\\bin\\" + config + "\\net8.0\\\"");
		RunCommand("xcopy", "/y \"D:\\Dev\\Rainfall\\RainfallNative\\lib\\lib\\nvcloth\\" + config + "\\NvCloth.dll\" \"D:\\Dev\\Rainfall\\" + ASSEMBLY_NAME + "\\bin\\" + config + "\\net8.0\\\"");
#endif

		LaunchParams launchParams = new LaunchParams(args);
		launchParams.fpsCap = 120;
#if DEBUG
		launchParams.width = 1280;
		launchParams.height = 720;
		launchParams.maximized = false;
#else
		launchParams.width = 1600;
		launchParams.height = 900;
		//launchParams.maximized = false;
		launchParams.fullscreen = true;
#endif

		Game game = new PixelEngine();
		game.run(launchParams);
	}
}
