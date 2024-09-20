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
			player.handItem.staffCharges = (int)MathF.Ceiling(player.handItem.maxStaffCharges * (1 + 0.5f * upgradeLevel));
			return true;
		}
		return false;
	}
}
