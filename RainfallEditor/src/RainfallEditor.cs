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

	public void writeScene(EditorInstance instance, string path)
	{
		FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
		SceneFormat.SerializeScene(ToEntityData(instance), instance.selectedEntity, stream);
		stream.Close();
	}

	public void readScene(EditorInstance instance, string path)
	{
		FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
		SceneFormat.DeserializeScene(stream, out List<SceneFormat.EntityData> entities, out uint selectedEntity);
		FromEntityData(entities, instance);
		instance.selectedEntity = selectedEntity;
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

	static string RelativePath(string path, string root)
	{
		if (path == null)
			return null;
		root = Path.GetDirectoryName(root);
		return Path.GetRelativePath(root, path);
	}

	static string AbsolutePath(string path, string root)
	{
		if (path == null)
			return null;
		root = Path.GetDirectoryName(root);
		return root + "/" + path;
	}

	public static List<SceneFormat.EntityData> ToEntityData(EditorInstance instance)
	{
		List<SceneFormat.EntityData> entities = new List<SceneFormat.EntityData>(instance.entities.Count);
		for (int i = 0; i < instance.entities.Count; i++)
		{
			Entity entity = instance.entities[i];
			SceneFormat.EntityData entityData = new SceneFormat.EntityData
			{
				name = entity.name,
				id = entity.id,
				isStatic = entity.isStatic,
				position = entity.position,
				rotation = entity.rotation,
				scale = entity.scale,
				modelPath = RelativePath(entity.modelPath, instance.path),
				model = entity.model,
				colliders = new List<SceneFormat.ColliderData>(entity.colliders),
				lights = new List<SceneFormat.LightData>(entity.lights),
				particles = new List<ParticleSystem>(entity.particles),
			};

			for (int j = 0; j < entityData.colliders.Count; j++)
			{
				SceneFormat.ColliderData collider = entityData.colliders[j];
				if (collider.type == SceneFormat.ColliderType.Mesh && collider.meshColliderPath != null)
					collider.meshColliderPath = RelativePath(collider.meshColliderPath, instance.path);
				entityData.colliders[j] = collider;
			}
			for (int j = 0; j < entityData.particles.Count; j++)
			{
				ParticleSystem particles = new ParticleSystem(0);
				particles.copyData(entity.particles[j]);
				if (particles.textureAtlasPath != null)
					particles.textureAtlasPath = RelativePath(particles.textureAtlasPath, instance.path);
			}

			entities.Add(entityData);
		}
		return entities;
	}

	public static void FromEntityData(List<SceneFormat.EntityData> entities, EditorInstance instance)
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
		instance.reset();

		bool deselectEntity = true;
		for (int i = 0; i < entities.Count; i++)
		{
			SceneFormat.EntityData entityData = entities[i];
			Entity entity = new Entity(entityData.name)
			{
				name = entityData.name,
				id = entityData.id,
				isStatic = entityData.isStatic,
				position = entityData.position,
				rotation = entityData.rotation,
				scale = entityData.scale,
				modelPath = AbsolutePath(entityData.modelPath, instance.path),
				model = entityData.model,
				colliders = new List<SceneFormat.ColliderData>(entityData.colliders),
				lights = new List<SceneFormat.LightData>(entityData.lights),
				particles = new List<ParticleSystem>(entityData.particles),
			};

			for (int j = 0; j < entity.colliders.Count; j++)
			{
				SceneFormat.ColliderData collider = entity.colliders[j];
				if (collider.type == SceneFormat.ColliderType.Mesh && collider.meshColliderPath != null)
					collider.meshColliderPath = AbsolutePath(collider.meshColliderPath, instance.path);
				entity.colliders[j] = collider;
			}
			for (int j = 0; j < entity.particles.Count; j++)
			{
				ParticleSystem particles = new ParticleSystem(0);
				particles.copyData(entityData.particles[j]);
				if (particles.textureAtlasPath != null)
					particles.textureAtlasPath = AbsolutePath(particles.textureAtlasPath, instance.path);
			}

			entity.reload();
			if (entityData.id == instance.selectedEntity)
				deselectEntity = false;

			instance.entities.Add(entity);
		}

		if (deselectEntity)
			instance.selectedEntity = 0;
	}

	public static string CompileAsset(string path)
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

		string projectDir = "D:\\Dev\\2023\\Rainfall\\RainfallEditor";
		string resCompilerDir = "D:\\Dev\\2023\\Rainfall\\RainfallResourceCompiler\\" + "bin\\x64\\Debug";
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
	}

	public static void Main(string[] args)
	{
#if DEBUG
		string outDir = "bin\\x64\\Debug";
		string projectDir = "D:\\Dev\\2023\\Rainfall\\RainfallEditor";
		string resCompilerDir = "D:\\Dev\\2023\\Rainfall\\RainfallResourceCompiler\\" + outDir;
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
