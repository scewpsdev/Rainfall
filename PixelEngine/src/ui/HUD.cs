using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HUD
{
	Player player;

	Sprite gemSprite;


	public HUD(Player player)
	{
		this.player = player;

		gemSprite = new Sprite(Item.tileset, 3, 0);
	}

	public void render()
	{
		if (player.inventoryOpen)
			return;

		for (int i = 0; i < player.maxHealth; i++)
		{
			int size = 24;
			int padding = 8;
			int x = 20 + i * (size + padding);
			int y = 20;
			if (i < player.health)
				Renderer.DrawUISprite(x, y, size, size, null, 0, 0, 0, 0, 0xFFFF7777);
			else
				Renderer.DrawUISprite(x, y, size, size, null, 0, 0, 0, 0, 0xFF777777);
		}

		{ // Gems
			int size = 48;
			int x = 18;
			int y = 20 + 24 + 8;

			Renderer.DrawUISprite(x, y, size, size, gemSprite, false);
			Renderer.DrawUIText(x + size, y, player.money.ToString(), 3);
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
