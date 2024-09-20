using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Potion : Item
{
	public List<PotionEffect> effects = new List<PotionEffect>();
	bool throwable = false;


	public Potion(string name)
		: base(name, ItemType.Potion)
	{
		displayName = "Mixed potion";
		value = 2;
		sprite = new Sprite(tileset, 4, 5);
		canDrop = false;
	}

	public Potion()
		: this("mixed_potion")
	{
	}

	public void makeThrowable()
	{
		value++;
		displayName = "Throwable " + displayName;
		breakOnWallHit = true;
		breakOnEnemyHit = true;
		throwable = true;
	}

	public void addEffect(PotionEffect effect)
	{
		if (effects.Count == 0)
			displayName += " of ";
		if (effects.Count > 0)
			displayName += ", ";
		displayName += effect.name;
		value += effect.value;
		sprite = effect.sprite;
		effects.Add(effect);
	}

	public override bool use(Player player)
	{
		if (throwable)
		{
			player.throwItem(this, player.lookDirection);
			return true;
		}
		else
		{
			foreach (PotionEffect effect in effects)
				effect.apply(player, this);
			player.removeItemSingle(this);
			player.giveItem(new GlassBottle());
			return false;
		}
	}

	public override void onEntityBreak(ItemEntity entity)
	{
		float radius = 2.5f;
		HitData[] hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(entity.position - radius, entity.position + radius, hits, Entity.FILTER_MOB | Entity.FILTER_PLAYER);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].entity != null && hits[i].entity != entity)
			{
				if (hits[i].entity is Mob)
				{
					Mob mob = hits[i].entity as Mob;
					for (int j = 0; j < effects.Count; j++)
					{
						effects[j].apply(mob, null);
					}
				}
				else if (hits[i].entity is Player)
				{
					Player player = hits[i].entity as Player;
					for (int j = 0; j < effects.Count; j++)
					{
						effects[j].apply(player, null);
					}
				}
			}
		}
	}
}
