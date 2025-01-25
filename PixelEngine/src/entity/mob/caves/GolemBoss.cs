using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GolemBoss : Mob
{
	public GolemBoss()
		: base("golem_boss")
	{
		displayName = "Grk";

		sprite = new Sprite(Resource.GetTexture("sprites/golem_boss.png", false), 0, 0, 128, 64);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 4, 2, true);
		animator.addAnimation("run", 4, 1, true);
		animator.addAnimation("jump0", 1, 1, false);
		animator.addAnimation("jump1", 1, 1, false);
		animator.addAnimation("jump2", 1, 1, false);
		animator.addAnimation("dash0", 2, 1, false);
		animator.addAnimation("dash1", 3, 1, false);
		animator.addAnimation("dash2", 2, 1, false);
		animator.addAnimation("slam0", 2, 1, false);
		animator.addAnimation("slam1", 1, 1, false);
		animator.addAnimation("slam2", 3, 1, false);
		animator.addAnimation("dead", 1, 1, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.375f, 0.0f, 0.75f, 1.8f);
		rect = new FloatRect(-4, -1, 8, 4);

		health = 40;
		poise = 4;
		speed = 3;
		damage = 1.0f;
		jumpPower = 16;
		gravity = -25;
		awareness = 1;
		itemDropChance = 2;
		itemDropValueMultiplier = 2;

		AdvancedAI ai = new AdvancedAI(this);
		this.ai = ai;

		ai.loseRange = 100;
		ai.patrol = false;
		ai.hesitation = 2;


		const float jumpSpeed = 1.5f;


		{
			const float slamCharge = 1.0f;
			const float slamCooldown = 1.0f;
			const float slamDistance = 1;
			const float slamDuration = 0.1f;
			const float slamTrigger = 3;

			AIAction slam = ai.addAction("slam1", slamDuration, "slam0", slamCharge, "slam2", slamCooldown, slamDistance / slamDuration, (AIAction action, Vector2 toTarget, float targetDistance) => targetDistance < slamTrigger && ai.canSeeTarget);
			slam.onFinished = (AIAction action) =>
			{
				TileType tile = GameState.instance.level.getTile(ai.mob.position - new Vector2(0, 0.5f));
				if (tile != null)
					GameState.instance.level.addEntity(ParticleEffects.CreateImpactEffect(Vector2.Up, 6, 40, MathHelper.ARGBToVector(tile.particleColor).xyz), ai.mob.position + ai.mob.direction * Vector2.Right);
				GameState.instance.camera.addScreenShake(ai.mob.position + ai.mob.direction * Vector2.Right, 1, 1);
			};
			slam.actionColliders = [new FloatRect(0, 0, 1, 3), new FloatRect(0, 0, 2, 1)];
		}

		{
			const float dashChargeTime = 1.25f;
			const float dashCooldownTime = 1.0f;
			const float dashSpeed = 16.0f;
			const float dashDistance = 6;
			const float dashTriggerDistance = 6;

			AIAction dash = ai.addAction("dash1", dashDistance / dashSpeed, "dash0", dashChargeTime, "dash2", dashCooldownTime, dashSpeed, (AIAction action, Vector2 toTarget, float targetDistance) => targetDistance < dashTriggerDistance && ai.canSeeTarget);
			dash.onFinished = (AIAction action) =>
			{
				TileType tile = GameState.instance.level.getTile(ai.mob.position - new Vector2(0, 0.5f));
				if (tile != null)
					GameState.instance.level.addEntity(ParticleEffects.CreateImpactEffect(Vector2.Up, 6, 40, MathHelper.ARGBToVector(tile.particleColor).xyz), ai.mob.position + ai.mob.direction * Vector2.Right);
				GameState.instance.camera.addScreenShake(ai.mob.position + ai.mob.direction * Vector2.Right, 1, 1);
			};
			dash.actionColliders = [new FloatRect(0, 0, 3, 2)];
		}

		{
			const float jumpAttackSpeed = 7.0f;
			const float jumpTriggerDistance = 16;

			AIAction jumpAttack = ai.addAction("jump1", 100, "jump0", 0.5f, "jump2", 1, jumpAttackSpeed, (AIAction action, Vector2 toTarget, float targetDistance) => targetDistance < jumpTriggerDistance && isGrounded && ai.canSeeTarget);
			jumpAttack.onStarted = (AIAction action) =>
			{
				ai.mob.inputJump = true;
			};
			jumpAttack.onAction = (AIAction action, float elapsed, Vector2 toTarget) =>
			{
				return !(!ai.mob.inputJump && ai.mob.isGrounded);
			};
			jumpAttack.onFinished = (AIAction action) =>
			{
				TileType tile = GameState.instance.level.getTile(ai.mob.position - new Vector2(0, 0.5f));
				if (tile != null)
					GameState.instance.level.addEntity(ParticleEffects.CreateImpactEffect(Vector2.Up, 6, 40, MathHelper.ARGBToVector(tile.particleColor).xyz), ai.mob.position + ai.mob.direction * Vector2.Right);
				GameState.instance.camera.addScreenShake(ai.mob.position + ai.mob.direction * Vector2.Right, 1, 1);

				Player player = GameState.instance.player;

				float stunRange = 3;
				if ((player.center - ai.mob.center).length < stunRange && player.isGrounded)
					player.stun();
			};
		}

		AIAction jump = ai.addAction("jump1", 100, 0, 0, jumpSpeed, (AIAction action, Vector2 toTarget, float targetDistance) =>
		{
			TileType forwardTile = GameState.instance.level.getTile(ai.mob.position + new Vector2(1.0f * action.ai.walkDirection, 0.5f));
			TileType forwardUpTile = GameState.instance.level.getTile(ai.mob.position + new Vector2(1.0f * action.ai.walkDirection, 1.5f));
			return forwardTile != null && forwardUpTile == null;
		});
		jump.onStarted = (AIAction action) =>
		{
			ai.mob.inputJump = true;
		};
		jump.onAction = (AIAction action, float elapsed, Vector2 toTarget) =>
		{
			return !(!ai.mob.inputJump && ai.mob.isGrounded);
		};
	}

	public override void init(Level level)
	{
		AdvancedAI ai = this.ai as AdvancedAI;
		ai.triggerAction(ai.getAction("jump1"));
	}
}
