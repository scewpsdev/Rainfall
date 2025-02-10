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
		tumbles = false;

		value = 27;

		sprite = new Sprite(tileset, 15, 7);

		buff = new ItemBuff(this) { manaCostModifier = 0.8f };
	}

	public override void onEquip(Player player)
	{
		player.itemBuffs.Add(buff);
	}

	public override void onUnequip(Player player)
	{
		player.itemBuffs.Remove(buff);
	}
}
