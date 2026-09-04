using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CrimsonPotion : Potion
{
	public CrimsonPotion(float amount)
		: base("crimson_potion", "Crimson Potion", PotionEffectType.Healing)
	{
		/*
		addEffect(new HealPotionEffect(amount));

		displayName = "Crimson Flask";
		stackable = true;
		canDrop = true;
		upgradable = true;

		value = 20;

		sprite = new Sprite(tileset, 7, 0);
		*/
	}

	public CrimsonPotion()
		: this(1.5f)
	{
	}

	public override void upgrade()
	{
		base.upgrade();
		(effects[0] as HealPotionEffect).amount += 0.5f;
		if (upgradeLevel == 1)
		{
			displayName = "Potion of Greater Healing";
			name = "potion_of_greater_healing";
			upgradeLevel = 0;
		}
		else if (upgradeLevel == 2)
		{
			displayName = "Potion of Immense Healing";
			name = "potion_of_immense_healing";
			upgradeLevel = 0;
		}
		else
		{
			displayName = "Potion of Supreme Healing";
			name = "potion_of_supreme_healing";
			upgradeLevel = 0;
		}
	}
}
