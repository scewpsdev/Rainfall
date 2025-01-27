using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Raya : Mob
{
	public Raya()
		: base("raya")
	{
		displayName = "Raya";
		nameSubtitle = "Third Knight of the King";

		health = 65;
		poise = 10;
		speed = 1.5f;
		damage = 2;
		jumpPower = 18;
		gravity = -35;
		awareness = 1;
		itemDropChance = 2;
		itemDropValueMultiplier = 2;

		sprite = new Sprite(Resource.GetTexture("sprites/mob/gardens/raya.png", false), 0, 0, 64, 64);
		collider = new FloatRect(-0.25f, 0, 0.5f, 1.4f);
		rect = new FloatRect(-2, -1, 4, 4);

		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 2, true);
		animator.addAnimation("run", 8, 1, true);
		animator.addAnimation("dash0", 3, 1, false);
		animator.addAnimation("dash1", 1, 1, false);
		animator.addAnimation("dash2", 6, 1, false);
		animator.addAnimation("jump0", 2, 1, false);
		animator.addAnimation("jump1", 1, 1, false);
		animator.addAnimation("jump2", 1, 1, false);
		animator.setAnimation("idle");

		AdvancedAI ai = new AdvancedAI(this);
		this.ai = ai;

		ai.hesitation = 1;

		{
			const float dashDuration = 0.2f;
			const float dashDistance = 8;
			const float dashSpeed = dashDistance / dashDuration;
			const float dashTriggerDistance = 6;
			const float dashCharge = 0.8f;
			const float dashCooldown = 0.7f;

			ai.addAction("dash", dashCharge, dashDuration, dashCooldown, dashSpeed, dashTriggerDistance);
		}

		{
			const float jumpCharge = 0.5f;
			const float jumpCooldown = 0.7f;
			const float jumpAttackSpeed = 10.0f;
			const float jumpMaxDistance = 24;
			const float jumpMinDistance = 3;

			AIAction jumpAttack = ai.addAction("jump", jumpCharge, 100, jumpCooldown, jumpAttackSpeed, jumpMaxDistance, jumpMinDistance);
			jumpAttack.onStarted = (AIAction action) =>
			{
				float time = MathF.Abs(ai.target.position.x - position.x) / jumpAttackSpeed;
				ai.mob.jumpPower = -gravity * 0.5f * time;
				ai.mob.inputJump = true;
				//jumpAttack.walkSpeed = MathF.Abs(ai.target.position.x - position.x) * 0.9f;
				speed = jumpAttack.walkSpeed;
			};
			jumpAttack.onAction = (AIAction action, float elapsed, Vector2 toTarget) =>
			{
				return !(!ai.mob.inputJump && ai.mob.isGrounded);
			};
			jumpAttack.onFinished = (AIAction action) =>
			{
				TileType tile = GameState.instance.level.getTile(ai.mob.position - new Vector2(0, 0.5f));
				if (tile != null)
					GameState.instance.level.addEntity(ParticleEffects.CreateImpactEffect(Vector2.Up, 6, 16, MathHelper.ARGBToVector(tile.particleColor).xyz), ai.mob.position + ai.mob.direction * Vector2.Right);
			};
		}
	}
}
