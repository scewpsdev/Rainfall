using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SapphireRing : Item
{
	public SapphireRing()
		: base("sapphire_ring", ItemType.Relic)
	{
		displayName = "Sapphire Ring";

		description = "Increases energy recovery rate";
		value = 45;

		sprite = new Sprite(tileset, 13, 5);

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
