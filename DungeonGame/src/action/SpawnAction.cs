using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class SpawnAction : Action
{
	public SpawnAction()
		: base(ActionType.Spawn)
	{
		animationName[0] = "spawn";
		animationName[1] = "spawn";
		fullBodyAnimation = true;
		animateCameraRotation = true;
		rootMotion = true;

		movementSpeedMultiplier = 0.0f;
		lockRotation = true;

		animationTransitionDuration = 0.0f;

		overrideHandModels[0] = true;
		overrideHandModels[1] = true;

		handItemModels[0] = null;
		handItemModels[1] = null;
	}

	public override void onStarted(Player player)
	{
		base.onStarted(player);

		player.stats.health = player.stats.maxHealth;
	}

	public override void onFinished(Player player)
	{
		player.hud.fadeout = 1.0f;
	}

	public override void update(Player player)
	{
		base.update(player);

		player.pitch = 0.0f;

		player.hud.fadeout = 1.0f - MathF.Exp(-elapsedTime / duration * 10.0f);
	}
}
