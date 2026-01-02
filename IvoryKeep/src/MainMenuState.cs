using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;


enum MainMenuScreen
{
	Main,
	SaveSelect,
	SaveCreate,
	CustomRunSettings,
	Options,
	Credits,
}

public class MainMenuState : State
{
	const float FADEIN = 1;


	MainMenuScreen screen = MainMenuScreen.Main;
	int currentButton = 0;

	int creditsScroll = 0;
	int creditsHeight;

	Sprite splash, splashSmall;

	UIParticleEffect particles;

	MultilayerTrack mainMenuAmbience;

	long startTime = -1;


	public MainMenuState()
	{
		splash = new Sprite(Resource.GetTexture("sprites/ui/splash2.png", false), 0, 0, 256, 64);
		splashSmall = new Sprite(Resource.GetTexture("sprites/ui/splash2.png", false), 0, 64, 256, 32);

		particles = new UIParticleEffect(null, "effects/menu.rfs");

		mainMenuAmbience = new MultilayerTrack("sounds/ost/misc/menu", 0);
	}

	public override void onSwitchTo(State from)
	{
		Array.Fill(IvoryKeep.instance.saves, null);
		for (int i = 0; i < 3; i++)
		{
			if (File.Exists(SaveFile.GetSaveFilePath(i)))
				IvoryKeep.instance.saves[i] = SaveFile.Load(i);
		}

		startTime = Time.timestamp;
		AudioManager.SetAmbientTrack(mainMenuAmbience, true);
	}

	void mainScreen()
	{
		float elapsed = (Time.timestamp - startTime) / 1e9f;
		float anim = MathF.Pow(MathF.Min(elapsed / FADEIN, 1), 0.3f);
		Renderer.DrawUISprite(Renderer.UIWidth / 2 - splash.width / 2, 64 - splash.height / 2 + (1 - anim) * splash.height, splash.width, (int)(anim * splash.height), splash.spriteSheet.texture, splash.position.x, splash.position.y, splash.size.x, (int)(splash.size.y * anim), 0xFF888888);

		string[] labels = [
			"Start",
			"Daily Run",
			"Custom Run",
			"Options",
			"Credits",
			"Quit"
		];

		bool[] enabled = [
			true,
			true,
			true,
			true,
			true,
			true
		];

		float[] fade = new float[labels.Length];
		for (int i = 0; i < fade.Length; i++)
			fade[i] = Mathf.Clamp(elapsed - 0.5f * FADEIN - i * 0.1f, 0, 1);

		int selection = FullscreenMenu.Render(labels, enabled, ref currentButton, fade);
		if (selection != -1)
		{
			switch (selection)
			{
				case 0: // Start
					screen = MainMenuScreen.SaveSelect;
					currentButton = 0;
					break;

				case 1: // Daily Run
					DateTime today = DateTime.Today;
					int day = today.DayOfYear;
					int year = today.Year;
					uint seed = Hash.combine(Hash.hash(day), Hash.hash(year));
					IvoryKeep.instance.pushState(new GameState(-1, seed.ToString(), false, true));
					break;

				case 2: // Custom Run
					screen = MainMenuScreen.CustomRunSettings;
					break;

				case 3: // Options
					screen = MainMenuScreen.Options;
					SettingsUI.OnOpen();
					break;

				case 4: // Credits
					screen = MainMenuScreen.Credits;
					creditsScroll = 0;
					break;

				case 5: // Quit
					IvoryKeep.instance.popState();
					break;

				default:
					Debug.Assert(false);
					break;
			}
		}

#if DEBUG
		string versionStr = $"Test Build {IvoryKeep.VERSION_MAJOR}.{IvoryKeep.VERSION_MINOR}.{IvoryKeep.VERSION_PATCH}{IvoryKeep.VERSION_SUFFIX} DEBUG";
#else
		string versionStr = $"Test Build {IvoryKeep.VERSION_MAJOR}.{IvoryKeep.VERSION_MINOR}.{IvoryKeep.VERSION_PATCH}{IvoryKeep.VERSION_SUFFIX}";
#endif
		Renderer.DrawUITextBMP(0, 0, versionStr);
	}

