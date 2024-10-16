using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfSwiftness : Item
{
	public RingOfSwiftness()
		: base("ring_of_swiftness", ItemType.Relic)
	{
		displayName = "Ring of Swiftness";

		description = "Increases speed by 20%";

		value = 70;

		sprite = new Sprite(tileset, 9, 0);

		modifier = new Modifier() { movementSpeedModifier = 1.2f };
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
