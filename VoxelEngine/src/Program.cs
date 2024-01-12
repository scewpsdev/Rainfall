using Rainfall;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

internal class Program : Game
{
	const int VERSION_MAJOR = 0;
	const int VERSION_MINOR = 1;
	const int VERSION_PATCH = 1;
	const char VERSION_SUFFIX = 'a';


	public static new Program instance { get => (Program)Game.instance; }


	Camera camera;

	//World world;
	Chunk chunk;

	float msAcc = 0.0f;
	int fpsAcc = 0;
	int totalFrames = 0;

	float cpuTimeAcc = 0.0f;
	float gpuTimeAcc = 0.0f;
	float cpuTime, gpuTime;
	int numFrames = 0;
	long lastSecond = 0;


	public override void init()
	{
		Renderer.Init(graphics);

		camera = new FreeCamera();
		camera.position = new Vector3(8, 20, 8);
		camera.yaw = MathF.PI * -0.5f;
		camera.pitch = MathF.PI * -0.5f;

		//world = new World(graphics);
		chunk = new Chunk(256, graphics);
		for (int z = 0; z < chunk.resolution; z++)
		{
			for (int y = 0; y < chunk.resolution; y++)
			{
				for (int x = 0; x < chunk.resolution; x++)
				{
					chunk.setVoxel(x, y, z, Random.Shared.Next() % 256 == 0);
					//chunk.setVoxel(x, y, z, y < chunk.resolution / 2);
				}
			}
		}
		chunk.update(graphics);

		Input.mouseLocked = true;
	}

	public override void destroy()
	{
		//world.destroy(graphics);
		chunk.destroy(graphics);

		if (totalFrames > 0)
		{
			Console.WriteLine("Final Benchmark:");
			Console.WriteLine("Frametime: " + (msAcc / totalFrames));
			Console.WriteLine("FPS: " + (fpsAcc / totalFrames));
		}
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

		// update
		camera.update();
	}

	public override void draw()
	{
		Renderer.Begin();
		Renderer.SetCamera(camera);

		// draw
		//world.draw(graphics);
		chunk.draw(new Vector3i(0, 0, 0));
		//chunk.draw(new Vector3i(0, 0, 1));
		//chunk.draw(new Vector3i(0, 1, 0));
		//chunk.draw(new Vector3i(0, 1, 1));
		//chunk.draw(new Vector3i(1, 0, 0));
		//chunk.draw(new Vector3i(1, 0, 1));
		//chunk.draw(new Vector3i(1, 1, 0));
		//chunk.draw(new Vector3i(1, 1, 1));

		drawDebugStats();

		Renderer.End();

		if (Time.currentTime > 3 * 1e9)
		{
			fpsAcc += Time.fps;
			msAcc += Time.ms;
			totalFrames++;
		}
		if (Time.currentTime - lastSecond >= 1e9)
		{
			cpuTime = cpuTimeAcc / numFrames;
			gpuTime = gpuTimeAcc / numFrames;
			cpuTimeAcc = 0;
			gpuTimeAcc = 0;
			numFrames = 0;
			lastSecond = Time.currentTime;
		}
	}

	void drawDebugStats()
	{
		int line = 0;

		Span<byte> str = stackalloc byte[64];

		StringUtils.WriteString(str, "Test Build 0.0.0a");
		str[11] = '0' + VERSION_MAJOR;
		str[13] = '0' + VERSION_MINOR;
		str[15] = '0' + VERSION_PATCH;
		str[16] = (byte)VERSION_SUFFIX;
		Debug.DrawDebugText(0, line++, 0xB, str);

		line++;

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

		line++;

		graphics.getRenderStats(out RenderStats renderStats);
		cpuTimeAcc += renderStats.cpuTime;
		gpuTimeAcc += renderStats.gpuTime;
		numFrames++;

		StringUtils.WriteString(str, "CPU time: ");
		StringUtils.AppendFloat(str, cpuTime * 1000);
		StringUtils.AppendString(str, "ms");
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "GPU time: ");
		StringUtils.AppendFloat(str, gpuTime * 1000);
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

		StringUtils.WriteString(str, "Draw calls: ");
		StringUtils.AppendInteger(str, (int)renderStats.numDraw);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "Computes: ");
		StringUtils.AppendInteger(str, (int)renderStats.numCompute);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "Blits: ");
		StringUtils.AppendInteger(str, (int)renderStats.numBlit);
		Debug.DrawDebugText(0, line++, str);

		line++;

		StringUtils.WriteString(str, "x=");
		StringUtils.AppendFloat(str, camera.position.x);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "y=");
		StringUtils.AppendFloat(str, camera.position.y);
		Debug.DrawDebugText(0, line++, str);

		StringUtils.WriteString(str, "z=");
		StringUtils.AppendFloat(str, camera.position.z);
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
		else
		{
			StringUtils.AppendFloat(str, mem, 2);
			StringUtils.AppendString(str, "By");
		}
	}

	public static void Main(string[] args)
	{
#if DEBUG
		string outDir = "bin\\x64\\Debug";
		string projectDir = "D:\\Dev\\Rainfall\\VoxelEngine";
		string resCompilerDir = "D:\\Dev\\Rainfall\\RainfallResourceCompiler\\" + outDir;
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
		//launchParams.fpsCap = 60;
		//launchParams.vsync = 1;

		Game game = new Program();
		game.run(launchParams);
	}
}