	void saveSelect()
	{
		string[] labels = new string[3];
		bool[] enabled = new bool[3];

		for (int i = 0; i < 3; i++)
		{
			if (IvoryKeep.instance.saves[i] != null)
				labels[i] = IvoryKeep.instance.saves[i].name;
			else
				labels[i] = "Save <" + (i + 1) + ">";
		}

		Array.Fill(enabled, true);

		int selection = FullscreenMenu.Render(labels, enabled, ref currentButton);
		if (selection != -1)
		{
			if (IvoryKeep.instance.saves[selection] != null)
			{
				screen = MainMenuScreen.Main;
				currentButton = 0;
				IvoryKeep.instance.pushState(new GameState(selection, null));
			}
			else
			{
				saveCreateID = selection;
				screen = MainMenuScreen.SaveCreate;
			}
		}
	}

	StringBuilder saveFileNameStr = new StringBuilder();
	int saveCreateID = -1;

	void saveCreate()
	{
		int width = 60;
		int height = 15;
		int x = Renderer.UIWidth / 2 - width / 2;
		int y = Renderer.UIHeight / 2 - height / 2;

		Renderer.DrawUISprite(x - 2, y - 2, width + 4, height + 4, null, false, 0xFFAAAAAA);
		Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFF000000);
		Renderer.DrawUITextBMP(x, Renderer.UIHeight / 2 - Renderer.smallFont.size / 2, saveFileNameStr.ToString() + "_", 1, 0xFFFFFFFF);
	}

	StringBuilder customRunSeedStr = new StringBuilder();

	void customRun()
	{
		int width = 60;
		int height = 15;
		int x = Renderer.UIWidth / 2 - width / 2;
		int y = Renderer.UIHeight / 2 - height / 2;

		Renderer.DrawUISprite(x - 2, y - 2, width + 4, height + 4, null, false, 0xFFAAAAAA);
		Renderer.DrawUISprite(x - 1, y - 1, width + 2, height + 2, null, false, 0xFF000000);
		Renderer.DrawUITextBMP(x, Renderer.UIHeight / 2 - Renderer.smallFont.size / 2, customRunSeedStr.ToString() + "_", 1, 0xFFFFFFFF);
	}

	void options()
	{
		if (!SettingsUI.Render())
			screen = MainMenuScreen.Main;
	}

	void credits()
	{
		if (InputManager.IsDown("UIDown") || InputManager.IsDown("Down"))
			creditsScroll = Math.Min(creditsScroll + 1, creditsHeight - Renderer.UIHeight);
		if (InputManager.IsDown("UIUp") || InputManager.IsDown("Up"))
			creditsScroll = Math.Max(creditsScroll - 1, 0);

		int x = 100;
		int y = 10 - creditsScroll;

		Renderer.DrawUISprite(Renderer.UIWidth / 2 - splashSmall.width / 2, y, splashSmall.width, splashSmall.height, 0, splashSmall, 0xFF888888);
		y += 45;

		void drawLine(string str)
		{
			Renderer.DrawUITextBMP(x, y, str, 1);
			y += Renderer.smallFont.size + 2;
		}

		void back()
		{
			y -= Renderer.smallFont.size + 2;
		}

		void drawLineRight(string str)
		{
			Renderer.DrawUITextBMP(Renderer.UIWidth - x - Renderer.MeasureUITextBMP(str).x, y, str, 1);
			y += Renderer.smallFont.size + 2;
		}

		drawLine("A Game by Scewps");
		drawLine("");
		drawLine("Soundtrack:");
		back();
		drawLineRight("Scewps");
		drawLineRight("Abstraction");
		drawLine("");
		drawLine("With help from:");
		back();
		drawLineRight("Tojota");
		drawLine("");
		drawLine("Playtesters:");
		back();
		drawLineRight("Godebob");
		drawLineRight("Lenee");
		drawLineRight("Tojota");
		drawLineRight("SovereignsGreed");
		drawLineRight("KarrasTheRaven");
		drawLineRight("Souls_Lito");
		drawLineRight("wsfz_demon");
		drawLine("");
		drawLine("Libraries:");
		back();
		drawLineRight("GLFW");
		drawLineRight("BGFX");
		drawLineRight("Soloud");
		drawLine("");
		drawLine("Software");
		back();
		drawLineRight("Visual Studio");
		drawLineRight("Aseprite");
		drawLineRight("OneNote");
		drawLineRight("Audacity");
		drawLineRight("Reaper");
		drawLineRight("Tiled");
		drawLineRight("Leaderboards.Dev");

		y += 10;

		if (creditsHeight == 0)
			creditsHeight = y - creditsScroll;
	}

	public override void onCharEvent(byte length, uint value)
	{
		if (screen == MainMenuScreen.SaveCreate)
		{
			saveFileNameStr.Append((char)value);
		}
		else if (screen == MainMenuScreen.CustomRunSettings)
		{
			customRunSeedStr.Append((char)value);
		}
	}

	public override void onKeyEvent(KeyCode key, KeyModifier modifiers, bool down)
	{
		if (screen == MainMenuScreen.SaveSelect)
		{
			if (InputManager.IsPressed("UIQuit", true) || InputManager.IsPressed("UIBack", true))
			{
				screen = MainMenuScreen.Main;
				currentButton = 0;
				Audio.PlayBackground(UISound.uiBack);
			}
		}
		else if (screen == MainMenuScreen.SaveCreate)
		{
			if (key == KeyCode.Backspace && modifiers == KeyModifier.None && down && saveFileNameStr.Length > 0)
				saveFileNameStr.Remove(saveFileNameStr.Length - 1, 1);
			if (key == KeyCode.Return && modifiers == KeyModifier.None && down)
			{
				IvoryKeep.instance.pushState(new GameState(saveCreateID, null, saveFileNameStr.ToString()));
				saveCreateID = -1;
				screen = MainMenuScreen.Main;
				currentButton = 0;
			}
			if (InputManager.IsPressed("UIQuit", true))
			{
				screen = MainMenuScreen.Main;
				currentButton = 0;
				Audio.PlayBackground(UISound.uiBack);
			}
		}
		else if (screen == MainMenuScreen.CustomRunSettings)
		{
			if (key == KeyCode.Backspace && modifiers == KeyModifier.None && down && customRunSeedStr.Length > 0)
				customRunSeedStr.Remove(customRunSeedStr.Length - 1, 1);
			if (key == KeyCode.Return && modifiers == KeyModifier.None && down)
			{
				IvoryKeep.instance.pushState(new GameState(-1, customRunSeedStr.ToString(), true));
				screen = MainMenuScreen.Main;
				currentButton = 0;
			}
			if (InputManager.IsPressed("UIQuit", true))
			{
				screen = MainMenuScreen.Main;
				currentButton = 0;
				Audio.PlayBackground(UISound.uiBack);
			}
		}
		else if (screen == MainMenuScreen.Options)
		{
			SettingsUI.OnKeyEvent(key, modifiers, down);
		}
		else if (screen == MainMenuScreen.Credits)
		{
			if (InputManager.IsPressed("UIBack", true) || InputManager.IsPressed("UIQuit", true))
			{
				screen = MainMenuScreen.Main;
				currentButton = 0;
				Audio.PlayBackground(UISound.uiBack);
			}
		}
	}

	public override void onMouseButtonEvent(MouseButton button, bool down)
	{
		if (screen == MainMenuScreen.Options)
		{
			SettingsUI.OnMouseButtonEvent(button, down);
		}
	}

	public override void onGamepadButtonEvent(GamepadButton button, bool down)
	{
		if (screen == MainMenuScreen.Options)
		{
			SettingsUI.OnGamepadButtonEvent(button, down);
		}
	}

	public override void update()
	{
		particles.update();
	}

	public override void draw(GraphicsDevice graphics)
	{
		if (screen == MainMenuScreen.Main)
			mainScreen();
		else if (screen == MainMenuScreen.SaveSelect)
			saveSelect();
		else if (screen == MainMenuScreen.SaveCreate)
			saveCreate();
		else if (screen == MainMenuScreen.CustomRunSettings)
			customRun();
		else if (screen == MainMenuScreen.Options)
			options();
		else if (screen == MainMenuScreen.Credits)
			credits();

		float elapsed = (Time.timestamp - startTime) / 1e9f;
		if (elapsed < FADEIN)
		{
			float alpha = 1 - elapsed / FADEIN;
			uint color = Mathf.ColorAlpha(0xFF000000, alpha);
			Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, null, false, color);
		}

		particles.render();
	}
}
