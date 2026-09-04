using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Skull : Container
{
	public Skull()
	{
		displayName = "Skull";

		damage = 4;

		sprite = new Sprite(Item.tileset, 0, 0);
		collider = new Hitbox(-4 / 16.0f, 0, 8 / 16.0f, 8 / 16.0f);
		platformCollider = true;

		hitSound = Item.woodHit;
		breakSound = Item.woodBreak;
	}

	protected override void breakContainer()
	{
		base.breakContainer();

		GameState.instance.level.addEntity(ParticleEffects.CreateDestroyWoodEffect(0xFF675051, 20, velocity * 0.25f), position);

		int numCoins = Mathf.RandomInt(1, (level.floor + 1) * 5); // Mathf.RandomInt((int)MathF.Round(value / 2), (int)MathF.Round(value * 1.5f));
		while (numCoins > 0)
		{
			CoinType type = Coin.SubtractCoinFromValue(ref numCoins);
			Coin coin = new Coin(type);
			Vector2 spawnPosition = position + Mathf.RandomVector2(-0.5f, 0.5f, Random.Shared);
			coin.velocity = (spawnPosition - position).normalized * 4;
			GameState.instance.level.addEntity(coin, spawnPosition);
		}
	}

	public override bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		base.hit(damage, by, item, byName, triggerInvincibility);
		if (health > 0)
			GameState.instance.level.addEntity(ParticleEffects.CreateDestroyWoodEffect(0xFF675051), position);
		return true;
	}
}
