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
		displayName = "Deadeye";
		description = "Increases projectile accuracy";
		stackable = true;
		tumbles = false;

		value = 24;

		sprite = new Sprite(tileset, 0, 7);

		modifier = new Modifier() { accuracyModifier = 1.5f };
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
