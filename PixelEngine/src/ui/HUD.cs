using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HUD
{
	Player player;


	public HUD(Player player)
	{
		this.player = player;
	}

	public void render()
	{
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
	}
}
