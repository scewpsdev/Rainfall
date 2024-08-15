using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


public static class GameOverScreen
{
	static string TimeToString(float time)
	{
		int millis = (int)(time * 1000) % 1000;
		int seconds = (int)time % 60;
		int minutes = (int)time / 60 % 60;
		int hours = (int)time / 60 / 60;

		string result = hours.ToString() + ":" + minutes.ToString("00") + ":" + seconds.ToString("00") + "." + millis.ToString("000") + " s";
		return result;
	}

	static void RenderRunStats(GameState game, int x, int y, int width, int height)
	{
		y += 10;

		{
			string caption = "YOU DIED";
			int captionWidth = Renderer.MeasureUIText(caption, caption.Length, 1).x;
			Renderer.DrawUIText(x + width / 2 - captionWidth / 2, y, caption, 1, 0xFFAA3333);
			y += 7;
		}

		y += 17;

		int lineHeight = 8 + 4;

		void drawLeft(string str, uint color = 0xFFAAAAAA)
		{
			if (str == null)
				str = "???";
			Renderer.DrawUITextBMP(x, y, str, 1, color);
		}
		void drawRight(string str, uint color = 0xFFAAAAAA)
		{
			if (str == null)
				str = "???";
			int textWidth = Renderer.MeasureUITextBMP(str, str.Length, 1).x;
			Renderer.DrawUITextBMP(x + width - textWidth, y, str, 1, color);
		}

		drawLeft("Score");
		drawRight(game.run.score.ToString());
		y += lineHeight;

		drawLeft("Time");
		drawRight(TimeToString(game.run.duration));
		y += lineHeight;

		drawLeft("Seed");
		drawRight(game.run.seed.ToString());
		y += lineHeight;

		/*
		drawLeft("Name");
		//drawRight(game.player.name);
		drawRight("Bob");
		y += lineHeight;

		drawLeft("Class");
		//drawRight(game.player.playerClass != null ? game.player.playerClass.type.ToString() : "???");
		drawRight("Knight");
		y += lineHeight;

		drawLeft("Level");
		//drawRight(game.player.stats.level.ToString());
		drawRight("69");
		y += lineHeight;

		drawLeft("Gold");
		//drawRight(game.player.inventory.numGold.ToString());
		drawRight("broke");
		y += lineHeight;
		*/

		drawLeft("Killed by ");
		drawRight(game.run.killedBy != null ? game.run.killedBy.displayName : "The Void");
		y += lineHeight;

		drawLeft("Floor");
		drawRight((game.run.floor + 1).ToString());
		y += lineHeight;

		drawLeft("Enemies killed");
		drawRight(game.run.kills.ToString());
		y += lineHeight;

		drawLeft("Steps taken");
		drawRight(game.run.stepsTaken.ToString());
		y += lineHeight;

		/*
		drawLeft("Airborne time");
		drawRight("1");
		y += lineHeight;

		drawLeft("Cookies eaten");
		drawRight("0");
		y += lineHeight;
		*/
	}

	public static void Render()
	{
		int x = 16;
		int y = 16;
		int width = Renderer.UIWidth - 2 * 16;
		int height = Renderer.UIHeight - 2 * 16;
		Renderer.DrawUISprite(x, y, width, height, null, false, 0xFF111111);

		int padding = 8;

		RenderRunStats(GameState.instance, x + padding, y + padding, width / 2 - 2 * padding, height - 2 * padding);
		InventoryUI.DrawInventory(x + width / 2 + padding, y + padding, width / 2 - 2 * padding, height - 2 * padding, GameState.instance.player);
	}
}
