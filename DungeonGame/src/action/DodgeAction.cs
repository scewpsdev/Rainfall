using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


internal class DodgeAction : Action
{
	const float DODGE_DURATION = 0.8f;
	const float DODGE_DISTANCE = 2.0f;
	const float MAX_DODGE_SPEED = 2 * DODGE_DISTANCE / DODGE_DURATION - Player.MAX_GROUND_SPEED;

	const float STAMINA_COST = 3.0f;


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

	public float getCameraHeight()
	{
		float floorHitPoint = 0.75f;
		float progress = elapsedTime / duration;
		if (progress < floorHitPoint)
		{
			float blend = 1.0f - MathF.Pow(progress / floorHitPoint, 2.0f);
			return MathHelper.Lerp(Player.CAMERA_HEIGHT_STANDING * 0.5f, Player.CAMERA_HEIGHT_STANDING, blend);
		}
		else
		{
			float blend = 1.0f - MathF.Pow(1-(progress - floorHitPoint) / (1 - floorHitPoint), 2.0f);
			return MathHelper.Lerp(Player.CAMERA_HEIGHT_STANDING * 0.5f, Player.CAMERA_HEIGHT_STANDING, blend);
		}

		/*
		float progress = elapsedTime / duration;
		progress = 1.0f - MathF.Pow(1.0f - progress, 2.0f);
		float sway = 1.0f - MathF.Sin(progress * MathF.PI);
		return MathHelper.Lerp(CAMERA_HEIGHT_STANDING - 0.2f, CAMERA_HEIGHT_STANDING, sway);
		*/
	}

	public override void update(Player player)
	{
		base.update(player);

		//maxSpeed = MathHelper.Lerp(MAX_DODGE_SPEED, Player.MAX_GROUND_SPEED, elapsedTime / duration);
		maxSpeed = MathHelper.Lerp(Player.MAX_GROUND_SPEED, MAX_DODGE_SPEED, elapsedTime / duration);

		float targetFOV = MathHelper.Remap(maxSpeed, Player.MAX_GROUND_SPEED, MAX_DODGE_SPEED, 90.0f, 96.0f);
		player.camera.fov = MathHelper.Lerp(player.camera.fov, targetFOV, 20 * Time.deltaTime);

		if (player.isGrounded)
			iframesEndTime = DODGE_DURATION;
	}
}
