using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


internal class BowShootAction : Action
{
	Item bow;
	int handID;


	public BowShootAction(Item bow, int handID)
		: base(ActionType.BowShoot)
	{
		this.bow = bow;
		this.handID = handID;

		handItemAnimations[handID] = "shoot";

		animationName[0] = "attack_shoot";
		animationName[1] = "attack_shoot";
		animationSet[0] = bow.moveset;
		animationSet[1] = bow.moveset;

		mirrorAnimation = handID == 1;

		if (bow.sfxShoot != null)
		{
			addSoundEffect(bow.sfxShoot, handID, 0.0f, true);
		}
	}

	public override void onStarted(Player player)
	{
		base.onStarted(player);

		Vector3 arrowPosition = player.lookOrigin;
		Vector3 arrowDirection = player.lookDirection;
		Quaternion arrowRotation = player.lookRotation;

		Matrix arrowTransform = player.getWeaponTransform(handID ^ 1);
		Vector3 offset = arrowTransform.translation - arrowPosition;

		DungeonGame.instance.level.addEntity(new Arrow(Item.Get("arrow"), bow, player, arrowDirection, offset), arrowPosition, arrowRotation);

		player.inventory.consumeArrow();
	}
}
