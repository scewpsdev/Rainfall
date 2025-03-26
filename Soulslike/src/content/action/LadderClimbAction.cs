using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LadderClimbAction : PlayerAction
{
	int idx;
	bool up;

	public LadderClimbAction(Ladder ladder, int idx = -1, bool up = true)
		: base("ladder_climb", (idx + (up ? 0 : 1)) % 2)
	{
		this.idx = idx;
		this.up = up;

		animationName[0] = up ? "ladder_up" : "ladder_down";
		animationName[1] = up ? "ladder_up" : "ladder_down";

		overrideWeaponModel[0] = true;
		overrideWeaponModel[1] = true;

		mirrorAnimation = idx != -1 && idx % 2 == 0;

		animationSpeed = FirstPersonController.LADDER_SPEED / (0.2f / (10 / 24.0f));

		fullBodyAnimation = true;

		duration = 1000;
		lockYaw = true;
		ignorePitch = true;
		lockMovement = true;
		swayAmount = 0;

		followUpCancelTime = idx != -1 ? 10 / 24.0f : 0;

		if (idx != -1)
			addSoundEffect(new ActionSfx(ladder.stepSound, 1, 1, followUpCancelTime, true));

		//lockCameraRotation = true;
		//movementSpeedMultiplier = 0.0f;
	}

	public override void update(Player player)
	{
		base.update(player);

		inputForward = false;
		inputBack = false;

		if (elapsedTime < followUpCancelTime)
		{
			if (up)
				inputForward = true;
			else
				inputBack = true;
		}
		else
		{
			if (InputManager.IsDown("MoveForward"))
				player.actionManager.queueAction(new LadderClimbAction(player.controller.currentLadder, idx + 1, true));
			else if (InputManager.IsDown("MoveBack"))
				player.actionManager.queueAction(new LadderClimbAction(player.controller.currentLadder, idx + 1, false));
		}

		if (player.controller.currentLadder == null)
			cancel();
	}
}
