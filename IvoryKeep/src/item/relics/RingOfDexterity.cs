using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfDexterity : Item
{
	public RingOfDexterity()
		: base("ring_of_dexterity", ItemType.Relic)
	{
		displayName = "Ring of Dexterity";

		description = "Increases attack speed by 10%";
		value = 60;

		sprite = new Sprite(tileset, 9, 0);

		buff = new ItemBuff(this) { attackSpeedModifier = 1.1f };
	}
}
