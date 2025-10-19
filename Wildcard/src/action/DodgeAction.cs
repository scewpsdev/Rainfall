using Rainfall;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DodgeAction : EntityAction
{
	public const float manaCost = 0.5f;

	const float cooldownTime = 0.2f;
	const float dashTime = 0.2f;
	const float dashDistance = 2.5f;
	const float speed = dashDistance / dashTime;

	int direction;

	ParticleEffect particles;


	public DodgeAction()
		: base("dodge")
	{
		speedMultiplier = 0;

		duration = dashTime + cooldownTime;
		iframesStartTime = 0;
		iframesEndTime = dashTime + 0.5f * cooldownTime;
		canJump = false;

		turnToCrosshair = false;
	}

	public override void onStarted(Player player)
	{
		direction = player.direction;
		player.velocity.y = player.jumpPower * 0.5f;
		Audio.Play(player.jumpSound, new Vector3(player.position, 0));

		GameState.instance.level.addEntity(particles = new ParticleEffect(player, "effects/dodge.rfs"), player.position);

		player.consumeMana(manaCost);
	}

	public override void onFinished(Player player)
	{
		Audio.Play(player.landSound, new Vector3(player.position, 0));
	}

	public override void update(Player player)
	{
		elapsedTime += Time.deltaTime * animationSpeed;

		if (elapsedTime >= dashTime)
		{
			actionMovement = 0;
			animation = "stun";

			unsafe { particles.systems[0].handle->emissionRate = 0; }

			speedMultiplier = player.isGrounded ? 0 : 1;
		}
		else
		{
			actionMovement = speed * player.equipLoadModifier * direction;
			/*
			if (direction == 1)
				player.velocity.x = Math.Max(player.velocity.x, speed * direction);
			else
				player.velocity.x = Math.Min(player.velocity.x, speed * direction);
			*/
			animation = "backhop";
		}
	}
}
