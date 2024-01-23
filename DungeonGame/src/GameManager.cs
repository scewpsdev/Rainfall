using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GameManager
{
	public Player player;
	Creature currentBoss;

	public bool mapUnlocked = false;


	public GameManager()
	{
	}

	public void initiateBoss(Creature boss)
	{
		currentBoss = boss;
	}

	public void terminateBoss()
	{
		currentBoss = null;
	}

	public void update()
	{
		if (currentBoss != null)
		{
			if (currentBoss.isDead)
			{
				player.wonTime = Time.currentTime;
				terminateBoss();
			}
			else if (!player.isAlive)
			{
				currentBoss.stats.health = currentBoss.stats.maxHealth;
				terminateBoss();
			}
		}

		mapUnlocked = player.inventory.hasItemEquipped(Item.Get("map"));
	}

	public void draw()
	{
		if (currentBoss != null)
		{
			int width = Display.width / 8 * 7;
			int height = 16;
			Renderer.DrawUIRect(Display.width / 2 - width / 2 - 2, Display.height - 128 - 2, width + 4, height + 4, 0xFF111111);
			Renderer.DrawUIRect(Display.width / 2 - width / 2, Display.height - 128, (int)(width * currentBoss.stats.health / (float)currentBoss.stats.maxHealth), height, 0xFFFF1111);
			Renderer.DrawText(Display.width / 2 - width / 2, Display.height - 128 - (int)Renderer.promptFont.size - 8, 1, currentBoss.name, Renderer.promptFont, 0xFFAAAAAA);
		}
		if (player.hasWon)
		{
			float timeSinceWin = (Time.currentTime - player.wonTime) / 1e9f;
			float intensity = timeSinceWin < 1 ? timeSinceWin : timeSinceWin > 4 && timeSinceWin < 5 ? 1 - (timeSinceWin - 4) : timeSinceWin >= 1 && timeSinceWin <= 4 ? 1 : 0;
			if (intensity > 0)
			{
				string text = "V I C T O R Y";
				uint color = (uint)(((byte)(intensity * 255) << 24) | 0xCCAA66);
				int width = Renderer.victoryFont.measureText(text);
				Renderer.DrawText(Display.viewportSize.x / 2 - width / 2, Display.viewportSize.y / 2 - (int)Renderer.victoryFont.size / 2, 1.0f, text, Renderer.victoryFont, color);
			}
		}
	}
}
