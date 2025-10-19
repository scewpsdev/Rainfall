using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


public class GreenPotion : Potion
{
	public GreenPotion()
		: base("green_potion", "Green Potion", PotionEffectType.Poison)
	{
		/*
		addEffect(new PoisonEffect());

		displayName = "Green Flask";
		stackable = true;
		value = 11;
		canDrop = true;
		//makeThrowable();

		sprite = new Sprite(tileset, 5, 5);
		*/
	}

	public override void upgrade()
	{
		base.upgrade();
		(effects[0] as PoisonEffect).amount++;
	}
}
