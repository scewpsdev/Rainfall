using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum PotionEffectType
{
	Water,
	Poison,
	Burn,
	Mana,
	Healing,
	Invisibility,
	Teleport,
	Lucky,

	Count
}

public abstract class Potion : Item
{
	public static Sprite[] potionSprites = [
		new Sprite(tileset, 4, 5),
		new Sprite(tileset, 5, 5),
		new Sprite(tileset, 13, 10),
		new Sprite(tileset, 6, 2),
		new Sprite(tileset, 7, 0),
		new Sprite(tileset, 7, 5),
		new Sprite(tileset, 6, 5),
		new Sprite(tileset, 14, 10),
	];
	public static string[] potionNames = [
		//"Pale Potion",
		//"Green Potion",
		//"Fiery Potion",
		//"Blue Potion",
		//"Crimson Potion",
		//"Smokey Potion",
		//"Whirly Potion",
		//"Golden Potion",
		"Bottle of Water",
		"Poison Vial",
		"Burning Potion",
		"Potion of Resonance",
		"Potion of Healing",
		"Potion of Invisibility",
		"Potion of Teleport",
		"Lucky Potion",
	];
	public static Func<Potion>[] potionConstructors = [
		() => new PalePotion(),
		() => new GreenPotion(),
		() => new FieryPotion(),
		() => new BluePotion(),
		() => new CrimsonPotion(),
		() => new SmokeyPotion(),
		() => new WhirlyPotion(),
		() => new GoldenPotion()
	];
	public static int[] potionValues = [
		3,
		17,
		19,
		22,
		25,
		16,
		14,
		23,
	];
	public static uint[] potionColors = [
		0xFF7fa6c4,
		0xFFAFAF2A,
		0xFFc64d16,
		0xFF758FFF,
		0xFFFF4D40,
		0xFFabb6bd,
		0xFFabb6bd,
		0xFFb59964,
	];

	PotionEffectType effect;
	public List<PotionEffect> effects = new List<PotionEffect>();
	public bool throwable = false;
	bool randomized = false;
	uint potionColor = 0xFFFF00FF;

	public float spillRadius = 1.5f;

	Sound[] breakSound;


	public Potion(string name, string displayName, PotionEffectType effect)
		: base(name, ItemType.Potion)
	{
		this.effect = effect;
		this.displayName = displayName;

		//displayName = "Potion";
		baseValue = 20;
		stackable = true;
		identified = false;
		//sprite = new Sprite(tileset, 4, 5);
		//canDrop = false;

		breakSound = Resource.GetSounds("sounds/break_bottle", 3);

		sprite = potionSprites[(int)effect];
		//displayName = potionNames[(int)effect];
		potionColor = potionColors[(int)effect];

		renderOffset.x = 0;
	}

	public Potion()
		: this("mixed_potion", "???", PotionEffectType.Count)
	{
	}

	public override int getValue()
	{
		int effectShift = (int)(Hash.hash(GameState.instance.run.seed) % (int)PotionEffectType.Count);
		int idx = ((int)effect + effectShift) % (int)PotionEffectType.Count;
		int randomVariation = (int)(Hash.combine(Hash.hash(GameState.instance.run.seed), Hash.hash((int)effect)) % 5);
		return potionValues[idx] + randomVariation;
	}

	public override void identify()
	{
		int effectShift = (int)(Hash.hash(GameState.instance.run.seed) % (int)PotionEffectType.Count);
		int idx = ((int)effect + effectShift) % (int)PotionEffectType.Count;
		//sprite = potionSprites[idx];
		displayName = (throwable ? "Throwable " : "") + potionNames[idx];
		//potionColor = potionColors[idx];
		if (!GameState.instance.identifiedPotions.Contains(name))
			GameState.instance.identifiedPotions.Add(name);
	}

	public override void update(Entity entity)
	{
		base.update(entity);

		if (GameState.instance.identifiedPotions.Contains(name))
			identify();
	}

	public PotionEffectType getTrueEffect()
	{
		int effectShift = (int)(Hash.hash(GameState.instance.run.seed) % (int)PotionEffectType.Count);
		int idx = ((int)this.effect + effectShift) % (int)PotionEffectType.Count;
		return (PotionEffectType)idx;
	}

