using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HuntersRing : Item
{
	public HuntersRing()
		: base("hunters_ring", ItemType.Ring)
	{
		displayName = "Hunter's Ring";

		description = "Unlimited arrows";

		value = 100;

		sprite = new Sprite(tileset, 0, 4);
	}

	public override void update(Entity entity)
	{
		if (entity is Player)
		{
			Player player = entity as Player;
			Item arrows = player.getItem("arrow");
			if (arrows == null)
				player.giveItem(new Arrow() { stackSize = 3 });
			else
			{
				if (arrows.stackSize < 3)
					arrows.stackSize = 3;
			}
		}
	}

	public override void onEquip(Player player)
	{
		player.speed++;
	}

	public override void onUnequip(Player player)
	{
		player.speed--;
	}
}
