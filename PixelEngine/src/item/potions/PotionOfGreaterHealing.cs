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
		addEffect(new HealEffect(2, 3));

		displayName = "Potion of Greater Healing";
		//stackable = true;

		value = 80;

		sprite = new Sprite(tileset, 7, 0);
	}
}
