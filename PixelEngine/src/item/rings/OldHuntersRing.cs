using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class OldHuntersRing : Item
{
	public OldHuntersRing()
		: base("old_hunters_ring", ItemType.Ring)
	{
		displayName = "Old Hunter's Ring";

		description = "Unlimited arrows";

		value = 100;

		sprite = new Sprite(tileset, 0, 4);
	}

	public override void onEquip(Player player)
	{
		player.unlimitedArrows = true;
	}

	public override void onUnequip(Player player)
	{
		player.unlimitedArrows = false;
	}
}
