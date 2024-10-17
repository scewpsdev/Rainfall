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

		sprite = new Sprite(TileType.tileset, 2, 0);
	}

	void spawnCoins()
	{
		for (int j = 0; j < value; j++)
		{
			Coin coin = new Coin();
			Vector2 spawnPosition = position + MathHelper.RandomVector2(-0.5f, 0.5f, Random.Shared);
			coin.velocity = (spawnPosition - position).normalized * 4;
			GameState.instance.level.addEntity(coin, spawnPosition);
		}
	}

	public override void update()
	{
		HitData[] hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(position - 0.25f, position + 0.25f, hits, FILTER_PLAYER);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].entity != null && hits[i].entity is Player)
			{
				spawnCoins();
				remove();
				break;
			}
		}
	}

	public override void render()
	{
		Renderer.DrawVerticalSprite(position.x - 0.5f, position.y - 0.5f, 1, 1, sprite, false, 0xFFFFFFFF);
	}
}
