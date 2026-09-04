using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ClimbingGear : Item
{
	public ClimbingGear()
		: base("climbing_gear", ItemType.Relic)
	{
		displayName = "Climbing Gear";

		description = "Grants more air control when jumping off a wall";

		baseValue = 37;

		sprite = new Sprite(tileset, 6, 8);
	}

	public override void onEquip(Player player)
	{
		//player.canWallJump = true;
		player.wallControl *= 2;
	}

	public override void onUnequip(Player player)
	{
		//player.canWallJump = false;
		player.wallControl /= 2;
	}
}
