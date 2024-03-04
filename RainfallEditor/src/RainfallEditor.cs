using Rainfall;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public unsafe class RainfallEditor : Game
{
	public static new RainfallEditor instance { get => (RainfallEditor)Game.instance; }
	static readonly string ASSEMBLY_NAME = Assembly.GetAssembly(typeof(RainfallEditor))?.GetName().Name;


	public List<EditorInstance> tabs = new List<EditorInstance>();
	public EditorInstance currentTab = null;

	bool windowClosed = false;

	float cpuTimeAcc = 0.0f;
	float gpuTimeAcc = 0.0f;
	float physicsTimeAcc = 0.0f;
	float cpuTime, gpuTime, physicsTime;
	int numFrames = 0;
	long lastSecond = 0;


	public override void init()
	{
		Display.windowTitle = ASSEMBLY_NAME;

		Renderer.Init(graphics);
		Physics.Init();
		Audio.Init();

		Debug.debugTextEnabled = true;

		newTab(null);
	}

	public override void destroy()
	{
		Audio.Shutdown();
		Physics.Shutdown();
	}

	protected override bool onExitEvent(bool windowExit)
	{
		for (int i = 0; i < tabs.Count; i++)
		{
			if (tabs[i].unsavedChanges)
				EditorUI.unsavedChangesPopup.Add(tabs[i]);
		}
		if (EditorUI.unsavedChangesPopup.Count > 0)
		{
			windowClosed = true;
			return false;
		}
		return true;
	}

	public List<SceneFormat.EntityData> toEntityData(EditorInstance instance)
	{
		List<SceneFormat.EntityData> entities = new List<SceneFormat.EntityData>(instance.entities.Count);
		for (int i = 0; i < instance.entities.Count; i++)
		{
			Entity entity = instance.entities[i];
			entities.Add(new SceneFormat.EntityData
			{
				position = entity.position,
				rotation = entity.rotation,
				scale = entity.scale,
				name = entity.name,
				id = entity.id,
				modelPath = entity.modelPath,
				model = entity.model,
				colliders = new List<SceneFormat.ColliderData>(entity.colliders),
				lights = new List<SceneFormat.LightData>(entity.lights),
				particles = new List<ParticleSystem>(entity.particles),
			});
		}
		return entities;
	}

	public void fromEntityData(List<SceneFormat.EntityData> entities, EditorInstance instance)
	{
		/*
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 scale;

		public string name;
		public uint id;

		public string modelPath;
		public Model model;

		public List<ColliderData> colliders;
		public List<LightData> lights;
		public List<ParticleSystem> particles;
		 */
		foreach (Entity entity in instance.entities)
			entity.destroy();
		instance.entities.Clear();

		for (int i = 0; i < entities.Count; i++)
		{
			SceneFormat.EntityData entityData = entities[i];
			instance.entities.Add(new Entity(entityData.name)
			{
				position = entityData.position,
				rotation = entityData.rotation,
				scale = entityData.scale,
				name = entityData.name,
				id = entityData.id,
				modelPath = entityData.modelPath,
				model = entityData.model,
				colliders = new List<SceneFormat.ColliderData>(entityData.colliders),
				lights = new List<SceneFormat.LightData>(entityData.lights),
				particles = new List<ParticleSystem>(entityData.particles),
			});
			instance.entities[i].reload();
		}
	}

	public void writeScene(EditorInstance instance, string path)
	{
		FileStream stream = new FileStream(instance.path, FileMode.Create, FileAccess.Write);
		SceneFormat.SerializeScene(toEntityData(instance), stream);
		stream.Close();
	}

	public void readScene(EditorInstance instance, string path)
	{
		FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
		List<SceneFormat.EntityData> entities = SceneFormat.DeserializeScene(stream);
		fromEntityData(entities, instance);
		stream.Close();
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

	public EditorInstance newTab(string path = null)
	{
		EditorInstance instance = new EditorInstance(path);
		tabs.Add(instance);
		currentTab = instance;
		EditorUI.nextSelectedTab = instance;
		return instance;
	}

	public void closeTab(EditorInstance instance)
	{
		if (instance.unsavedChanges)
		{
			EditorUI.unsavedChangesPopup.Add(instance);
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

			newTab(path);
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

			writeScene(tab, tab.path);

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
			writeScene(tab, tab.path);

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
		if (path != null && path.Length == 0)
			Debug.Assert(false);

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
		if (windowClosed && EditorUI.unsavedChangesPopup.Count == 0)
			terminate();

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
