using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagicStaff : Item
{
	public MagicStaff()
		: base("magic_staff", ItemType.Staff)
	{
		displayName = "Magic Staff";

		attackRate = 2.0f;
		trigger = false;

		attackDamage = 2;

		value = 30;

		sprite = new Sprite(tileset, 8, 1);
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new SpellCastAction(this));
		return true;
	}
}
