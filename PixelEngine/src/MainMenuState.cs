using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;


enum MainMenuScreen
{
	Main,
	CustomRunSettings,
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
			"About",
			"Quit"
		];

		int linePadding = 3;

		if (InputManager.IsPressed("Down"))
			currentButton = (currentButton + 1) % labels.Length;
		if (InputManager.IsPressed("Up"))
			currentButton = (currentButton + labels.Length - 1) % labels.Length;

		for (int i = 0; i < labels.Length; i++)
		{
			string txt = labels[i];
			Vector2i size = Renderer.MeasureUITextBMP(txt, txt.Length, 1);
			uint color = i == currentButton ? 0xFFFFFFFF : 0xFF666666;
			Renderer.DrawUITextBMP(Renderer.UIWidth / 2 - size.x / 2, Renderer.UIHeight / 2 - size.y / 2 + i * (size.y + linePadding), txt, 1, color);

			if (i == currentButton && InputManager.IsPressed("Interact"))
			{
				switch (i)
				{
					case 0: // Play
						PixelEngine.instance.pushState(new GameState(0));
						break;

					case 1: // Daily Run
						DateTime today = DateTime.Today;
						int day = today.DayOfYear;
						int year = today.Year;
						uint seed = Hash.combine(Hash.hash(day), Hash.hash(year));
						PixelEngine.instance.pushState(new GameState(seed));
						break;

					case 2: // Custom Run
						screen = MainMenuScreen.CustomRunSettings;
						break;

					case 3: // Options

						break;

					case 4: // About

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
				PixelEngine.instance.pushState(new GameState(Hash.hash(customRunSeedStr.ToString())));
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		if (screen == MainMenuScreen.Main)
			mainScreen();
		else if (screen == MainMenuScreen.CustomRunSettings)
			customRun();
	}
}
