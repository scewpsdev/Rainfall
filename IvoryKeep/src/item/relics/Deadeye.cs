using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Deadeye : Item
{
	public Deadeye()
		: base("deadeye", ItemType.Relic)
	{
		displayName = "Magnifying Glass";
		description = "Increases projectile accuracy";
		stackable = true;

		value = 28;

		sprite = new Sprite(tileset, 0, 7);

		buff = new ItemBuff(this) { accuracyModifier = 1.5f };
	}
}
