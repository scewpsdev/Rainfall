using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class HUD
{
	public const int Width = 200;

	public const uint ColorHealth = 0xFFb4202a;
	public const uint ColorMana = 0xFF113d8b;

	public static void Render()
	{
		Player player = GameState.instance.player;

		int height = Renderer.UIHeight;
		int x = Renderer.UIWidth - Width;
		int y = 0;

		int framePadding = 5;
		int frameHeight = height / 2;

		Renderer.DrawUISprite(x, y, Width, height, null, false, 0xFF000000);

		Renderer.DrawUISprite(x + framePadding, y + framePadding, Width - 2 * framePadding, frameHeight - 2 * framePadding, null, false, 0xFFAAAAAA);
		Renderer.DrawUISprite(x + framePadding + 1, y + framePadding + 1, Width - 2 * (framePadding + 1), frameHeight - 2 * (framePadding + 1), null, false, 0xFF000000);

		x += framePadding + 4;
		y += framePadding + 4;

		Renderer.DrawUITextBMP(x, y, "HP " + player.health + "/" + player.maxHealth, 1, ColorHealth);
		y += Renderer.smallFont.size + 1;

		Renderer.DrawUITextBMP(x, y, "MP " + player.mana + "/" + player.maxMana, 1, ColorMana);
		y += Renderer.smallFont.size + 1;

		x -= framePadding + 4;
		y = frameHeight - 4;

		Renderer.DrawUISprite(x + framePadding, y + framePadding, Width - 2 * framePadding, frameHeight - 2 * framePadding, null, false, 0xFFAAAAAA);
		Renderer.DrawUISprite(x + framePadding + 1, y + framePadding + 1, Width - 2 * (framePadding + 1), frameHeight - 2 * (framePadding + 1), null, false, 0xFF000000);

		x += framePadding + 4;
		y += framePadding + 4;

		int cellsX = 4;
		int cellsY = (player.hotbar.Length + cellsX - 1) / cellsX;

		for (int ycell = 0; ycell < cellsY; ycell++)
		{
			for (int xcell = 0; xcell < cellsX; xcell++)
			{
				int i = xcell + ycell * cellsX;
				if (i < player.hotbar.Length)
				{
					int slotSize = 16;
					int slotPadding = 6;

					DrawItemSlot(x + xcell * (slotSize + slotPadding), y + ycell * (slotSize + slotPadding), slotSize, player.hotbar[i]);
				}
			}
		}
	}

	static void DrawItemSlot(int x, int y, int size, Item item)
	{
		Renderer.DrawUISprite(x - 1, y - 1, size + 2, size + 2, null, false, 0xFFAAAAAA);
		Renderer.DrawUISprite(x, y, size, size, null, false, 0xFF000000);
		if (item != null)
		{
			Renderer.DrawUISprite(x, y, size, size, item.icon, false);
		}
	}
}
