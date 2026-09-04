using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LeaderboardEntity : Entity, Interactable
{
	bool speedrun;

	Sprite sprite;
	float namesAlpha;
	bool hovered;
	//uint outline;


	public LeaderboardEntity(bool speedrun)
	{
		this.speedrun = speedrun;
		sprite = new Sprite(tileset, 0, 19, 9, 5);
	}

	public float getRange() { return 4; }

	public void interact(Player player)
	{
	}

	public void onFocusEnter(Player player)
	{
		hovered = true;
		//outline = OUTLINE_COLOR;
	}

	public void onFocusLeft(Player player)
	{
		hovered = false;
		//outline = 0;
	}

	public override void update()
	{
		namesAlpha = Mathf.Lerp(namesAlpha, hovered ? 0.8f : 0, 1 * Time.deltaTime);
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 4.5f, position.y, 9, 5, sprite);

		float lineWidth = 7.5f;
		float lineHeight = 0.7f;
		int numLines = 6;

		for (int i = 0; i < Leaderboards.leaderboard.Count; i++)
		{
			LeaderboardEntry entry = Leaderboards.leaderboard[i];

			float x = position.x - lineWidth / 2;
			float y = position.y + 2.5f + numLines * 0.5f * lineHeight - lineHeight - i * lineHeight;
			Renderer.DrawWorldTextBMP(x, y, LAYER_FG, entry.name, 1.0f / 16, Mathf.ColorAlpha(0xFF7F7FFF, namesAlpha), true);
			string scoreTxt = entry.score.ToString();
			Renderer.DrawWorldTextBMP(x + lineWidth - Renderer.MeasureWorldTextBMP(scoreTxt, -1, 1.0f / 16).x, y, LAYER_FG, scoreTxt, 1.0f / 16, Mathf.ColorAlpha(0xFF7F7FFF, namesAlpha), true);
		}
	}
}
