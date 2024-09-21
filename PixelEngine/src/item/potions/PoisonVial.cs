using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


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

public class PoisonVial : Potion
{
	public PoisonVial()
		: base("poison_vial")
	{
		addEffect(new PoisonEffect());

		displayName = "Poison Vial";
		stackable = true;
		value = 11;
		canDrop = true;

		sprite = new Sprite(tileset, 5, 5);
	}

	public override void upgrade(Player player)
	{
		base.upgrade(player);
		(effects[0] as PoisonEffect).amount++;
	}
}
