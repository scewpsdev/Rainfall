using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SmokeyPotion : Potion
{
	public SmokeyPotion()
		: base("smokey_potion", "Smokey Potion", PotionEffectType.Invisibility)
	{
		/*
		addEffect(new InvisibilityEffect(10));

		displayName = "Smokey Flask";

		value = 24;
		canDrop = true;
		stackable = true;

		sprite = new Sprite(tileset, 7, 5);
		*/
	}
}
