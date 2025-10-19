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
			int captionWidth = Renderer.MeasureUITextBMP(caption, caption.Length, 1).x;
			Renderer.DrawUITextBMP(x + width / 2 - captionWidth / 2, y, caption, 1, color);
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
		drawRight(game.run.score.ToString(), game.run.scoreRecord ? RunStats.recordColors[1] : 0xFFAAAAAA);
		if (game.run.scoreRecord && scoreRecordParticles == null && !particlesEmitted)
		{
			GameState.instance.level.addEntity(scoreRecordParticles = ParticleEffects.CreateRecordUIEffect(RunStats.recordColors[1]), new Vector2(x + width - 8, y + 4));
			scoreRecordParticles.removeCallbacks.Add(() =>
			{
				scoreRecordParticles = null;
			});
		}
		y += lineHeight;

		drawLeft("Time");
		drawRight(StringUtils.TimeToString(game.run.duration), game.run.timeRecord ? RunStats.recordColors[0] : 0xFFAAAAAA);
		if (game.run.timeRecord && timeRecordParticles == null && !particlesEmitted)
		{
			GameState.instance.level.addEntity(timeRecordParticles = ParticleEffects.CreateRecordUIEffect(RunStats.recordColors[0]), new Vector2(x + width - 8, y + 4));
			timeRecordParticles.removeCallbacks.Add(() => { timeRecordParticles = null; });
		}
		y += lineHeight;

		drawLeft("Enemies killed");
		drawRight(game.run.kills.ToString(), game.run.killRecord ? RunStats.recordColors[3] : 0xFFAAAAAA);
		if (game.run.killRecord && killRecordParticles == null && !particlesEmitted)
		{
			GameState.instance.level.addEntity(killRecordParticles = ParticleEffects.CreateRecordUIEffect(RunStats.recordColors[3]), new Vector2(x + width - 8, y + 4));
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

		particlesEmitted = true;
	}

	public static void Render()
	{
		Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, null, false, 0x7F000000);
		//Renderer.DrawUISprite(0, 0, Renderer.UIWidth, Renderer.UIHeight, null, false, 0xFF000000);

		int x = 16;
		int y = 16;
		int width = Renderer.UIWidth - 2 * 16;
		int height = Renderer.UIHeight - 2 * 16;
		//Renderer.DrawUISprite(x, y, width, height, null, false, 0xFF555555);
		Renderer.DrawUISprite(x + 2, y + 2, width / 2 - 4, height - 4, null, false, 0xFF111111);
		Renderer.DrawUISprite(x + width / 2, y + 2, width / 2 - 2, height - 4, null, false, 0xFF111111);

		int padding = 8;

		int playerViewSize = 32;
		PlayerThumbnail.Render(x + width / 4 - playerViewSize / 2, y + padding, playerViewSize, playerViewSize);

		RenderRunStats(GameState.instance, x + padding, y + padding + playerViewSize, width / 2 - 2 * padding, height - 2 * padding);
		Vector2i selectedCell = Vector2i.Zero;
		InventoryUI.DrawEquipment3(x + width / 2 + padding, y + padding, width / 2 - 2 * padding, (height - 2 * padding) - playerViewSize, GameState.instance.player, ref selectedCell, out _);

		string prompt1 = InputManager.GetBinding("UIConfirm").ToString() + " to restart";
		Renderer.DrawUITextBMP(x + width / 2 + width / 4 - Renderer.MeasureUITextBMP(prompt1).x / 2, y + height - padding - 12 - Renderer.MeasureUITextBMP(prompt1).y, prompt1);

		//string prompt2 = InputManager.GetBinding("UIConfirm2").ToString() + " to return to hub";
		//Renderer.DrawUITextBMP(x + width / 2 + width / 4 - Renderer.MeasureUITextBMP(prompt2).x / 2, y + height - padding - Renderer.MeasureUITextBMP(prompt2).y, prompt2);

		scoreRecordParticles?.update();
		floorRecordParticles?.update();
		timeRecordParticles?.update();
		killRecordParticles?.update();

		scoreRecordParticles?.render();
		floorRecordParticles?.render();
		timeRecordParticles?.render();
		killRecordParticles?.render();

		//float elapsed = (Time.currentTime - GameState.instance.run.endedTime) / 1e9f - GameState.GAME_OVER_SCREEN_DELAY;
		//float animation = elapsed * 3;
		//if (animation < 1)
		//	Renderer.DrawUISprite(0, (int)(animation * Renderer.UIHeight), Renderer.UIWidth, (int)((1 - animation) * Renderer.UIHeight), null, false, 0xFF000000);
	}
}
