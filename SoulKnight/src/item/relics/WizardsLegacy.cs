using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WizardsLegacy : Item
{
	public WizardsLegacy()
		: base("wizards_legacy", ItemType.Relic)
	{
		displayName = "Wizard's Legacy";
		description = "Increases mana recovery rate by 100%";
		stackable = true;
		tumbles = false;
		canDrop = false;

		value = 27;

		sprite = new Sprite(tileset, 5, 6);

		modifier = new ItemBuff() { manaRecoveryModifier = 2 };
	}

	public override void onEquip(Player player)
	{
		player.itemBuffs.Add(modifier);
	}

	public override void onUnequip(Player player)
	{
		player.itemBuffs.Remove(modifier);
	}
}
