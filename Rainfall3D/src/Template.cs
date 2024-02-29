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

		Debug.debugTextEnabled = true;
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

		if (!GraphicsManager.cinematicMode)
			drawDebugStats();

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
		int line = 0;

		Span<byte> str = stackalloc byte[128];

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

		StringUtils.WriteString(str, "Physics bodies: ");
		StringUtils.AppendInteger(str, RigidBody.numBodies);
		Debug.DrawDebugText(0, line++, str);

		line++;

		RenderStats renderStats = graphics.getRenderStats();
		cpuTimeAcc += renderStats.cpuTime;
		gpuTimeAcc += renderStats.gpuTime;
		physicsTimeAcc += Time.physicsDelta;
		numFrames++;

		StringUtils.WriteString(str, "CPU time: ");
		StringUtils.AppendFloat(str, cpuTime * 1000);
		StringUtils.AppendString(str, "ms");
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "GPU time: ");
		StringUtils.AppendFloat(str, gpuTime * 1000);
		StringUtils.AppendString(str, "ms");
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "Physics time: ");
		StringUtils.AppendFloat(str, physicsTime * 1000);
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

		StringUtils.WriteString(str, "Textures: ");
		StringUtils.AppendInteger(str, renderStats.numTextures);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "Render Targets: ");
		StringUtils.AppendInteger(str, renderStats.numRenderTargets);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "Shaders: ");
		StringUtils.AppendInteger(str, renderStats.numShaders);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "Draw calls: ");
		StringUtils.AppendInteger(str, (int)renderStats.numDraw);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "Triangles: ");
		StringUtils.AppendInteger(str, renderStats.numTriangles);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "Computes: ");
		StringUtils.AppendInteger(str, (int)renderStats.numCompute);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "Blits: ");
		StringUtils.AppendInteger(str, (int)renderStats.numBlit);
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
		string projectDir = "D:\\Dev\\2024\\" + ASSEMBLY_NAME + "\\" + ASSEMBLY_NAME;
		string resCompilerDir = "D:\\Dev\\Rainfall\\RainfallResourceCompiler\\bin\\x64\\Debug";
		//string projectDir = "C:\\Users\\faris\\Documents\\Dev\\Rainfall";
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
		startInfo.FileName = "cmd.exe";
		startInfo.Arguments = "/C " + resCompilerDir + "\\RainfallResourceCompiler.exe res " + projectDir + "\\" + outDir + "\\net8.0\\res gltf fbx png hdr ogg dat shader ttf";
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

		Game game = new Program();
		game.run(launchParams);
	}
}
