using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Container : Object
{
	Item[] items;
	public int coins = 0;

	protected Sound[] hitSound;
	protected Sound[] breakSound;


	public Container(params Item[] items)
	{
		this.items = items;
	}

	void dropItems()
	{
		for (int i = 0; i < items.Length; i++)
		{
			Vector2 itemVelocity = new Vector2(MathHelper.RandomFloat(-0.2f, 0.2f), 0.5f) * 8;
			Vector2 throwOrigin = position + new Vector2(0, 0.5f);
			ItemEntity obj = new ItemEntity(items[i], null, itemVelocity);
			GameState.instance.level.addEntity(obj, throwOrigin);
		}
		items = null;

		for (int i = 0; i < coins; i++)
		{
			Coin coin = new Coin();
			Vector2 spawnPosition = position + new Vector2(0, 0.5f) + Vector2.Rotate(Vector2.UnitX, i / (float)coins * 2 * MathF.PI) * 0.2f;
			coin.velocity = (spawnPosition - position - new Vector2(0, 0.5f)).normalized * 4;
			GameState.instance.level.addEntity(coin, spawnPosition);
		}
	}

	protected virtual void breakContainer()
	{
		if (items != null)
			dropItems();
		if (breakSound != null)
			Audio.PlayOrganic(breakSound, new Vector3(position, 0));
		remove();
	}

	public override bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		base.hit(damage, by, item, byName, triggerInvincibility, buffedHit);

		if (health <= 0)
			breakContainer();
		else
		{
			if (hitSound != null)
				Audio.PlayOrganic(hitSound, new Vector3(position, 0));
		}

		return true;
	}

	protected override void onCollision(bool x, bool y, bool isEntity)
	{
		if (isEntity)
			hit(velocity.length / 8);
		else if (velocity.length > 8)
			hit((velocity.length - 8) / 8);

		base.onCollision(x, y, isEntity);
	}
}
