﻿using Rainfall;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;

public class Game3D<T> : Game where T : Game
{
	public static new T instance => (T)Game.instance;
	static readonly string ASSEMBLY_NAME = Assembly.GetAssembly(typeof(T))?.GetName().Name;


	int versionMajor;
	int versionMinor;
	int versionPatch;
	char versionSuffix;

	bool loadResourcePackages;

	bool debugStats = false;

	Stack<State> stateMachine = new Stack<State>();


	protected Game3D(int versionMajor, int versionMinor, int versionPatch, char versionSuffix, bool loadResourcePackages)
	{
		this.versionMajor = versionMajor;
		this.versionMinor = versionMinor;
		this.versionPatch = versionPatch;
		this.versionSuffix = versionSuffix;
		this.loadResourcePackages = loadResourcePackages;
	}

	public void compileResources(string projectPath)
	{
#if DEBUG
		string config = "Debug";
#else
		string config = "Release";
#endif

		string binaryPath = projectPath + $"\\bin\\{config}\\net8.0";

		CompileFolder(projectPath, binaryPath + "\\assets");
		CompileFolder("D:\\Dev\\Rainfall\\RainfallNative", binaryPath + "\\assets");

		Utils.RunCommand("xcopy", "/y \"D:\\Dev\\Rainfall\\RainfallNative\\bin\\x64\\" + config + "\\RainfallNative.dll\" \"" + binaryPath + "\"");
	}

	public void packageResources(string projectPath)
	{
#if DEBUG
		string config = "Debug";
#else
		string config = "Release";
#endif

		string binaryPath = projectPath + $"\\bin\\{config}\\net8.0";

		PackageFolder(binaryPath + "\\assets");
	}

	public override void init()
	{
		Display.windowTitle = ASSEMBLY_NAME;

		if (loadResourcePackages)
		{
			Resource.LoadPackageHeader("datas.dat");
			Resource.LoadPackageHeader("datat.dat");
			Resource.LoadPackageHeader("datag.dat");
			Resource.LoadPackageHeader("dataa.dat");
			Resource.LoadPackageHeader("datam.dat");
		}

		InputManager.LoadBindings();
		GraphicsManager.Init();

		Renderer.Init(Display.width, Display.height, graphics);


		Physics.Init();
		Audio.Init();
		AudioManager.Init();
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

		//#if DEBUG
		if (Input.IsKeyPressed(KeyCode.F10) || ImGui.IsKeyPressed(KeyCode.F10, false))
			debugStats = !debugStats;
		//#endif

		Audio.Update();
		AudioManager.Update();

		Physics.Update();

		if (stateMachine.TryPeek(out State state))
			state.update();
	}

	public override void fixedUpdate(float delta)
	{
		if (stateMachine.TryPeek(out State state))
			state.fixedUpdate(delta);
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
#if DEBUG
		Span<byte> str = stackalloc byte[128];

		int y = 0;

		StringUtils.WriteString(str, "Test Build ");
		StringUtils.AppendInteger(str, versionMajor);
		StringUtils.AppendCharacter(str, '.');
		StringUtils.AppendInteger(str, versionMinor);
		StringUtils.AppendCharacter(str, '.');
		StringUtils.AppendInteger(str, versionPatch);
		StringUtils.AppendCharacter(str, versionSuffix);
		StringUtils.AppendString(str, " Debug");

		graphics.drawDebugText(0, y++, 0x1F, str);
#endif
	}

	void drawDebugStats()
	{
		Span<byte> str = stackalloc byte[128];

		int y = 0;

		StringUtils.WriteString(str, "Test Build ");
		StringUtils.AppendInteger(str, versionMajor);
		StringUtils.AppendCharacter(str, '.');
		StringUtils.AppendInteger(str, versionMinor);
		StringUtils.AppendCharacter(str, '.');
		StringUtils.AppendInteger(str, versionPatch);
		StringUtils.AppendCharacter(str, versionSuffix);
#if DEBUG
		StringUtils.AppendString(str, " Debug");
#endif
		graphics.drawDebugText(0, y++, 0x1F, str);

		y = graphics.drawDebugInfo(0, y, 0x1F);

		y = Renderer.DrawDebugStats(0, y, 0x1F);

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
		startInfo.Arguments = $"/C {resCompilerDir}\\RainfallResourceCompiler.exe res {outDir} png hdr ogg vsh fsh csh ttf rfs gltf glb";
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
}
