using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfSwiftness : Item
{
	SpeedModifier modifier;

	public RingOfSwiftness()
		: base("ring_of_swiftness", ItemType.Relic)
	{
		displayName = "Ring of Swiftness";

		description = "Increases speed by 1";

		value = 100;

		sprite = new Sprite(tileset, 9, 0);
	}

	public override void onEquip(Player player)
	{
		player.speed += 1.0f + 0.5f * upgradeLevel;
		player.addStatusEffect(modifier = new SpeedModifier());
	}

	public override void onUnequip(Player player)
	{
		player.speed -= 1.0f + 0.5f * upgradeLevel;
		player.removeStatusEffect(modifier);
		modifier = null;
	}
}
