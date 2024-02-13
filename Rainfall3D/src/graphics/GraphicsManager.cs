using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GraphicsSettings
{
	public int vsync = 0;
	public int fpsCap = 0;
	public bool potato = false;
	public bool ambientOcclusion = true;
	public bool bloom = true;
}

public static class GraphicsManager
{
	const string GRAPHICS_SETTINGS_FILE = "GraphicsSettings.config";


	static GraphicsSettings settings;

	public static bool cinematicMode = false;

	public static Cubemap skybox = null;
	public static float skyboxIntensity = 1.0f;
	public static Cubemap environmentMap = null;
	public static float environmentMapIntensity = 1.0f;
	public static DirectionalLight sun = null;


	public static void Init()
	{
		settings = new GraphicsSettings();

		DatFile settingsFile = new DatFile(File.ReadAllText(GRAPHICS_SETTINGS_FILE), GRAPHICS_SETTINGS_FILE);
		foreach (DatField setting in settingsFile.root.fields)
		{
			bool found = true;

			if (setting.name == "vSync")
			{
				Debug.Assert(setting.value.type == DatValueType.Number);
				settings.vsync = setting.value.integer;
			}
			else if (setting.name == "fpsCap")
			{
				Debug.Assert(setting.value.type == DatValueType.Number);
				settings.fpsCap = setting.value.integer;
			}
			else if (setting.name == "potato")
			{
				Debug.Assert(setting.value.type == DatValueType.Number);
				settings.potato = setting.value.integer != 0;
			}
			else if (setting.name == "ambientOcclusion")
			{
				Debug.Assert(setting.value.type == DatValueType.Number);
				settings.ambientOcclusion = setting.value.integer != 0;
			}
			else if (setting.name == "bloom")
			{
				Debug.Assert(setting.value.type == DatValueType.Number);
				settings.bloom = setting.value.integer != 0;
			}
			else
			{
				found = false;
			}

			if (found)
				Console.WriteLine("Loaded graphics setting " + setting.name);
		}

		ApplyGraphicsSettings();
	}

	static void ApplyGraphicsSettings()
	{
		Display.vsync = settings.vsync;
		Display.fpsCap = settings.fpsCap;
		Renderer.simplifiedLighting = settings.potato;
		Renderer.ambientOcclusionEnabled = settings.ambientOcclusion;
		Renderer.bloomEnabled = settings.bloom;
	}

	public static void Draw()
	{
		if (environmentMap != null)
			Renderer.SetEnvironmentMap(environmentMap, environmentMapIntensity);
		if (skybox != null)
			Renderer.DrawSky(skybox, skyboxIntensity, Matrix.Identity);
		if (sun != null)
			Renderer.DrawDirectionalLight(sun);
	}
}
