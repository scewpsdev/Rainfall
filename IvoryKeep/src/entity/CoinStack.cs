using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CoinStack : Entity
{
	public int value;

	Sprite sprite;


	public CoinStack(int value)
	{
		this.value = value;

		sprite = new Sprite(tileset, 3, 1);
	}

	public CoinStack()
		: this(150)
	{
	}

	public override void update()
	{
		TileType tile = GameState.instance.level.getTile(position - new Vector2(0, 0.51f));
		if (!(tile != null && (tile.isSolid || tile.isPlatform)))
		{
			SpellEffects.SpawnCoins(value, position);
			remove();
		}

		HitData[] hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(position - 0.25f, position + 0.25f, hits, FILTER_PLAYER);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].entity != null && hits[i].entity is Player)
			{
				SpellEffects.SpawnCoins(value, position);
				remove();
				break;
			}
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, 1, 1, sprite, false, 0xFFFFFFFF);
	}
}
