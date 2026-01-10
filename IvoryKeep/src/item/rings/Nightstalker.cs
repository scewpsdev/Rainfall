using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Nightstalker : Item
{
	public Nightstalker()
		: base("nightstalker", ItemType.Relic)
	{
		displayName = "Nightstalker";
		description = "Greatly increases critical attack chance when attacking an enemy from behind";
		//stackable = true;
		tumbles = false;
		//canDrop = false;

		baseValue = 22;

		sprite = new Sprite(tileset, 7, 6);

		buff = new ItemBuff(this) { stealthAttackModifier = 8 };
	}
}
