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
}

internal class GraphicsManager
{
	const string GRAPHICS_SETTINGS_FILE = "GraphicsSettings.config";


	static GraphicsSettings settings;

	public static Cubemap skybox;
	public static float skyboxIntensity = 1.0f;
	public static Cubemap environmentMap;
	public static float environmentMapIntensity = 1.0f;
	public static DirectionalLight sun;


	public static void Init()
	{
		settings = new GraphicsSettings();

		DatFile settingsFile = new DatFile(File.ReadAllText(GRAPHICS_SETTINGS_FILE), GRAPHICS_SETTINGS_FILE);
		foreach (DatField setting in settingsFile.root.fields)
		{
			bool found = true;

			if (setting.name == "VSync")
			{
				Debug.Assert(setting.value.type == DatValueType.Number);
				settings.vsync = setting.value.integer;
			}
			else if (setting.name == "FpsCap")
			{
				Debug.Assert(setting.value.type == DatValueType.Number);
				settings.fpsCap = setting.value.integer;
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
