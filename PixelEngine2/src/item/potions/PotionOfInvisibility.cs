using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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

public class PotionOfInvisibility : Potion
{
	public PotionOfInvisibility()
		: base("potion_of_invisibility")
	{
		addEffect(new InvisibilityEffect(10));

		displayName = "Potion of Invisibility";

		value = 24;
		canDrop = true;
		stackable = true;

		sprite = new Sprite(tileset, 7, 5);
	}
}
