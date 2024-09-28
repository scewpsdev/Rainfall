using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum AimMode
{
	Directional,
	Crosshair
}

public struct GameSettings
{
	public AimMode aimMode = AimMode.Directional;

	public GameSettings(int _) { }
}

public struct GraphicsSettings
{
	public GraphicsSettings(int _) { }
}

public static class Settings
{
	public static GameSettings game = new GameSettings(0);
	public static GraphicsSettings graphics = new GraphicsSettings(0);

	const string gameSettingsPath = "GameSettings.config";
	const string graphicsSettingsPath = "GraphicsSettings.config";


	public static void Load()
	{
		LoadGameSettings();
		LoadGraphicsSettings();

		OptionsMenu.Init();
	}

	static void LoadGameSettings()
	{
		if (File.Exists(gameSettingsPath))
		{
			DatFile file = new DatFile(File.ReadAllText(gameSettingsPath), gameSettingsPath);
			if (file.getIdentifier("aimMode", out string aimMode))
				game.aimMode = Utils.ParseEnum<AimMode>(aimMode);
		}
	}

	static void LoadGraphicsSettings()
	{
		if (File.Exists(graphicsSettingsPath))
		{
			DatFile file = new DatFile(File.ReadAllText(graphicsSettingsPath), graphicsSettingsPath);
			file.getBoolean("bloom", out Renderer.bloomEnabled);
		}
	}

	public static void Save()
	{
		SaveGameSettings();
		SaveGraphicsSettings();
	}

	static void SaveGameSettings()
	{
		DatFile file = new DatFile();
		file.addIdentifier("aimMode", game.aimMode.ToString());
		file.serialize(gameSettingsPath);

#if DEBUG
		Utils.RunCommand("xcopy", "/y \"" + gameSettingsPath + "\" \"..\\..\\..\\\"");
#endif

		Console.WriteLine("Saved game settings");
	}

	static void SaveGraphicsSettings()
	{
		DatFile file = new DatFile();
		file.addBoolean("bloom", Renderer.bloomEnabled);
		file.serialize(graphicsSettingsPath);

#if DEBUG
		Utils.RunCommand("xcopy", "/y \"" + graphicsSettingsPath + "\" \"..\\..\\..\\\"");
#endif

		Console.WriteLine("Saved graphics settings");
	}
}
