using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SapphireRing : Item
{
	public SapphireRing()
		: base("sapphire_ring", ItemType.Relic)
	{
		displayName = "Sapphire Ring";

		description = "Increases energy recovery rate";
		value = 45;

		sprite = new Sprite(tileset, 13, 5);

		buff = new ItemBuff(this) { manaRecoveryModifier = 2 };
	}
}
