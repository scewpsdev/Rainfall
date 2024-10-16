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

		buff = new ItemBuff() { accuracyModifier = 1.5f };
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
