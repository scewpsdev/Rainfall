using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Skull : Object
{
	public Skull()
	{
		displayName = "Skull";

		damage = 4;

		sprite = new Sprite(Item.tileset, 0, 0);
		collider = new FloatRect(-4 / 16.0f, 0, 8 / 16.0f, 8 / 16.0f);
		platformCollider = true;

		hitSound = Item.woodHit;
	}

	protected override void onCollision(bool x, bool y, bool isEntity)
	{
		if (velocity.length > 8)
		{
			int numCoins = MathHelper.RandomInt(1, 6); // MathHelper.RandomInt((int)MathF.Round(value / 2), (int)MathF.Round(value * 1.5f));
			while (numCoins > 0)
			{
				CoinType type = Coin.SubtractCoinFromValue(ref numCoins);
				Coin coin = new Coin(type);
				Vector2 spawnPosition = position + MathHelper.RandomVector2(-0.5f, 0.5f, Random.Shared);
				coin.velocity = (spawnPosition - position).normalized * 4;
				GameState.instance.level.addEntity(coin, spawnPosition);
			}
			remove();
		}

		base.onCollision(x, y, isEntity);
	}
}
