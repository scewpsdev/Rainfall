﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class DeathAction : Action
{
	public DeathAction()
		: base(ActionType.Death, ActionPriority.Death)
	{
		animationName[0] = "death";
		animationName[1] = "death";
		fullBodyAnimation = true;
		animateCameraRotation = true;
		rootMotion = true;

		movementSpeedMultiplier = 0.0f;
		lockRotation = true;

		overrideHandModels[0] = true;
		overrideHandModels[1] = true;

		handItemModels[0] = null;
		handItemModels[1] = null;

		iframesStartTime = 0.0f;
		iframesEndTime = 100;
	}

	public override void update(Player player)
	{
		base.update(player);

		player.pitch = MathHelper.Lerp(player.pitch, 0.0f, Time.deltaTime * 4.0f);

		player.hud.fadeout = 1.0f - MathF.Pow(elapsedTime / duration, 5.0f);
	}

	public override void onFinished(Player player)
	{
		//player.setPosition(player.resetPoint.translation);
		//player.setRotation(player.resetPoint.rotation);
		//player.queueAction(new SpawnAction());

		player.hud.fadeout = 0.0f;

		DungeonGame.instance.gameManager.onDeath();
	}
}
