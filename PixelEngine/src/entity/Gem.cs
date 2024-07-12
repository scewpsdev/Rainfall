using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Gem : Entity
{
	public int value;

	Sprite sprite;

	
	public Gem(int value)
	{
		this.value = value;

		sprite = new Sprite(Item.tileset, 3, 0);
	}

	public override void update()
	{
		HitData[] hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(position - 0.25f, position + 0.25f, hits, FILTER_PLAYER);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].entity != null && hits[i].entity is Player)
			{
				Player player = hits[i].entity as Player;
				player.money += value;
				remove();
			}
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, 1, 1, sprite, false, 0xFFFFFFFF);
	}
}
