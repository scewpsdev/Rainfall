using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class InteractAction : Action
{
	Entity entity;

	Vector3 playerPosition;
	float playerYaw;


	public InteractAction(Entity entity, ActionType actionType, string actionName)
		: base(actionType, actionName)
	{
		this.entity = entity;

		animationName[0] = actionName;
		animationName[1] = actionName;
		fullBodyAnimation = true;
		animateCameraRotation = true;
		rootMotion = true;

		movementSpeedMultiplier = 0.0f;
		rotationSpeedMultiplier = 0.0f;
	}

	public override void onStarted(Player player)
	{
		playerPosition = player.position;
		playerYaw = player.yaw;
		player.setPhysicsEnabled(false);
		//player.controller.radius = 0.05f;

		//player.setPosition(chest.position + chest.rotation.back * 1.11111f);
		//player.yaw = chest.rotation.angle;
		//player.pitch = 0.0f;
	}

	public override void onFinished(Player player)
	{
		//player.controller.radius = 0.15f;
		player.setPhysicsEnabled(true);
	}

	public override void update(Player player)
	{
		{
			Vector3 dstPosition = entity.position + entity.rotation.back * 0.75f;
			Vector3 lastPlayerPosition = playerPosition;
			playerPosition = Vector3.Lerp(playerPosition, dstPosition, Time.deltaTime * 16.0f);
			Vector3 delta = playerPosition - lastPlayerPosition;
			player.setPosition(player.position + delta);
		}

		{
			float lastPlayerYaw = playerYaw;
			playerYaw = MathHelper.LerpAngle(playerYaw, entity.rotation.eulers.y, Time.deltaTime * 4.0f);
			//playerYaw = Quaternion.Slerp(Quaternion.FromAxisAngle(Vector3.Up, playerYaw), entity.rotation, Time.deltaTime * 4.0f).eulers.y;
			float yawDelta = playerYaw - lastPlayerYaw;
			player.yaw = player.yaw + yawDelta;
		}
	}
}
