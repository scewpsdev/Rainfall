using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Staff : Item
{
	public Staff()
		: base("staff")
	{
		displayName = "Staff";

		sprite = new Sprite(tileset, 8, 1);

		attackRate = 2.0f;
		trigger = false;

		attackDamage = 2;
	}

	public override Item createNew()
	{
		return new Staff();
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new SpellCastAction(this));
		return true;
	}
}
