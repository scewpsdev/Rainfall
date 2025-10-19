using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


public class BluePotion : Potion
{
	public BluePotion()
		: base("blue_potion", "Blue Potion", PotionEffectType.Mana)
	{
		/*
		addEffect(new ManaEffect(4, 30));

		displayName = "Blue Flask";
		description = "Boosts mana recovery speed for a short amount of time";

		stackable = true;
		canDrop = true;
		upgradable = true;

		value = 12;

		sprite = new Sprite(tileset, 6, 2);
		*/
	}

	public override void upgrade()
	{
		base.upgrade();
		(effects[0] as ManaEffect).amount++;
		if (upgradeLevel == 1)
		{
			displayName = "Potion of Greater Resonance";
			name = "potion_of_greater_resonance";
			upgradeLevel = 0;
		}
		else if (upgradeLevel == 2)
		{
			displayName = "Potion of Immense Resonance";
			name = "potion_of_immense_resonance";
			upgradeLevel = 0;
		}
		else
		{
			displayName = "Potion of Supreme Resonance";
			name = "potion_of_supreme_resonance";
			upgradeLevel = 0;
		}
	}
}
