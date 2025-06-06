using Rainfall;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BackhopAction : EntityAction
{
	const float windupTime = 0.2f;
	const float dashTime = 0.2f;
	const float dashDistance = 3;
	const float speed = dashDistance / dashTime;

	int direction;
	bool hopStarted = false;

	public BackhopAction()
		: base("backhop")
	{
		speedMultiplier = 0;

		duration = windupTime + dashTime;
		iframesStartTime = windupTime;
		iframesEndTime = duration;
		
		turnToCrosshair = false;
	}

	public override void onStarted(Player player)
	{
		direction = player.direction;
	}

	public override void onFinished(Player player)
	{
		Audio.Play(player.landSound, new Vector3(player.position, 0));
	}

	public override void update(Player player)
	{
		if (elapsedTime + Time.deltaTime * animationSpeed >= windupTime && !hopStarted && InputManager.IsDown("Sprint"))
			;
		else
			elapsedTime += Time.deltaTime * animationSpeed;

		if (elapsedTime < windupTime)
		{
			actionMovement = 0;
			animation = "stun";
		}
		else
		{
			if (!hopStarted)
			{
				direction = player.direction;
				player.velocity.y = player.jumpPower * 0.5f;
				Audio.Play(player.jumpSound, new Vector3(player.position, 0));
				hopStarted = true;
			}

			actionMovement = -speed * direction;
			animation = "backhop";
		}
	}
}
