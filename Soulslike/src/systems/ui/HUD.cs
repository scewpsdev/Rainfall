using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class HUD
{
	static Texture crosshair;
	static Texture crosshairHighlight;
	static Texture crosshairHand;

	static long lastPlayerHit = -1;

	static HUD()
	{
		crosshair = Resource.GetTexture("texture/ui/crosshair.png");
		crosshairHighlight = Resource.GetTexture("texture/ui/crosshair_highlight.png");
		crosshairHand = Resource.GetTexture("texture/ui/crosshair_hand.png");
	}

	public static void OnPlayerHit()
	{
		lastPlayerHit = Time.currentTime;
	}

	public static void Draw()
	{
		{ // crosshair
			int x = Display.width / 2 - crosshair.width / 2;
			int y = Display.height / 2 - crosshair.height / 2;

			if (GameState.instance.player.interactableInFocus != null)
			{
				GUI.Texture(x, y, crosshair.width, crosshair.height, crosshairHand);
			}
			else
			{
				GUI.Texture(x, y, crosshair);
			}
		}

		if (lastPlayerHit != -1)
		{
			float elapsed = (Time.currentTime - lastPlayerHit) / 1e9f;
			float anim = MathF.Exp(-elapsed);
			float falloff = MathHelper.Lerp(0.12f, 0.37f, anim);
			Vector3 color = Vector3.Lerp(Vector3.Zero, new Vector3(1, 0.3f, 0.3f), anim);
			GraphicsManager.vignetteFalloff = falloff;
			GraphicsManager.vignetteColor = color;
		}
		else
		{
			GraphicsManager.vignetteFalloff = 0.12f;
			GraphicsManager.vignetteColor = new Vector3(0.5f);
		}
	}
}
