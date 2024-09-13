using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Bomb : Item
{
	float blastRadius = 2.0f;
	float fuseTime = 1.5f;


	long useTime = -1;


	public Bomb()
		: base("bomb", ItemType.Utility)
	{
		displayName = "Bomb";
		stackable = true;

		value = 4;

		attackDamage = 8;

		sprite = new Sprite(tileset, 1, 0);

		//projectileItem = true;
	}

	public override bool use(Player player)
	{
		player.throwItem(this, player.lookDirection.normalized);
		ignite();
		return true;
	}

	public void ignite()
	{
		useTime = Time.currentTime;
	}

	public Bomb cook()
	{
		useTime = 1;
		return this;
	}

	void explode(Entity entity)
	{
		Effects.Explode(entity.position, blastRadius, attackDamage, entity, this);
	}

	public override void update(Entity entity)
	{
		if (useTime == 1) // cooking
			useTime = Time.currentTime;

		if (useTime != -1 && entity is ItemEntity)
		{
			ItemEntity itemEntity = entity as ItemEntity;
			itemEntity.color = (int)((Time.currentTime - useTime) / 1e9f * 20) % 2 == 1 ? new Vector4(5, 1, 1, 1) : new Vector4(1);
		}

		if (useTime != -1 && (Time.currentTime - useTime) / 1e9f >= fuseTime)
		{
			explode(entity);
			entity.remove();
		}
	}
}
