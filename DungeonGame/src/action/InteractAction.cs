using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class InteractAction : Action
{
	Entity entity;

	float previousPlayerPitch;

	Vector3 playerPosition;
	float playerPitch;
	float playerYaw;
	//Vector3 viewmodelOffset;


	public InteractAction(Entity entity, ActionType actionType, string actionName)
		: base(actionType)
	{
		this.entity = entity;

		animationName[0] = actionName;
		animationName[1] = actionName;
		fullBodyAnimation = true;
		animateCameraRotation = true;
		rootMotion = true;

		movementSpeedMultiplier = 0.0f;
		lockRotation = true;
	}

	public override void onStarted(Player player)
	{
		previousPlayerPitch = player.pitch;
		playerPosition = player.position;
		playerPitch = player.pitch;
		playerYaw = player.yaw;
		//viewmodelOffset = player.viewmodelOffset;
		player.moveType = Player.MoveType.Fly;
		player.viewmodelScale = 1.0f;
		player.velocity = Vector3.Zero;
		//player.controller.radius = 0.05f;

		//player.setPosition(chest.position + chest.rotation.back * 1.11111f);
		//player.yaw = chest.rotation.angle;
		//player.pitch = 0.0f;
	}

	public override void onFinished(Player player)
	{
		//player.controller.radius = 0.15f;
		player.moveType = Player.MoveType.Walk;
		player.viewmodelScale = Player.DEFAULT_VIEWMODEL_SCALE;
	}

	public override void update(Player player)
	{
		base.update(player);

		{
			Vector3 dstPosition = entity.position + entity.rotation.back * 0.75f;
			Vector3 lastPlayerPosition = playerPosition;
			playerPosition = Vector3.Lerp(playerPosition, dstPosition, Time.deltaTime * 16.0f);
			Vector3 delta = playerPosition - lastPlayerPosition;
			player.setPosition(player.position + delta);
		}

		{
			float lastPlayerPitch = playerPitch;
			if (elapsedTime < duration - 1.0f)
				playerPitch = MathHelper.Lerp(playerPitch, 0.0f, Time.deltaTime * 4.0f);
			else
				playerPitch = MathHelper.Lerp(playerPitch, previousPlayerPitch, Time.deltaTime * 2.0f);
			float pitchDelta = playerPitch - lastPlayerPitch;
			player.pitch = player.pitch + pitchDelta;
		}

		{
			float lastPlayerYaw = playerYaw;
			playerYaw = MathHelper.LerpAngle(playerYaw, entity.rotation.eulers.y, Time.deltaTime * 4.0f);
			//playerYaw = Quaternion.Slerp(Quaternion.FromAxisAngle(Vector3.Up, playerYaw), entity.rotation, Time.deltaTime * 4.0f).eulers.y;
			float yawDelta = playerYaw - lastPlayerYaw;
			player.yaw = player.yaw + yawDelta;
		}

		/*
		{
			if (elapsedTime < duration - 1.0f)
				player.viewmodelOffset = Vector3.Lerp(player.viewmodelOffset, Vector3.Zero, Time.deltaTime * 4.0f);
			else
				player.viewmodelOffset = Vector3.Lerp(player.viewmodelOffset, Vector3.Zero, Time.deltaTime * 2.0f);
		}
		*/
	}
}
