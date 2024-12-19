using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HealEffect : PotionEffect
{
	public float amount;
	float duration;

	public HealEffect(float amount = 1.5f, float duration = 2)
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

public class PotionOfHealing : Potion
{
	public PotionOfHealing(float amount)
		: base("potion_of_healing")
	{
		addEffect(new HealEffect(amount));

		displayName = "Potion of Healing";
		stackable = true;
		canDrop = true;

		value = 25;
		throwableChance = 0.1f;

		sprite = new Sprite(tileset, 7, 0);
	}

	public PotionOfHealing()
		: this(1.5f)
	{
	}

	public override void upgrade()
	{
		base.upgrade();
		(effects[0] as HealEffect).amount += 0.5f;
	}
}
