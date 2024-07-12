using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HUD
{
	Player player;

	SpriteSheet tileset;

	Sprite heartFull, heartHalf, heartEmpty;
	Sprite gems;


	public HUD(Player player)
	{
		this.player = player;

		tileset = new SpriteSheet(Resource.GetTexture("res/sprites/ui.png", false), 8, 8);

		heartFull = new Sprite(tileset, 0, 0);
		heartHalf = new Sprite(tileset, 1, 0);
		heartEmpty = new Sprite(tileset, 2, 0);

		gems = new Sprite(tileset, 3, 0);
	}

	public void render()
	{
		if (player.inventoryOpen)
			return;

		// Health
		for (int i = 0; i < (int)MathF.Ceiling(player.maxHealth / 2.0f); i++)
		{
			int size = 24;
			int padding = 8;
			int x = 20 + i * (size + padding);
			int y = 20;

			if (i < player.health / 2)
				Renderer.DrawUISprite(x, y, size, size, heartFull);
			else if (i == player.health / 2 && player.health % 2 == 1)
				Renderer.DrawUISprite(x, y, size, size, heartHalf);
			else
				Renderer.DrawUISprite(x, y, size, size, heartEmpty);
		}

		{ // Gems
			int size = 24;
			int x = 20;
			int y = 20 + 24 + 8;

			Renderer.DrawUISprite(x, y, size, size, gems, false);
			Renderer.DrawUIText(x + size + 8, y, player.money.ToString(), 2);
		}

		{ // Hand item
			int size = 64;
			int x = 40;
			int y = Display.height - 40 - size;

			Renderer.DrawUISprite(x, y, size, size, null, 0, 0, 0, 0, 0xFF111111);
			if (player.handItem != null)
				Renderer.DrawUISprite(x, y, size, size, player.handItem.sprite);
		}

		{ // Quick item
			int size = 48;
			int x = 124;
			int y = Display.height - 40 - size;

			Renderer.DrawUISprite(x, y, size, size, null, 0, 0, 0, 0, 0xFF111111);
			Renderer.DrawUIText(x + size - size / 4, y, (player.currentQuickItem + 1).ToString(), 2, 0xFFBBBBBB);
			if (player.quickItems[player.currentQuickItem] != null)
				Renderer.DrawUISprite(x, y, size, size, player.quickItems[player.currentQuickItem].sprite);
		}
	}
}
