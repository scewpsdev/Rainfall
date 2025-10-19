using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;


public class MapUI
{
	Player player;

	long lastToggle = -1;


	public MapUI(Player player)
	{
		this.player = player;
	}

	void openScreen()
	{
		player.mapOpen = true;
		player.numOverlaysOpen++;
		lastToggle = Time.currentTime;
	}

	void closeScreen()
	{
		player.mapOpen = false;
		player.numOverlaysOpen--;
		lastToggle = Time.currentTime;
	}

	public void render()
	{
		if (!player.mapOpen && player.numOverlaysOpen == 0 && InputManager.IsPressed("Map", true))
		{
			openScreen();
			Audio.PlayBackground(UISound.uiClick);
		}
		else if (player.mapOpen)
		{
			if (InputManager.IsPressed("Map", true) || InputManager.IsPressed("UIBack", true) || InputManager.IsPressed("UIQuit", true))
			{
				closeScreen();
				Audio.PlayBackground(UISound.uiBack);
			}
		}

		float openAnimDuration = 0.12f;
		float openProgress = player.mapOpen ? MathF.Min((Time.currentTime - lastToggle) / 1e9f / openAnimDuration, 1)
			: MathF.Max(1 - (Time.currentTime - lastToggle) / 1e9f / openAnimDuration, 0);

		// Minimap
		if (openProgress > 0)
		{
			Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, 0, null, MathHelper.ColorAlpha(0x7F000000, openProgress));

			if (player.level.minimap == null || Utils.RunEverySeconds(10, "updateMinimap"))
			{
				player.level.updateMinimap();
			}

			int border = 20;
			int x = (int)MathHelper.Remap(openProgress, 0, 1, Renderer.UIWidth / 2, border);
			int y = border;
			int width = Renderer.UIWidth - 2 * x;
			int height = Renderer.UIHeight - 2 * border;

			Vector2i playerTile = (Vector2i)Vector2.Floor(player.position + new Vector2(0, 0.5f));
			playerTile.y = player.level.minimap.height - playerTile.y - 1;
			int scrollx = playerTile.x - width / 2;
			int scrolly = playerTile.y - height / 2;

			Renderer.DrawUISprite(x, y, width, height, player.level.minimap, scrollx, scrolly, width, height);
			Renderer.DrawUISprite(x + playerTile.x - scrollx, y + playerTile.y - scrolly, 1, 1, null, false, 0xFF00FF00);

			Renderer.DrawUISprite(x - 4, y - 1, 4, height + 2, null, false, 0xFF7F7F7F);
			Renderer.DrawUISprite(x - 3, y - 2, 2, height + 4, null, false, 0xFF7F7F7F);

			Renderer.DrawUISprite(x + width, y - 1, 4, height + 2, null, false, 0xFF7F7F7F);
			Renderer.DrawUISprite(x + width + 1, y - 2, 2, height + 4, null, false, 0xFF7F7F7F);
		}
	}
}
