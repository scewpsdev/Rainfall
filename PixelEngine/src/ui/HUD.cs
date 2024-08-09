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
	Sprite armor, armorEmpty;
	Sprite gems;


	public HUD(Player player)
	{
		this.player = player;

		tileset = new SpriteSheet(Resource.GetTexture("res/sprites/ui.png", false), 8, 8);

		heartFull = new Sprite(tileset, 0, 0);
		heartHalf = new Sprite(tileset, 1, 0);
		heartEmpty = new Sprite(tileset, 2, 0);

		armor = new Sprite(tileset, 4, 0);
		armorEmpty = new Sprite(tileset, 5, 0);

		gems = new Sprite(tileset, 3, 0);
	}

	public void render()
	{
		if (player.inventoryOpen)
			return;

		// Health
		for (int i = 0; i < player.maxHealth; i++)
		{
			int size = 8;
			int padding = 3;
			int x = 6 + i * (size + padding);
			int y = 6;

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

		// Armor
		int totalArmor = player.getTotalArmor();
		for (int i = 0; i < (int)MathF.Ceiling(totalArmor / 10.0f); i++)
		{
			int size = 8;
			int padding = 3;
			int x = 6 + i * (size + padding);
			int y = 6 + 8 + 3;

			Renderer.DrawUISprite(x, y, size, size, armorEmpty);
			float fraction = MathF.Min(totalArmor / 10.0f - i, 1);
			fraction = MathF.Floor(fraction * 7) / 8.0f + 0.125f;
			Renderer.DrawUISprite(x, y + (int)((1 - fraction) * size), size, (int)(fraction * size), armor.spriteSheet.texture, armor.position.x, armor.position.y + (int)(armor.size.y * (1 - fraction)), armor.size.x, (int)(armor.size.y * fraction));
		}

		{ // Gems
			int size = 8;
			int x = 6;
			int y = 6 + 8 + 3 + 8 + 3;

			Renderer.DrawUISprite(x, y, size, size, gems, false);
			Renderer.DrawUITextBMP(x + size + 3, y, player.money.ToString(), 1);
		}

		{ // Hand item
			int size = 16;
			int x = 12;
			int y = Renderer.UIHeight - 12 - size;

			Renderer.DrawUISprite(x, y, size, size, null, 0, 0, 0, 0, 0xFF111111);
			if (player.handItem != null)
				Renderer.DrawUISprite(x, y, size, size, player.handItem.sprite);
		}

		{ // Quick item
			int size = 16;
			int x = 16 + 16 + 4;
			int y = Renderer.UIHeight - 12 - size;

			Renderer.DrawUISprite(x, y, size, size, null, 0, 0, 0, 0, 0xFF111111);
			if (player.quickItems[player.currentQuickItem] != null)
			{
				Renderer.DrawUISprite(x, y, size, size, player.quickItems[player.currentQuickItem].sprite);
				if (player.quickItems[player.currentQuickItem].stackable && player.quickItems[player.currentQuickItem].stackSize > 1)
					Renderer.DrawUIText(x + size - size / 4, y + size - 22, player.quickItems[player.currentQuickItem].stackSize.ToString(), 1, 0xFFBBBBBB);
			}
		}
	}
}
