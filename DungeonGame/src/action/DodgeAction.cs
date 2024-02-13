using Rainfall;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


internal class DodgeAction : Action
{
	const float DODGE_DURATION = 0.7f;
	const float MAX_DODGE_SPEED = 10.0f;
	const float MIN_DODGE_SPEED = 0.1f;
	const float DODGE_DISTANCE = (MAX_DODGE_SPEED + MIN_DODGE_SPEED) * 0.5f * DODGE_DURATION; //2.0f;
																							  //const float MIN_DODGE_SPEED = 2 * DODGE_DISTANCE / DODGE_DURATION - Player.MAX_GROUND_SPEED;

	const float STAMINA_COST = 3.0f;


	Vector3 direction;

	public DodgeAction(Vector3 fsu, float yaw)
		: base(ActionType.Dodge)
	{
		direction = Quaternion.FromAxisAngle(Vector3.Up, yaw) * (fsu * new Vector3(1, 1, -1));

		movementSpeedMultiplier = 1.0f;
		lockMovement = true;
		duration = DODGE_DURATION;

		//movementInput = fsu;
		maxSpeed = MIN_DODGE_SPEED;

		iframesStartTime = 0.0f;
		iframesEndTime = 0.0f;

		staminaCost = STAMINA_COST;
		staminaCostTime = 0.0f;
	}

	float heightFunc(float x)
	{
		float floorHitPoint = 0.7f;
		float xx = (x + (1 - floorHitPoint)) * 2 % 2 - 1;
		float y = 1 - MathF.Pow(xx, 4);
		return y;
	}

	public float getCameraHeight()
	{
		float progress = elapsedTime / duration;
		float y = heightFunc(progress) / heightFunc(0.0f);
		return MathHelper.Lerp(Player.CAMERA_HEIGHT_STANDING * 0.5f, Player.CAMERA_HEIGHT_STANDING, y);
		/*
		if (progress < floorHitPoint)
		{
			float blend = 1.0f - MathF.Pow(progress / floorHitPoint, 2.0f);
			return MathHelper.Lerp(Player.CAMERA_HEIGHT_STANDING * 0.5f, Player.CAMERA_HEIGHT_STANDING, blend);
		}
		else
		{
			float blend = 1.0f - MathF.Pow(1 - (progress - floorHitPoint) / (1 - floorHitPoint), 2.0f);
			return MathHelper.Lerp(Player.CAMERA_HEIGHT_STANDING * 0.5f, Player.CAMERA_HEIGHT_STANDING, blend);
		}
		*/

		/*
		float progress = elapsedTime / duration;
		progress = 1.0f - MathF.Pow(1.0f - progress, 2.0f);
		float sway = 1.0f - MathF.Sin(progress * MathF.PI);
		return MathHelper.Lerp(CAMERA_HEIGHT_STANDING - 0.2f, CAMERA_HEIGHT_STANDING, sway);
		*/
	}

	public float getViewmodelHeight()
	{
		float progress = elapsedTime / duration;
		float y = heightFunc(progress) / heightFunc(0.0f);
		return MathHelper.Lerp(MathF.PI * 1.0f, 0.0f, y);
		/*
		if (progress < floorHitPoint)
		{
			float blend = MathF.Pow(progress / floorHitPoint, 2.0f);
			return MathHelper.Lerp(-0.1f * MathF.PI, 0.5f * MathF.PI, blend);
			//return MathHelper.Lerp(Player.CAMERA_HEIGHT_STANDING * 0.5f, Player.CAMERA_HEIGHT_STANDING, blend);
		}
		else
		{
			float blend = MathF.Pow(1 - (progress - floorHitPoint) / (1 - floorHitPoint), 2.0f);
			return MathHelper.Lerp(0.0f, 0.5f * MathF.PI, blend);
			//return MathHelper.Lerp(Player.CAMERA_HEIGHT_STANDING * 0.5f, Player.CAMERA_HEIGHT_STANDING, blend);
		}
		*/
	}

	public override void update(Player player)
	{
		base.update(player);

		//maxSpeed = MathHelper.Lerp(MAX_DODGE_SPEED, Player.MAX_GROUND_SPEED, elapsedTime / duration);
		maxSpeed = Math.Clamp(MathHelper.Lerp(MAX_DODGE_SPEED, MIN_DODGE_SPEED, elapsedTime / duration), MIN_DODGE_SPEED, MAX_DODGE_SPEED);

		movementInput = (Quaternion.FromAxisAngle(Vector3.Up, player.yaw).conjugated * direction) * new Vector3(1, 1, -1);

		float progress = elapsedTime / duration;
		float fovFunc = 1 - MathF.Pow(progress * 2 - 1, 2);
		float targetFOV = MathHelper.Lerp(90, 98, fovFunc);
		player.camera.fov = MathHelper.Lerp(player.camera.fov, targetFOV, 5 * Time.deltaTime);
		//float targetFOV = MathHelper.Remap(maxSpeed, MAX_DODGE_SPEED, MIN_DODGE_SPEED, 90.0f, 96.0f);
		//player.camera.fov = MathHelper.Lerp(player.camera.fov, targetFOV, 20 * Time.deltaTime);

		if (player.isGrounded)
			iframesEndTime = DODGE_DURATION;
	}
}
