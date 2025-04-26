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

	public static Cubemap skybox = null;
	public static float skyboxIntensity = 1.0f;
	public static Cubemap environmentMap = null;
	public static float environmentMapIntensity = 1.0f;
	public static DirectionalLight sun = null;

	public static bool bloomEnabled = true;
	public static float bloomStrength = 0.1f;
	public static float bloomFalloff = 5.0f;

	public static bool vignetteEnabled = true;
	public static Vector4 vignetteColor = new Vector4(0, 0, 0, 1);
	public static float vignetteFalloff = 0.12f;

	public static bool ssaoEnabled = true;

	public static Vector3 fogColor = Vector3.One;
	public static float fogStrength = 0.0f;


	public static void Init()
	{
		settings = new GraphicsSettings();

		if (File.Exists(GRAPHICS_SETTINGS_FILE))
		{
			DatFile settingsFile = new DatFile(File.ReadAllText(GRAPHICS_SETTINGS_FILE), GRAPHICS_SETTINGS_FILE);
			foreach (DatField setting in settingsFile.root.fields)
			{
				bool found = true;

				if (setting.name.Equals("vsync", StringComparison.OrdinalIgnoreCase))
				{
					Debug.Assert(setting.value.type == DatValueType.Number);
					settings.vsync = setting.value.integer;
				}
				else if (setting.name.Equals("fpscap", StringComparison.OrdinalIgnoreCase))
				{
					Debug.Assert(setting.value.type == DatValueType.Number);
					settings.fpsCap = setting.value.integer;
				}
				else if (setting.name.Equals("potato", StringComparison.OrdinalIgnoreCase))
				{
					Debug.Assert(setting.value.type == DatValueType.Number);
					settings.potato = setting.value.integer != 0;
				}
				else if (setting.name.Equals("ambientocclusion", StringComparison.OrdinalIgnoreCase))
				{
					Debug.Assert(setting.value.type == DatValueType.Number);
					settings.ambientOcclusion = setting.value.integer != 0;
				}
				else if (setting.name.Equals("bloom", StringComparison.OrdinalIgnoreCase))
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
	}

	static void ApplyGraphicsSettings()
	{
		Display.vsync = settings.vsync;
		Display.fpsCap = settings.fpsCap;
		//Renderer.simplifiedLighting = settings.potato;
		//Renderer.ambientOcclusionEnabled = settings.ambientOcclusion;
		//Renderer.bloomEnabled = settings.bloom;
	}

	public static void Draw()
	{
		RendererSettings renderSettings = new RendererSettings(0);
		renderSettings.bloomEnabled = bloomEnabled;
		renderSettings.bloomStrength = bloomStrength;
		renderSettings.bloomFalloff = bloomFalloff;
		renderSettings.vignetteEnabled = vignetteEnabled;
		renderSettings.vignetteColor = vignetteColor;
		renderSettings.vignetteFalloff = vignetteFalloff;
		renderSettings.ssaoEnabled = ssaoEnabled;
		renderSettings.fogColor = fogColor;
		renderSettings.fogStrength = fogStrength;
		//renderSettings.exposure = 8;
		Renderer.SetSettings(renderSettings);

		if (environmentMap != null)
			Renderer.DrawEnvironmentMap(environmentMap, environmentMapIntensity);
		if (skybox != null)
			Renderer.DrawSky(skybox, skyboxIntensity, Quaternion.Identity);
		if (sun != null)
			Renderer.DrawDirectionalLight(sun);
	}
}
