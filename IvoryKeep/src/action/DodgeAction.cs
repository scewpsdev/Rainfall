using Rainfall;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DodgeAction : EntityAction
{
	const float cooldownTime = 0.2f;
	const float dashTime = 0.2f;
	const float dashDistance = 3;
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

		turnToCrosshair = false;
	}

	public override void onStarted(Player player)
	{
		direction = player.direction;
		player.velocity.y = player.jumpPower * 0.5f;
		Audio.Play(player.jumpSound, new Vector3(player.position, 0));

		GameState.instance.level.addEntity(particles = ParticleEffects.CreateDeathEffect(player, direction), player.position);
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
		}
		else
		{
			actionMovement = speed * direction;
			animation = "backhop";
		}
	}
}
