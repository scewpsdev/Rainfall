using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ItemGate : Entity
{
	public override void update()
	{
		HitData[] hits = new HitData[1];
		int numHits = GameState.instance.level.overlap(position, position + new Vector2(1, 2), hits, FILTER_PLAYER);
		if (numHits > 0)
		{
			GameState.instance.player.clearInventory();
			GameState.instance.player.health = GameState.instance.player.maxHealth;
			GameState.instance.player.mana = GameState.instance.player.maxMana;
		}
	}

	public override void render()
	{
		Vector4 color = new Vector4(1, 0, 0.8f, MathF.Sin(Time.currentTime / 1e9f * 60) * 0.5f + 0.5f);
		Renderer.DrawSprite(position.x, position.y, LAYER_BG, 1, 2, 0, null, false, color);
	}
}
