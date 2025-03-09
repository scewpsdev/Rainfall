using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class HUD
{
	public const int Width = 100;

	public static void Render()
	{
		Player player = GameState.instance.player;

		int height = Renderer.UIHeight;
		int x = Renderer.UIWidth - Width;
		int y = 0;

		Renderer.DrawUISprite(x, y, Width, height, null, false, 0xFF000000);
		Renderer.DrawUISprite(x + 1, y + 1, Width - 2, height - 2, null, false, 0xFFCCCCCC);
		Renderer.DrawUISprite(x + 2, y + 2, Width - 4, height - 4, null, false, 0xFF333333);

		x += 5;
		y += 5;

		Renderer.DrawUITextBMP(x, y, "HP: " + player.health + "/" + player.maxHealth);
		y += Renderer.smallFont.size + 1;

		Renderer.DrawUITextBMP(x, y, "MP: " + player.mana + "/" + player.maxMana);
		y += Renderer.smallFont.size + 1;
	}
}
