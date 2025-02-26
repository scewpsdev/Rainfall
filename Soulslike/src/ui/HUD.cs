using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class HUD
{
	static Texture crosshair;
	static Texture crosshairHighlight;

	static HUD()
	{
		crosshair = Resource.GetTexture("texture/ui/crosshair.png");
		crosshairHighlight = Resource.GetTexture("texture/ui/crosshair_highlight.png");
	}

	public static void Draw()
	{
		{ // crosshair
			int x = Display.width / 2 - crosshair.width / 2;
			int y = Display.height / 2 - crosshair.height / 2;
			GUI.Texture(x, y, crosshair);

			if (GameState.instance.player.interactableInFocus != null)
			{
				GUI.Texture(x, y, crosshairHighlight);
			}
		}
	}
}
