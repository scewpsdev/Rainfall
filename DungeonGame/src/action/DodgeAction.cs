using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


internal class DodgeAction : Action
{
	const float DODGE_DURATION = 0.3f;
	const float DODGE_DISTANCE = 2.0f;
	const float MAX_DODGE_SPEED = 2 * DODGE_DISTANCE / DODGE_DURATION - Player.MAX_GROUND_SPEED;

	const float STAMINA_COST = 5.5f;


	public DodgeAction(Vector3 fsu)
		: base(ActionType.Dodge)
	{
		movementSpeedMultiplier = 1.0f;
		lockMovement = true;
		duration = DODGE_DURATION;

		movementInput = fsu;
		maxSpeed = MAX_DODGE_SPEED;

		iframesStartTime = 0.0f;
		iframesEndTime = 0.0f;

		staminaCost = STAMINA_COST;
		staminaCostTime = 0.0f;
	}

	public override void update(Player player)
	{
		base.update(player);

		maxSpeed = MathHelper.Lerp(MAX_DODGE_SPEED, Player.MAX_GROUND_SPEED, elapsedTime / duration);

		if (player.isGrounded)
			iframesEndTime = DODGE_DURATION;
	}
}
