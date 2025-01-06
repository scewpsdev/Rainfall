using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WoodenShield : Item
{
	public WoodenShield()
		: base("wooden_shield", ItemType.Shield)
	{
		displayName = "Wooden Shield";

		baseArmor = 1;
		value = 5;
		baseWeight = 1;

		isSecondaryItem = true;
		blockDuration = 0.3f;
		blockCharge = 0.08f;
		blockMovementSpeed = 0.5f;
		blockAbsorption = 0.8f;

		sprite = new Sprite(tileset, 9, 8);
		renderOffset.x = 0.2f;

		blockSound = woodHit;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new BlockAction(this, player.handItem == this));
		return false;
	}
}
