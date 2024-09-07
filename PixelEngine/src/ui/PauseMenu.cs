using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class PauseMenu
{
	static int currentButton = 0;


	public static void OnPause()
	{
		currentButton = 0;
	}

	public static void OnUnpause()
	{
	}

	public static void Render(GameState game)
	{
		Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, null, false, 0x7F000000);

		string[] labels = [
			"Resume",
			"Options",
			"Quit"
		];

		bool[] enabled = [
			true,
			false,
			true
		];

		int selection = FullscreenMenu.Render(labels, enabled, ref currentButton);
		if (selection != -1)
		{
			switch (selection)
			{
				case 0: // Resume
					game.isPaused = false;
					OnUnpause();
					break;

				case 1: // Options
					break;

				case 2: // Quit
					PixelEngine.instance.popState();
					break;

				default:
					Debug.Assert(false);
					break;
			}
		}
	}
}
