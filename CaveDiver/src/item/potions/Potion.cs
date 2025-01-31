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

	public float spillRadius = 1.5f;

	Sound[] breakSound;


	public Potion(string name)
		: base(name, ItemType.Potion)
	{
		displayName = "Potion";
		value = 2;
		sprite = new Sprite(tileset, 4, 5);
		canDrop = false;

		breakSound = Resource.GetSounds("sounds/break_bottle", 3);
	}

	public Potion()
		: this("mixed_potion")
	{
	}

	public Item makeThrowable()
	{
		value++;
		name = "throwable_" + name;
		displayName = "Throwable " + displayName;
		projectileItem = true;
		projectileSpins = true;
		breakOnWallHit = true;
		breakOnEnemyHit = true;
		throwable = true;
		return this;
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
			ItemEntity entity = player.throwItem(this, player.lookDirection);
			return true;
		}
		else
		{
			player.actions.queueAction(new PotionDrinkAction(this));
			return true;
		}
	}

	public override void onEntityBreak(ItemEntity entity)
	{
		HitData[] hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(entity.position - spillRadius, entity.position + spillRadius, hits, Entity.FILTER_MOB | Entity.FILTER_PLAYER);
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

		Vector3 color = Vector3.Zero;
		for (int i = 0; i < effects.Count; i++)
			color += MathHelper.ARGBToVector(effects[i].color).xyz;
		color /= effects.Count;

		GameState.instance.level.addEntity(ParticleEffects.CreatePotionExplodeEffect(color), entity.position);
		GameState.instance.level.addEntity(new PotionExplodeEffect(spillRadius, color), entity.position);

		Audio.PlayOrganic(breakSound, new Vector3(entity.position, 0), 3);
	}
}
