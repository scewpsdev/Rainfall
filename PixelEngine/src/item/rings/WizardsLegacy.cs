using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WizardsLegacy : Item
{
	public WizardsLegacy()
		: base("wizards_legacy", ItemType.Ring)
	{
		displayName = "Wizard's Legacy";
		description = "Reduces mana cost of spells by 20%";
		stackable = true;

		value = 27;

		sprite = new Sprite(tileset, 5, 6);
	}

	public override void onEquip(Player player)
	{
		player.manaCostModifier *= 1 - 0.2f;
	}

	public override void onUnequip(Player player)
	{
		player.manaCostModifier /= 1 - 0.2f;
	}
}
