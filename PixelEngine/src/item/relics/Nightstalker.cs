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
		description = "Increases attack against unsuspecting enemies";
		stackable = true;
		tumbles = false;
		canDrop = false;

		value = 22;

		sprite = new Sprite(tileset, 7, 6);
	}

	public override void onEquip(Player player)
	{
		player.stealthAttackModifier *= 1 + 1;
	}

	public override void onUnequip(Player player)
	{
		player.stealthAttackModifier /= 1 + 1;
	}
}
