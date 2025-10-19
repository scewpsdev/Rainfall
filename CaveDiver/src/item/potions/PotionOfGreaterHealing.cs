using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PotionOfGreaterHealing : Potion
{
	public PotionOfGreaterHealing()
		: base("potion_of_greater_healing")
	{
		addEffect(new HealPotionEffect(2, 3));

		displayName = "Potion of Greater Healing";
		//stackable = true;
		canDrop = true;

		value = 44;

		sprite = new Sprite(tileset, 7, 0);
	}

	public override void upgrade()
	{
		base.upgrade();
		(effects[0] as HealPotionEffect).amount += 0.5f;
	}
}
