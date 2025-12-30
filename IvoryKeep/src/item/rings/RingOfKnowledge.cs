using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RingOfKnowledge : Item
{
	public RingOfKnowledge()
		: base("ring_of_knowledge", ItemType.Relic)
	{
		displayName = "Ring of Knowledge";
		description = "Allows it's bearer to identify items immediately";
		stackable = false;

		baseValue = 25;

		sprite = new Sprite(tileset, 15, 10);
	}

	public override void onItemPickUp(Player player, Item item)
	{
		if (!item.identified)
			item.identify();
	}

	public override void onEquip(Player player)
	{
		for (int i = 0; i < player.items.Count; i++)
		{
			if (!player.items[i].identified)
				player.items[i].identify();
		}
	}
}
