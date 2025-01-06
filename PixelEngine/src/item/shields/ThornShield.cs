using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ThornShield : Item
{
	public ThornShield()
		: base("thorn_shield", ItemType.Shield)
	{
		displayName = "Thorn Shield";

		baseArmor = 3;
		damageReflect = 1.0f;
		value = 14;

		isSecondaryItem = true;

		sprite = new Sprite(tileset, 4, 3);
		renderOffset.x = 0.2f;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new BlockAction(this, player.handItem == this));
		return false;
	}
}
