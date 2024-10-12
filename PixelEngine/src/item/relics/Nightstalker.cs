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
		description = "Attacks against unsuspecting enemies are critical attacks";
		//stackable = true;
		tumbles = false;
		canDrop = false;

		value = 22;

		sprite = new Sprite(tileset, 7, 6);

		modifier = new Modifier() { stealthAttackModifier = 2.0f };
	}

	public override void onEquip(Player player)
	{
		player.modifiers.Add(modifier);
	}

	public override void onUnequip(Player player)
	{
		player.modifiers.Remove(modifier);
	}
}
