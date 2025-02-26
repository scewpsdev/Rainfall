using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LadderClimbAction : PlayerAction
{
	public LadderClimbAction()
		: base("ladder_climb")
	{
		animationName[0] = "ladder";
		animationName[1] = "ladder";

		overrideWeaponModel[0] = true;
		overrideWeaponModel[1] = true;

		fullBodyAnimation = true;

		duration = 1000;
		lockYaw = true;
		ignorePitch = true;

		//lockCameraRotation = true;
		//movementSpeedMultiplier = 0.0f;
	}

	public override void update(Player player)
	{
		base.update(player);

		if (player.controller.currentLadder == null)
			cancel();
	}
}
