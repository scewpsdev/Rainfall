using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;


public class MainMenuState : State
{
	int currentButton = 0;


	public MainMenuState()
	{
	}

	public override void update()
	{
	}

	public override void draw(GraphicsDevice graphics)
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

		if (Input.IsKeyPressed(KeyCode.Down))
			currentButton = (currentButton + 1) % labels.Length;
		if (Input.IsKeyPressed(KeyCode.Up))
			currentButton = (currentButton + labels.Length - 1) % labels.Length;

		for (int i = 0; i < labels.Length; i++)
		{
			string txt = labels[i];
			Vector2i size = Renderer.MeasureUITextBMP(txt, txt.Length, 1);
			uint color = i == currentButton ? 0xFFFFFFFF : 0xFF666666;
			Renderer.DrawUITextBMP(Renderer.UIWidth / 2 - size.x / 2, Renderer.UIHeight / 2 - size.y / 2 + i * (size.y + linePadding), txt, 1, color);

			if (i == currentButton && Input.IsKeyPressed(KeyCode.X))
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
}
