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
	CustomRunSettings,
	Credits,
}

public class MainMenuState : State
{
	MainMenuScreen screen = MainMenuScreen.Main;
	int currentButton = 0;


	public MainMenuState()
	{
	}

	public override void update()
	{
	}

	void mainScreen()
	{
		string[] labels = [
			"Play",
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
			false,
			true,
			true
		];

		int selection = FullscreenMenu.Render(labels, enabled, ref currentButton);
		if (selection != -1)
		{
			switch (selection)
			{
				case 0: // Play
					PixelEngine.instance.pushState(new GameState(null));
					break;

				case 1: // Daily Run
					DateTime today = DateTime.Today;
					int day = today.DayOfYear;
					int year = today.Year;
					uint seed = Hash.combine(Hash.hash(day), Hash.hash(year));
					PixelEngine.instance.pushState(new GameState(seed.ToString()));
					break;

				case 2: // Custom Run
					screen = MainMenuScreen.CustomRunSettings;
					break;

				case 3: // Options

					break;

				case 4: // Credits
					screen = MainMenuScreen.Credits;
					break;

				case 5: // Quit
					PixelEngine.instance.popState();
					PixelEngine.instance.terminate();
					break;

				default:
					Debug.Assert(false);
					break;
			}
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

	void credits()
	{
		int x = 100;
		int y = 60;

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

		string title = "Test Title";
		Renderer.DrawUIText(Renderer.UIWidth / 2 - Renderer.MeasureUIText(title).x / 2, 30, title);

		drawLine("A Game by Scewps");
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
		if (screen == MainMenuScreen.CustomRunSettings)
		{
			if (key == KeyCode.Backspace && modifiers == KeyModifier.None && down && customRunSeedStr.Length > 0)
				customRunSeedStr.Remove(customRunSeedStr.Length - 1, 1);
			if (key == KeyCode.Return && modifiers == KeyModifier.None && down)
				PixelEngine.instance.pushState(new GameState(customRunSeedStr.ToString()));
			if (key == KeyCode.Esc && modifiers == KeyModifier.None && down)
				screen = MainMenuScreen.Main;
		}
		else if (screen == MainMenuScreen.Credits)
		{
			if ((key == KeyCode.Backspace || key == KeyCode.Q) && modifiers == KeyModifier.None && down)
				screen = MainMenuScreen.Main;
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		if (screen == MainMenuScreen.Main)
			mainScreen();
		else if (screen == MainMenuScreen.CustomRunSettings)
			customRun();
		else if (screen == MainMenuScreen.Credits)
			credits();
	}
}
