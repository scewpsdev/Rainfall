using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AmethystRing : Item
{
	public AmethystRing()
		: base("amethyst_ring", ItemType.Relic)
	{
		displayName = "Amethyst Ring";

		description = "Increases energy recovery rate";
		value = 45;

		sprite = new Sprite(tileset, 13, 5);

		modifier = new Modifier() { manaRecoveryModifier = 2 };
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
