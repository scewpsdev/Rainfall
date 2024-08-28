using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Revolver : Item
{
	public Revolver()
		: base("revolver", ItemType.Weapon)
	{
		displayName = "Revolver";

		attackRate = 10.0f;
		//trigger = false;

		attackDamage = 20;

		value = 1000;
		canDrop = false;

		sprite = new Sprite(tileset, 14, 0);
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new RevolverShootAction(this));
		return true;
	}
}
