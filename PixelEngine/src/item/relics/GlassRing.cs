using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GlassRing : Item
{
	AttackModifier modifier;


	public GlassRing()
		: base("glass_ring", ItemType.Relic)
	{
		displayName = "Glass Ring";
		description = "Doubles attack, halves defense";
		stackable = false;
		canDrop = false;

		value = 25;

		sprite = new Sprite(tileset, 10, 6);
	}

	public override void onEquip(Player player)
	{
		player.attackDamageModifier *= 2;
		player.defenseModifier *= 0.5f;
		player.addStatusEffect(modifier = new AttackModifier(2, false));
	}

	public override void onUnequip(Player player)
	{
		player.attackDamageModifier /= 2;
		player.defenseModifier /= 0.5f;
		player.removeStatusEffect(modifier);
		modifier = null;
	}
}
