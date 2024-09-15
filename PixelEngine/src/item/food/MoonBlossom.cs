using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MoonBlossom : Item
{
	public MoonBlossom()
		: base("moon_blossom", ItemType.Food)
	{
		displayName = "Moon Blossom";
		stackable = true;

		description = "Refills staff charges";

		value = 29;

		sprite = new Sprite(tileset, 12, 4);
	}

	public override bool use(Player player)
	{
		if (player.handItem != null && player.handItem.type == ItemType.Staff)
		{
			player.handItem.staffCharges = player.handItem.maxStaffCharges;
			return true;
		}
		return false;
	}
}