	public override void applyEffect(Entity entity)
	{
		PotionEffectType effect = getTrueEffect();

		Player player = entity as Player;
		StatusEffectReceiver statusEffect = entity as StatusEffectReceiver;
		Hittable hittable = entity as Hittable;

		switch (effect)
		{
			case PotionEffectType.Water:
				if (player != null)
					player.hud.showMessage("It tastes bland.");
				break;
			case PotionEffectType.Poison:
				if (statusEffect != null)
					statusEffect.addStatusEffect(new PoisonStatusEffect(1.5f, 16));
				if (player != null)
					player.hud.showMessage("It burns on your tongue.");
				break;
			case PotionEffectType.Burn:
				if (hittable != null)
					hittable.hit(1, null, this);
				break;
			case PotionEffectType.Mana:
				if (statusEffect != null)
					statusEffect.addStatusEffect(new ManaRechargeEffect(2, 4));
				if (player != null)
					player.hud.showMessage("You feel energy flow through you.");
				break;
			case PotionEffectType.Healing:
				if (statusEffect != null)
					statusEffect.addStatusEffect(new HealStatusEffect(1.5f, 1));
				if (player != null)
				{
					if (Random.Shared.NextSingle() < 0.5f)
						player.hud.showMessage("You feel refreshed.");
					else
						player.hud.showMessage("You feel your strength returning.");
				}
				break;
			case PotionEffectType.Invisibility:
				if (statusEffect != null)
					statusEffect.addStatusEffect(new InvisibilityStatusEffect(10));
				if (player != null)
					player.hud.showMessage("Your body vanishes.");
				break;
			case PotionEffectType.Teleport:
				SpellEffects.TeleportEntity(entity);
				if (player != null)
					player.hud.showMessage("Everything around you starts spinning.");
				break;
			case PotionEffectType.Lucky:
				//player.luck *= 2;
				if (player != null)
					player.hud.showMessage("You feel like luck is on your side.");
				break;
		}
	}

	public Item makeThrowable()
	{
		if (!throwable)
		{
			baseValue++;
			name = "throwable_" + name;
			displayName = "Throwable " + displayName;
			projectileItem = true;
			projectileSpins = true;
			breakOnWallHit = true;
			breakOnEnemyHit = true;
			isHandItem = false;
			isActiveItem = true;
			throwable = true;
		}
		return this;
	}

	public void addEffect(PotionEffect effect)
	{
		if (effects.Count == 0)
			displayName += " of ";
		if (effects.Count > 0)
			displayName += ", ";
		displayName += effect.name;
		baseValue += effect.value;
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
			if (player.actions.actionQueue.Count <= 1)
			{
				player.actions.queueAction(new PotionDrinkAction(this));
				return false;
			}
			return false;
		}
	}

	public override bool useSecondary(Player player)
	{
		projectileItem = true;
		projectileSpins = true;
		breakOnWallHit = true;
		breakOnEnemyHit = true;
		throwable = true;

		player.throwItem(this, player.lookDirection);
		return true;
	}

	public override void onEntityBreak(ItemEntity entity)
	{
		HitData[] hits = new HitData[16];
		int numHits = GameState.instance.level.overlap(entity.position - spillRadius, entity.position + spillRadius, hits, Entity.FILTER_MOB | Entity.FILTER_PLAYER);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].entity != null && hits[i].entity != entity)
			{
				applyEffect(hits[i].entity);

				/*
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
				*/
			}
		}

		//Vector3 color = Vector3.Zero;
		//for (int i = 0; i < effects.Count; i++)
		//	color += Mathf.ARGBToVector(effects[i].color).xyz;
		//color /= effects.Count;
		Vector3 color = Mathf.ARGBToVector(potionColor).xyz;

		GameState.instance.level.addEntity(ParticleEffects.CreatePotionExplodeEffect(color), entity.position);
		GameState.instance.level.addEntity(new PotionExplodeEffect(spillRadius, color), entity.position);
		ParticleEffect potionParticleEffect = CreatePotionParticleEffect(getTrueEffect());
		if (potionParticleEffect != null)
			GameState.instance.level.addEntity(potionParticleEffect, entity.position);

		Audio.PlayOrganic(breakSound, new Vector3(entity.position, 0), 3);
	}

	public static Potion CreatePotionWithEffect(PotionEffectType effect)
	{
		int effectShift = (int)(Hash.hash(GameState.instance.run.seed) % (int)PotionEffectType.Count);
		int idx = ((int)effect + (int)PotionEffectType.Count - effectShift) % (int)PotionEffectType.Count;
		return potionConstructors[idx]();
	}

	public static ParticleEffect CreatePotionParticleEffect(PotionEffectType effect)
	{
		switch (effect)
		{
			case PotionEffectType.Water:
				return null;
			case PotionEffectType.Poison:
				return PoisonStatusEffect.createParticleEffect(null, 16, 1.5f);
			case PotionEffectType.Burn:
				return null;
			case PotionEffectType.Mana:
				return ManaRechargeEffect.createParticleEffect(null, 4, 2);
			case PotionEffectType.Healing:
				return HealStatusEffect.createParticleEffect(null, 1, 1.5f);
			case PotionEffectType.Invisibility:
				return InvisibilityStatusEffect.createParticleEffect(null, 10);
			case PotionEffectType.Teleport:
				return ParticleEffects.CreateScrollUseEffect(null);
			case PotionEffectType.Lucky:
				return null;
			default:
				return null;
		}
	}
}
