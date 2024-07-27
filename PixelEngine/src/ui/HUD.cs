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
		for (int i = 0; i < player.maxHealth; i++)
		{
			int size = 24;
			int padding = 8;
			int x = 20 + i * (size + padding);
			int y = 20;

			Renderer.DrawUISprite(x, y, size, size, heartEmpty);
			if (i < player.health)
			{
				float fraction = MathF.Min(player.health - i, 1);
				fraction = MathF.Floor(fraction * 7) / 8.0f + 0.125f;
				//Renderer.DrawUISprite(x, y, size, size, heartFull);
				Renderer.DrawUISprite(x, y + (int)((1 - fraction) * size), size, (int)(fraction * size), heartFull.spriteSheet.texture, heartFull.position.x, heartFull.position.y + (int)(heartFull.size.y * (1 - fraction)), heartFull.size.x, (int)(heartFull.size.y * fraction));
			}
			//else if (i == player.health / 2 && player.health % 2 == 1)
			//	Renderer.DrawUISprite(x, y, size, size, heartHalf);
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
			if (player.quickItems[player.currentQuickItem] != null)
			{
				Renderer.DrawUISprite(x, y, size, size, player.quickItems[player.currentQuickItem].sprite);
				if (player.quickItems[player.currentQuickItem].stackable && player.quickItems[player.currentQuickItem].stackSize > 1)
					Renderer.DrawUIText(x + size - size / 4, y + size - 22, player.quickItems[player.currentQuickItem].stackSize.ToString(), 2, 0xFFBBBBBB);
			}
		}
	}
}
