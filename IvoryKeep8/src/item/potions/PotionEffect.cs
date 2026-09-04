using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class PotionEffect
{
	public string name;
	public int value;
	public Sprite sprite;
	public uint color;

	public PotionEffect(string name, int value, Sprite sprite, uint color)
	{
		this.name = name;
		this.value = value;
		this.sprite = sprite;
		this.color = color;
	}

	public abstract void apply(Entity entity, Potion potion);
}

public class WaterEffect : PotionEffect
{
	public bool boiling;

	public WaterEffect(bool boiling = false)
		: base("Water", 1, new Sprite(Item.tileset, 4, 5), 0xFF7fa6c4)
	{
		this.boiling = boiling;
	}

	public override void apply(Entity entity, Potion potion)
	{
		if (boiling)
		{
			if (entity is Hittable)
			{
				Hittable hittable = entity as Hittable;
				hittable.hit(Random.Shared.NextSingle() * 2, null, potion, "Boiling Water");
			}
			if (entity is Player)
			{
				Player player = entity as Player;
				player.hud.showMessage("The water is scalding hot.");
			}
		}
		else
		{
			if (entity is Player)
			{
				Player player = entity as Player;
				player.hud.showMessage("It tastes bland.");
			}
		}
	}
}

public class PoisonEffect : PotionEffect
{
	public float amount;
	float duration;

	public PoisonEffect(float amount = 1, float duration = 16)
		: base("Poison", 3, new Sprite(Item.tileset, 5, 5), 0xFFAFAF2A)
	{
		this.amount = amount;
		this.duration = duration;
	}

	public override void apply(Entity entity, Potion potion)
	{
		if (entity is StatusEffectReceiver)
		{
			StatusEffectReceiver receiver = entity as StatusEffectReceiver;
			receiver.addStatusEffect(new PoisonStatusEffect(amount, duration));
		}
		if (entity is Player)
		{
			Player player = entity as Player;
			player.hud.showMessage("The water burns on your tongue.");
		}
	}
}

public class ManaEffect : PotionEffect
{
	public float amount;
	public float duration;

	public ManaEffect(float amount = 2, float duration = 4)
		: base("Energy", 23, new Sprite(Item.tileset, 6, 2), 0xFF758FFF)
	{
		this.amount = amount;
		this.duration = duration;
	}

	public override void apply(Entity entity, Potion potion)
	{
		if (entity is StatusEffectReceiver)
		{
			StatusEffectReceiver receiver = entity as StatusEffectReceiver;
			receiver.addStatusEffect(new ManaRechargeEffect(amount, 4));
			if (entity is Player)
				(entity as Player).hud.showMessage("You feel energy flow through you.");
		}
	}
}

public class HealPotionEffect : PotionEffect
{
	public float amount;
	float duration;

	public HealPotionEffect(float amount = 1.5f, float duration = 1)
		: base("Healing", 23, new Sprite(Item.tileset, 7, 0), 0xFFFF4D40)
	{
		this.amount = amount;
		this.duration = duration;
	}

	public override void apply(Entity entity, Potion potion)
	{
		if (entity is StatusEffectReceiver)
		{
			StatusEffectReceiver receiver = entity as StatusEffectReceiver;
			receiver.addStatusEffect(new HealStatusEffect(amount, duration));
		}
		if (entity is Player)
		{
			Player player = entity as Player;

			float overshoot = player.health + amount - player.maxHealth;
			if (MathF.Floor(overshoot / 2 + 0.001f) >= 1)
				player.hp += (int)MathF.Floor(overshoot / 2 + 0.001f);

			if (Random.Shared.NextSingle() < 0.5f)
				player.hud.showMessage("You feel refreshed.");
			else
				player.hud.showMessage("You feel your strength returning.");
		}
	}
}

public class InvisibilityEffect : PotionEffect
{
	float duration;

	public InvisibilityEffect(float duration)
		: base("Invisibility", 26, new Sprite(Item.tileset, 7, 5), 0xFFabb6bd)
	{
		this.duration = duration;
	}

	public override void apply(Entity entity, Potion potion)
	{
		if (entity is StatusEffectReceiver)
		{
			StatusEffectReceiver receiver = entity as StatusEffectReceiver;
			receiver.addStatusEffect(new InvisibilityStatusEffect(duration));
		}
	}
}

public class TeleportEffect : PotionEffect
{
	public TeleportEffect()
		: base("Teleport", 9, new Sprite(Item.tileset, 6, 5), 0xFFabb6bd)
	{
	}

	public override void apply(Entity entity, Potion potion)
	{
		SpellEffects.TeleportEntity(entity);
	}
}
