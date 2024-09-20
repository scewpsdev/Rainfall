using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Skull : Item
{
	public Skull()
		: base("skull", ItemType.Weapon)
	{
		displayName = "Skull";

		projectileItem = true;
		breakOnWallHit = true;

		attackDamage = 4;

		value = 2;

		sprite = new Sprite(tileset, 0, 0);
	}

	public override bool use(Player player)
	{
		player.throwItem(this, player.lookDirection.normalized);
		return true;
	}

	public override void onEntityBreak(ItemEntity entity)
	{
		int numCoins = MathHelper.RandomInt((int)MathF.Round(value / 2), (int)MathF.Round(value * 1.5f));
		for (int i = 0; i < numCoins; i++)
		{
			Coin coin = new Coin();
			Vector2 spawnPosition = entity.position + MathHelper.RandomVector2(-0.5f, 0.5f, Random.Shared);
			coin.velocity = (spawnPosition - entity.position).normalized * 4;
			GameState.instance.level.addEntity(coin, spawnPosition);
		}
	}

	public override void upgrade()
	{
		base.upgrade();
		attackDamage++;
	}
}
