using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class GameOverScreen
{
	static UIParticleEffect scoreRecordParticles;
	static UIParticleEffect floorRecordParticles;
	static UIParticleEffect timeRecordParticles;
	static UIParticleEffect killRecordParticles;
	static bool particlesEmitted;

	public static void Init()
	{
		particlesEmitted = false;
	}

	public static void Destroy()
	{
		scoreRecordParticles?.remove();
		scoreRecordParticles = null;
		floorRecordParticles?.remove();
		floorRecordParticles = null;
		timeRecordParticles?.remove();
		timeRecordParticles = null;
		killRecordParticles?.remove();
		killRecordParticles = null;
	}

	static void RenderRunStats(GameState game, int x, int y, int width, int height)
	{
		y += 10;

		{
			string caption = game.run.hasWon ? "VICTORY" : "YOU DIED";
			uint color = game.run.hasWon ? 0xFFCCAA66 : 0xFFAA3333;
			int captionWidth = Renderer.MeasureUIText(caption, caption.Length, 1).x;
			Renderer.DrawUIText(x + width / 2 - captionWidth / 2, y, caption, 1, color);
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
		drawRight(game.run.score.ToString(), game.run.scoreRecord ? RunStats.recordColors[0] : 0xFFAAAAAA);
		if (game.run.scoreRecord && scoreRecordParticles == null && !particlesEmitted)
		{
			GameState.instance.level.addEntity(scoreRecordParticles = Effects.CreateRecordUIEffect(RunStats.recordColors[0]), new Vector2(x + width - 8, y + 4));
			scoreRecordParticles.removeCallbacks.Add(() =>
			{
				scoreRecordParticles = null;
			});
		}
		y += lineHeight;

		drawLeft("Floor");
		drawRight((game.run.floor + 1).ToString(), game.run.floorRecord ? RunStats.recordColors[1] : 0xFFAAAAAA);
		if (game.run.floorRecord && floorRecordParticles == null && !particlesEmitted)
		{
			GameState.instance.level.addEntity(floorRecordParticles = Effects.CreateRecordUIEffect(RunStats.recordColors[1]), new Vector2(x + width - 8, y + 4));
			floorRecordParticles.removeCallbacks.Add(() => { floorRecordParticles = null; });
		}
		y += lineHeight;

		drawLeft("Time");
		drawRight(StringUtils.TimeToString(game.run.duration), game.run.timeRecord ? RunStats.recordColors[2] : 0xFFAAAAAA);
		if (game.run.timeRecord && timeRecordParticles == null && !particlesEmitted)
		{
			GameState.instance.level.addEntity(timeRecordParticles = Effects.CreateRecordUIEffect(RunStats.recordColors[2]), new Vector2(x + width - 8, y + 4));
			timeRecordParticles.removeCallbacks.Add(() => { timeRecordParticles = null; });
		}
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

		drawLeft("Enemies killed");
		drawRight(game.run.kills.ToString(), game.run.killRecord ? RunStats.recordColors[3] : 0xFFAAAAAA);
		if (game.run.killRecord && killRecordParticles == null && !particlesEmitted)
		{
			GameState.instance.level.addEntity(killRecordParticles = Effects.CreateRecordUIEffect(RunStats.recordColors[3]), new Vector2(x + width - 8, y + 4));
			killRecordParticles.removeCallbacks.Add(() => { killRecordParticles = null; });
		}
		y += lineHeight;

		y += lineHeight;

		drawLeft("Killed by ");
		drawRight(game.run.hasWon ? "---" : game.run.killedByName != null ? game.run.killedByName : "The Void");
		y += lineHeight;

		drawLeft("Hits taken");
		drawRight(game.run.hitsTaken.ToString());
		y += lineHeight;

		drawLeft("Steps walked");
		drawRight(game.run.stepsWalked.ToString());
		y += lineHeight;

		drawLeft("Seed");
		drawRight(game.run.seed.ToString());
		y += lineHeight;

		/*
		drawLeft("Airborne time");
		drawRight("1");
		y += lineHeight;

		drawLeft("Cookies eaten");
		drawRight("0");
		y += lineHeight;
		*/

		particlesEmitted = true;
	}

	static void RenderPlayer(int x, int y, int width, int height)
	{
		Player player = GameState.instance.player;

		int size = 16;
		int xx = x + width / 2 - size / 2;
		int yy = y + size * 3 / 4;
		Renderer.DrawUISprite(xx - size / 2, yy - size / 2, size * 2, size * 2, null, false, 0xFF050505);

		if (player.offhandItem != null)
		{
			int w = (int)MathF.Round(player.offhandItem.size.x * size);
			int h = (int)MathF.Round(player.offhandItem.size.y * size);
			Renderer.DrawUISprite(xx - (w - size) / 2 + (int)(player.getWeaponOrigin(false).x * size + player.offhandItem.renderOffset.x * size), yy + size / 2 - (h - size) - (int)(player.getWeaponOrigin(false).y * size + player.offhandItem.renderOffset.y * size), w, h, player.offhandItem.sprite);
		}

		player.animator.setAnimation("idle");
		player.animator.update(player.sprite);
		Renderer.DrawUISprite(xx, yy, size, size, player.sprite);

		for (int i = 0; i < player.passiveItems.Count; i++)
		{
			if (player.passiveItems[i] != null && player.passiveItems[i].ingameSprite != null)
			{
				int ss = size * player.passiveItems[i].ingameSpriteSize;
				player.animator.update(player.passiveItems[i].ingameSprite);
				player.passiveItems[i].ingameSprite.position *= player.passiveItems[i].ingameSpriteSize;
				Renderer.DrawUISprite(xx - (ss - size) / 2, yy - (ss - size), ss, ss, player.passiveItems[i].ingameSprite, false, MathHelper.VectorToARGB(player.passiveItems[i].ingameSpriteColor));
			}
		}

		if (player.handItem != null)
		{
			int w = (int)MathF.Round(player.handItem.size.x * size);
			int h = (int)MathF.Round(player.handItem.size.y * size);
			Renderer.DrawUISprite(xx - (w - size) / 2 + (int)(player.getWeaponOrigin(true).x * size + player.handItem.renderOffset.x * size), yy + size / 2 - (h - size) - (int)(player.getWeaponOrigin(true).y * size + player.handItem.renderOffset.y * size), w, h, player.handItem.sprite);
		}
	}

	public static void Render()
	{
		Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, null, false, 0x7F000000);

		int x = 16;
		int y = 16;
		int width = Renderer.UIWidth - 2 * 16;
		int height = Renderer.UIHeight - 2 * 16;
		Renderer.DrawUISprite(x, y, width, height, null, false, 0xFF555555);
		Renderer.DrawUISprite(x + 2, y + 2, width / 2 - 4, height - 4, null, false, 0xFF111111);
		Renderer.DrawUISprite(x + width / 2, y + 2, width / 2 - 2, height - 4, null, false, 0xFF111111);

		int padding = 8;

		int playerViewHeight = 32; // (height - 2 * padding) * 2 / 8;

		RenderRunStats(GameState.instance, x + padding, y + padding, width / 2 - 2 * padding, height - 2 * padding);
		RenderPlayer(x + width / 2 + padding, y + padding, width / 2 - 2 * padding, playerViewHeight);
		InventoryUI.DrawEquipment(x + width / 2 + padding, y + padding + playerViewHeight, width / 2 - 2 * padding, (height - 2 * padding) - playerViewHeight, GameState.instance.player);

		string prompt1 = InputManager.GetBinding("UIConfirm").ToString() + " to quick restart";
		Renderer.DrawUITextBMP(x + width / 2 + width / 4 - Renderer.MeasureUITextBMP(prompt1).x / 2, y + height - padding - 12 - Renderer.MeasureUITextBMP(prompt1).y, prompt1);

		string prompt2 = InputManager.GetBinding("UIConfirm2").ToString() + " to return to hub";
		Renderer.DrawUITextBMP(x + width / 2 + width / 4 - Renderer.MeasureUITextBMP(prompt2).x / 2, y + height - padding - Renderer.MeasureUITextBMP(prompt2).y, prompt2);

		scoreRecordParticles?.update();
		floorRecordParticles?.update();
		timeRecordParticles?.update();
		killRecordParticles?.update();

		scoreRecordParticles?.render();
		floorRecordParticles?.render();
		timeRecordParticles?.render();
		killRecordParticles?.render();
	}
}
