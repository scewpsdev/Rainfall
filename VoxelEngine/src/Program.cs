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

	Chunk[] chunks;

	float msAcc = 0.0f;
	int fpsAcc = 0;
	int totalFrames = 0;


	public override void init()
	{
		Renderer.Init(graphics);

		camera = new FreeCamera();
		camera.position = new Vector3(10, 2.5f, 20);
		//camera.rotation = Quaternion.LookAt(camera.position, new Vector3(0, 2, 0));

		chunks = new Chunk[9];
		for (int i = 0; i < chunks.Length; i++)
		{
			chunks[i] = new Chunk(256, graphics);
			int x = i % 3;
			int y = 0;
			int z = i / 3;
			chunks[i].position = new Vector3i(x, y, z);
			generateTestChunk(chunks[i]);
			Console.WriteLine("Chunk #" + i + " generated");
		}

		Input.mouseLocked = true;
	}

	void generateTestChunk(Chunk chunk)
	{
		Simplex simplex = new Simplex(0, 3);
		for (int z = 0; z < chunk.resolution; z++)
		{
			for (int x = 0; x < chunk.resolution; x++)
			{
				int globalX = chunk.position.x * chunk.resolution + x;
				int globalZ = chunk.position.z * chunk.resolution + z;
				float scale = 1.0f / chunk.resolution * Chunk.CHUNK_SIZE * 0.02f;
				float heightmap = simplex.sample2f(globalX * scale, globalZ * scale);
				int height = (int)(64 + 32 * heightmap);

				for (int y = 0; y < height; y++)
				{
					float lh = simplex.sample2f(globalX * scale - 0.01f, globalZ * scale);
					float rh = simplex.sample2f(globalX * scale + 0.01f, globalZ * scale);
					float th = simplex.sample2f(globalX * scale, globalZ * scale - 0.01f);
					float bh = simplex.sample2f(globalX * scale, globalZ * scale + 0.01f);
					float dx = (lh - rh) * 32;
					float dz = (th - bh) * 32;
					Vector3 normal = new Vector3(dx, 0.02f, dz).normalized;
					chunk.setVoxel(x, y, z, normal);

					/*
					Vector3 p = new Vector3(x, y, z) + 0.5f;
					Vector3 center = new Vector3(chunk.resolution / 2);
					Vector3 toPoint = p - center;
					int radius = chunk.resolution / 4;
					if (toPoint.lengthSquared < radius * radius)
						chunk.setVoxel(x, y, z, toPoint.normalized);
					*/
				}
			}
		}
		/*
		for (int z = 0; z < chunk.resolution; z++)
		{
			for (int x = 0; x < chunk.resolution; x++)
			{
				for (int y = 0; y < chunk.resolution / 8; y++)
				{
					Vector3 normal = Vector3.Zero;
					if (y == chunk.resolution / 8 - 1)
						normal += Vector3.Up;
					if (x == 0)
						normal += Vector3.Left;
					else if (x == chunk.resolution - 1)
						normal += Vector3.Right;
					if (z == 0)
						normal += Vector3.Forward;
					else if (z == chunk.resolution - 1)
						normal += Vector3.Back;
					normal = normal.normalized;
					chunk.setVoxel(x, y, z, normal);
				}
			}
		}
		*/
		/*
		for (int z = 4; z < 12; z++)
		{
			for (int x = 4; x < 12; x++)
			{
				for (int y = 2; y < 12; y++)
				{
					Vector3 normal = Vector3.Zero;
					if (x == 4)
						normal += Vector3.Left;
					else if (x == 11)
						normal += Vector3.Right;
					if (z == 4)
						normal += Vector3.Forward;
					else if (z == 11)
						normal += Vector3.Back;
					if (y == 11)
						normal += Vector3.Up;
					normal = normal.normalized;
					voxelData[x + y * 16 + z * 16 * 16] = new Vector4(normal, 1.0f);
				}
			}
		}
		*/
		chunk.update(graphics);
	}

	public override void destroy()
	{
		if (totalFrames > 0)
		{
			Console.WriteLine("Final Benchmark:");
			Console.WriteLine("Frametime: " + (msAcc / totalFrames));
			Console.WriteLine("FPS: " + (fpsAcc / totalFrames));
		}
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

		// update
		camera.update();
	}

	public override void draw()
	{
		Renderer.Begin();
		Renderer.SetCamera(camera);

		// draw
		for (int i = 0; i < chunks.Length; i++)
			chunks[i].draw(graphics);
		drawDebugStats();

		Renderer.End();

		if (Time.currentTime > 3 * 1e9)
		{
			fpsAcc += Time.fps;
			msAcc += Time.ms;
			totalFrames++;
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
		Debug.DrawDebugText(Debug.debugTextSize.x - 20, line++, 0xB, str);

		line++;

		StringUtils.WriteInteger(str, Time.fps);
		StringUtils.AppendString(str, " fps");
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteFloat(str, Time.ms, 2);
		StringUtils.AppendString(str, " ms");
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		long mem = Time.memory;
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
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

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
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);

		StringUtils.WriteInteger(str, Time.numAllocations);
		StringUtils.AppendString(str, " allocations");
		Debug.DrawDebugText(Debug.debugTextSize.x - 16, line++, str);
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
