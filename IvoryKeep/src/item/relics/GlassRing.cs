using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GlassRing : Item
{
	public GlassRing()
		: base("glass_ring", ItemType.Relic)
	{
		displayName = "Glass Ring";
		description = "Doubles attack, halves defense";
		stackable = false;

		value = 45;

		sprite = new Sprite(tileset, 10, 6);

		buff = new ItemBuff(this) { meleeDamageModifier = 2, magicDamageModifier = 2, defenseModifier = 0.5f };
	}
}
