using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public unsafe class RainfallEditor : Game
{
	public static new RainfallEditor instance { get => (RainfallEditor)Game.instance; }


	public List<EditorInstance> tabs = new List<EditorInstance>();
	public EditorInstance currentTab = null;

	float cpuTimeAcc = 0.0f;
	float gpuTimeAcc = 0.0f;
	float physicsTimeAcc = 0.0f;
	float cpuTime, gpuTime, physicsTime;
	int numFrames = 0;
	long lastSecond = 0;


	public override void init()
	{
		Renderer.Init(graphics);
		Physics.Init();
		Audio.Init();

		Debug.debugTextEnabled = true;

		newTab();
	}

	public override void destroy()
	{
		Audio.Shutdown();
		Physics.Shutdown();
	}

	public EditorInstance getTab(string path)
	{
		foreach (EditorInstance tab in tabs)
		{
			if (tab.path == path)
				return tab;
		}
		return null;
	}

	public EditorInstance newTab()
	{
		EditorInstance instance = new EditorInstance();
		tabs.Add(instance);
		currentTab = instance;
		EditorUI.nextSelectedTab = instance;
		return instance;
	}

	public void closeTab(EditorInstance instance)
	{
		if (instance.unsavedChanges)
		{
			EditorUI.unsavedChangesPopup = instance;
		}
		else
		{
			instance.destroy();
			tabs.Remove(instance);
			if (tabs.Count == 0)
				currentTab = null;
		}
	}

	public EditorInstance getNextTab(EditorInstance tab)
	{
		int idx = tabs.IndexOf(tab);
		int nextIdx = (idx + 1) % tabs.Count;
		return tabs[nextIdx];
	}

	public EditorInstance getPrevTab(EditorInstance tab)
	{
		int idx = tabs.IndexOf(tab);
		int nextIdx = (idx - 1 + tabs.Count) % tabs.Count;
		return tabs[nextIdx];
	}

	public void open()
	{
		string defaultPath = null;
		if (currentTab != null)
			defaultPath = currentTab.path;

		byte* outPath;
		NFDResult result = NFD.NFD_OpenDialog("rfs", defaultPath, &outPath);
		if (result == NFDResult.NFD_OKAY)
		{
			string path = new string((sbyte*)outPath);
			EditorInstance alreadyOpenTab = getTab(path);
			if (alreadyOpenTab != null)
				closeTab(alreadyOpenTab);
			else if (currentTab != null && currentTab.path == null && !currentTab.unsavedChanges)
				closeTab(currentTab);

			EditorInstance instance = newTab();
			instance.path = path;
			SceneFormat.ReadScene(instance, instance.path);
			NFD.NFDi_Free(outPath);
		}
	}

	public void saveAs(EditorInstance tab)
	{
		byte* savePath;
		NFDResult result = NFD.NFD_SaveDialog("rfs", null, &savePath);
		if (result == NFDResult.NFD_OKAY)
		{
			tab.path = new string((sbyte*)savePath);
			SceneFormat.WriteScene(tab, tab.path);
			tab.notifySave();
			NFD.NFDi_Free(savePath);
		}
	}

	public void saveAs()
	{
		if (currentTab != null)
			saveAs(currentTab);
	}

	public void save(EditorInstance tab)
	{
		if (tab.path == null)
		{
			saveAs(tab);
		}
		else
		{
			SceneFormat.WriteScene(tab, tab.path);
			tab.notifySave();
		}
	}

	public void save()
	{
		if (currentTab != null && currentTab.unsavedChanges)
			save(currentTab);
	}

	public void saveAll()
	{
		foreach (EditorInstance tab in tabs)
		{
			if (tab.unsavedChanges)
			{
				save(tab);
			}
		}
	}

	public string compileAsset(string path)
	{
		string getFilenameFromPath(string path)
		{
			int slash = path.LastIndexOfAny(new char[] { '/', '\\' });
			if (slash != -1)
				return path.Substring(slash + 1);
			return path;
		}

		string getParentPath(string path)
		{
			int slash = path.LastIndexOfAny(new char[] { '/', '\\' });
			if (slash != -1)
				return path.Substring(0, slash);
			return null;
		}

		string assetName = getFilenameFromPath(path);
		string outDir = "res";

		string projectDir = "D:\\Dev\\Rainfall\\RainfallEditor";
		string resCompilerDir = "D:\\Dev\\Rainfall\\RainfallResourceCompiler\\" + "bin\\x64\\Debug";
		System.Diagnostics.Process process = new System.Diagnostics.Process();
		System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
		startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
		startInfo.FileName = "cmd.exe";
		startInfo.Arguments = "/C " + resCompilerDir + "\\RainfallResourceCompiler.exe -f " + assetName + " " + projectDir + "\\" + outDir + "\\" + assetName;
		startInfo.WorkingDirectory = getParentPath(path);
		process.StartInfo = startInfo;
		process.Start();
		process.WaitForExit();

		return "../../../../" + outDir + "\\" + assetName;
	}

	public override void update()
	{
		currentTab?.update();

		Physics.Update();
	}

	public override void draw()
	{
		currentTab?.draw(graphics);

		EditorUI.Draw(this);

		//drawDebugStats();

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
		string projectDir = "D:\\Dev\\Rainfall\\RainfallEditor";
		string resCompilerDir = "D:\\Dev\\Rainfall\\RainfallResourceCompiler\\" + outDir;
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
		launchParams.width = 1600;
		launchParams.height = 900;
		launchParams.maximized = true;
		launchParams.vsync = 0;
		launchParams.fpsCap = 60;

		Game game = new RainfallEditor();
		game.run(launchParams);
	}
}
