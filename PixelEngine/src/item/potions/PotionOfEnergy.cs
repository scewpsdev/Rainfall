using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


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

public class PotionOfEnergy : Potion
{
	public PotionOfEnergy()
		: base("potion_of_energy")
	{
		addEffect(new ManaEffect());

		displayName = "Potion of Energy";
		stackable = true;
		canDrop = true;

		value = 25;

		sprite = new Sprite(tileset, 6, 2);
	}

	public override void upgrade()
	{
		base.upgrade();
		(effects[0] as ManaEffect).amount++;
	}
}
