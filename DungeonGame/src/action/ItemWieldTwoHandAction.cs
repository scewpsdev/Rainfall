using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


internal class ItemWieldTwoHandAction : Action
{
	const float DURATION = 0.2f;


	int handID;
	int twoHandedWeapon;


	public ItemWieldTwoHandAction(Item item, int handID, Player player)
		: base(ActionType.ItemWieldTwoHand, ActionPriority.Normal)
	{
		this.handID = handID;

		twoHandedWeapon = handID;

		animationName[handID ^ 1] = "idle";
		animationSet[handID ^ 1] = item.moveset;

		mirrorAnimation = handID == 1;

		animationTransitionDuration = DURATION;
		duration = DURATION;
	}

	public override void onFinished(Player player)
	{
		player.inventory.twoHandedWeapon = twoHandedWeapon;
		player.updateMovesetLayer(player.inventory.getSelectedHandItem(handID ^ 1), handID ^ 1);
		//player.updateMovesetLayer(player.inventory.getSelectedHandItem(1), 1);
	}
}
