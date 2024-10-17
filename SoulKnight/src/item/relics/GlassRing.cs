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
		canDrop = false;

		value = 25;

		sprite = new Sprite(tileset, 10, 6);

		modifier = new ItemBuff() { meleeDamageModifier = 2, defenseModifier = 0.5f };
	}

	public override void onEquip(Player player)
	{
		player.itemBuffs.Add(modifier);
	}

	public override void onUnequip(Player player)
	{
		player.itemBuffs.Remove(modifier);
	}
}
