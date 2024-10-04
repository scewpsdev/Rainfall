using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Spellweaver : Item
{
	public Spellweaver()
		: base("spellweaver", ItemType.Relic)
	{
		displayName = "Spellweaver";
		description = "Reduces spell mana cost by 20%";
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
