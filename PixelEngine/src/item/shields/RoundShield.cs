using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RoundShield : Item
{
	public RoundShield()
		: base("round_shield", ItemType.Shield)
	{
		displayName = "Round Shield";

		baseArmor = 0.5f;
		value = 2;
		baseWeight = 0.5f;

		isSecondaryItem = true;
		blockDuration = 0.3f;
		blockCharge = 0.08f;
		blockMovementSpeed = 0.5f;
		blockAbsorption = 0.6f;

		sprite = new Sprite(tileset, 7, 7);
		renderOffset.x = 0.2f;

		blockSound = woodHit;
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new BlockAction(this, player.handItem == this));
		return false;
	}
}
