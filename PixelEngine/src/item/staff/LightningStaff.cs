using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LightningStaff : Item
{
	public LightningStaff()
		: base("staff_lightning", ItemType.Staff)
	{
		displayName = "Lightning Staff";

		attackRate = 4;
		//trigger = false;

		attackDamage = 2;

		value = 30;

		sprite = new Sprite(tileset, 8, 2);
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new LightningCastAction(this));
		return true;
	}
}
