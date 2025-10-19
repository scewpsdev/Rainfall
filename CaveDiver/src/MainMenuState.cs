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
	CustomRunSettings,
	Options,
	Credits,
}

public class MainMenuState : State
{
	MainMenuScreen screen = MainMenuScreen.Main;
	int currentButton = 0;

	Sprite splash, splashSmall;


	public MainMenuState()
	{
		splash = new Sprite(Resource.GetTexture("sprites/splash.png", false), 0, 0, 256, 64);
		splashSmall = new Sprite(Resource.GetTexture("sprites/splash.png", false), 0, 64, 256, 32);
	}

	public override void update()
	{
	}

	void mainScreen()
	{
		string[] labels = [
			"Start",
			"Options",
			"Credits",
			"Quit"
		];

		bool[] enabled = [
			true,
			true,
			true,
			true
		];

		int selection = FullscreenMenu.Render(labels, enabled, ref currentButton);
		if (selection != -1)
		{
			switch (selection)
			{
				case 0: // Start
					screen = MainMenuScreen.Main;
					CaveDiver.instance.pushState(new GameState(0, null));
					currentButton = 0;
					break;

				case 1: // Options
					screen = MainMenuScreen.Options;
					OptionsMenu.OnOpen();
					break;

				case 2: // Credits
					screen = MainMenuScreen.Credits;
					break;

				case 3: // Quit
					CaveDiver.instance.popState();
					CaveDiver.instance.terminate();
					break;

				default:
					Debug.Assert(false);
					break;
			}
		}
	}

	void saveSelect()
	{
		string[] labels = [
			"File 1",
			"File 2",
			"File 3",
		];
		bool[] enabled = [true, true, true];

		int selection = FullscreenMenu.Render(labels, enabled, ref currentButton);
		if (selection != -1)
		{
			screen = MainMenuScreen.Main;
			CaveDiver.instance.pushState(new GameState(selection, null));
		}
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
		if (!OptionsMenu.Render())
			screen = MainMenuScreen.Main;
	}

	void credits()
	{
		//Renderer.DrawUISprite(Renderer.UIWidth / 2 - splashSmall.width / 2, 10, splashSmall.width, splashSmall.height, 0, splashSmall, 0xFF888888);

		int x = 100;
		int y = 55;

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
		drawLine("Art and music");
		back();
		drawLineRight("Scewps");
		drawLine("");
		drawLine("Tools:");
		back();
		drawLineRight("C++");
		drawLineRight("GLFW");
		drawLineRight("BGFX");
		drawLineRight("Soloud");
		drawLine("");
		drawLine("Software");
		back();
		drawLineRight("Visual Studio");
		drawLineRight("Aseprite");
		drawLineRight("Reaper");
		drawLineRight("Audacity");
	}

	public override void onCharEvent(byte length, uint value)
	{
		if (screen == MainMenuScreen.CustomRunSettings)
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
				Audio.PlayBackground(UISound.uiBack);
			}
		}
		else if (screen == MainMenuScreen.CustomRunSettings)
		{
			if (key == KeyCode.Backspace && modifiers == KeyModifier.None && down && customRunSeedStr.Length > 0)
				customRunSeedStr.Remove(customRunSeedStr.Length - 1, 1);
			if (key == KeyCode.Return && modifiers == KeyModifier.None && down)
			{
				CaveDiver.instance.pushState(new GameState(-1, customRunSeedStr.ToString(), true));
				screen = MainMenuScreen.Main;
			}
			if (InputManager.IsPressed("UIQuit", true))
			{
				screen = MainMenuScreen.Main;
				Audio.PlayBackground(UISound.uiBack);
			}
		}
		else if (screen == MainMenuScreen.Options)
		{
			OptionsMenu.OnKeyEvent(key, modifiers, down);
		}
		else if (screen == MainMenuScreen.Credits)
		{
			if (InputManager.IsPressed("UIBack", true) || InputManager.IsPressed("UIQuit", true))
			{
				screen = MainMenuScreen.Main;
				Audio.PlayBackground(UISound.uiBack);
			}
		}
	}

	public override void onMouseButtonEvent(MouseButton button, bool down)
	{
		if (screen == MainMenuScreen.Options)
		{
			OptionsMenu.OnMouseButtonEvent(button, down);
		}
	}

	public override void onGamepadButtonEvent(GamepadButton button, bool down)
	{
		if (screen == MainMenuScreen.Options)
		{
			OptionsMenu.OnGamepadButtonEvent(button, down);
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		if (screen == MainMenuScreen.Main)
			mainScreen();
		else if (screen == MainMenuScreen.SaveSelect)
			saveSelect();
		else if (screen == MainMenuScreen.CustomRunSettings)
			customRun();
		else if (screen == MainMenuScreen.Options)
			options();
		else if (screen == MainMenuScreen.Credits)
			credits();
	}
}
