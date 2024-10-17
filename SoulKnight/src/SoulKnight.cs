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


public class SoulKnight : Game
{
	const int VERSION_MAJOR = 0;
	const int VERSION_MINOR = 1;
	const int VERSION_PATCH = 8;
	const char VERSION_SUFFIX = 'a';


	public static new SoulKnight instance { get => (SoulKnight)Game.instance; }
	static readonly string ASSEMBLY_NAME = Assembly.GetAssembly(typeof(SoulKnight))?.GetName().Name;


	const int idealScale = 4;
	public int scale;
	public int width, height;

	public bool debugStats = false;

	Stack<State> stateMachine = new Stack<State>();


	public override void init()
	{
		Display.windowTitle = ASSEMBLY_NAME;

		// pixel perfect correction
		scale = (int)MathF.Round(Display.width / 1920.0f * idealScale);
		width = (int)MathF.Ceiling(Display.width / (float)scale);
		height = (int)MathF.Ceiling(Display.height / (float)scale);

		Renderer.Init(graphics, width, height);

		Physics.Init();
		Audio.Init();

		Item.InitTypes();
		EntityType.InitTypes();

		Settings.Load();

		pushState(new MainMenuState());
	}

	public override void destroy()
	{
		while (stateMachine.Count > 0)
			stateMachine.Pop().destroy();

		Audio.Shutdown();
		Physics.Shutdown();
	}

	protected override void onViewportSizeEvent(int newWidth, int newHeight)
	{
		// pixel perfect correction
		scale = (int)MathF.Round(newWidth / 1920.0f * idealScale);
		width = (int)MathF.Ceiling(newWidth / (float)scale);
		height = (int)MathF.Ceiling(newHeight / (float)scale);

		Renderer.Resize(width, height);
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
		startInfo.Arguments = "/C " + resCompilerDir + "\\RainfallResourceCompiler.exe res " + outDir + "\\res png ogg vsh fsh csh ttf rfs";
		startInfo.WorkingDirectory = projectDir;
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

		CompileFolder("D:\\Dev\\Rainfall\\" + ASSEMBLY_NAME, "D:\\Dev\\Rainfall\\" + ASSEMBLY_NAME + "\\bin\\" + config + "\\net8.0");
		CompileFolder("D:\\Dev\\Rainfall\\RainfallNative", "D:\\Dev\\Rainfall\\" + ASSEMBLY_NAME + "\\bin\\" + config + "\\net8.0");
		CompileFolder("D:\\Dev\\Rainfall\\Rainfall2D", "D:\\Dev\\Rainfall\\" + ASSEMBLY_NAME + "\\bin\\" + config + "\\net8.0");

		Utils.RunCommand("xcopy", "/y \"D:\\Dev\\Rainfall\\RainfallNative\\bin\\x64\\" + config + "\\RainfallNative.dll\" \"D:\\Dev\\Rainfall\\" + ASSEMBLY_NAME + "\\bin\\" + config + "\\net8.0\\\"");
		//Utils.RunCommand("xcopy", "/y \"D:\\Dev\\Rainfall\\RainfallNative\\lib\\lib\\nvcloth\\" + config + "\\NvCloth.dll\" \"D:\\Dev\\Rainfall\\" + ASSEMBLY_NAME + "\\bin\\" + config + "\\net8.0\\\"");
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

		Game game = new SoulKnight();
		game.run(launchParams);
	}
}
