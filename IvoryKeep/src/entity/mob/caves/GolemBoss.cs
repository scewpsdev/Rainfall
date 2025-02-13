using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GolemBoss : Mob
{
	int phase = 0;

	AIAction jumpAttack;


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
		animator.addAnimation("dash0", 1, 1, false);
		animator.addAnimation("dash1", 1, 1, false);
		animator.addAnimation("dash2", 5, 1, false);
		animator.addAnimation("slam0", 2, 1, false);
		animator.addAnimation("slam1", 1, 1, false);
		animator.addAnimation("slam2", 3, 1, false);
		animator.addAnimation("dead", 1, 1, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.375f, 0.0f, 0.75f, 1.8f);
		rect = new FloatRect(-4, -1, 8, 4);

		health = 40;
		poise = 10;
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
		//ai.hesitation = 4;
		ai.hesitation = 4;
		ai.minRunDistance = 4;


		Sound impactSound = Resource.GetSound("sounds/explosion.ogg");
		Sound jumpSound = Resource.GetSound("sounds/jump_bare.ogg");
		Sound landSound = Resource.GetSound("sounds/land.ogg");


		const float jumpSpeed = 1.5f;


		{
			const float slamCharge = 1.2f;
			const float slamCooldown = 1.0f;
			const float slamDistance = 1;
			const float slamDuration = 0.1f;
			const float slamTrigger = 4;

			AIAction slam = ai.addAction("slam", slamCharge, slamDuration, slamCooldown, slamDistance / slamDuration, slamTrigger, 0);
			slam.onStarted = (AIAction action) =>
			{
				level.addEntity(new MobWeaponTrail(this, new Vector2(0, 0), MathF.PI * 0.5f, 0, 3, 0.1f, slamDuration + slamCooldown, false));
				Audio.Play(Item.weaponSwing, new Vector3(ai.mob.position, 0), 1, 0.5f);
			};
			slam.onFinished = (AIAction action) =>
			{
				TileType tile = GameState.instance.level.getTile(ai.mob.position - new Vector2(0, 0.5f));
				if (tile != null)
					GameState.instance.level.addEntity(ParticleEffects.CreateImpactEffect(Vector2.Up, 6, 40, MathHelper.ARGBToVector(tile.particleColor).xyz), ai.mob.position + ai.mob.direction * Vector2.Right);
				GameState.instance.camera.addScreenShake(ai.mob.position, 1, 1);
				Audio.Play(impactSound, new Vector3(ai.mob.position, 0));
			};
			slam.actionColliders = [new FloatRect(0, 0, 1, 3), new FloatRect(0, 0, 2, 1)];
		}

		{
			const float dashChargeTime = 1.25f;
			const float dashCooldownTime = 1.0f;
			const float dashDistance = 6;
			const float dashDuration = 0.25f;
			const float dashSpeed = dashDistance / dashDuration;
			const float dashTriggerDistance = 8;

			AIAction dash = ai.addAction("dash", dashChargeTime, dashDuration, dashCooldownTime, dashSpeed, dashTriggerDistance, 3);
			dash.onStarted = (AIAction action) =>
			{
			};
			dash.onFinished = (AIAction action) =>
			{
				TileType tile = GameState.instance.level.getTile(ai.mob.position - new Vector2(0, 0.5f));
				if (tile != null)
					GameState.instance.level.addEntity(ParticleEffects.CreateImpactEffect(Vector2.Up, 6, 40, MathHelper.ARGBToVector(tile.particleColor).xyz), ai.mob.position + ai.mob.direction * Vector2.Right);
				GameState.instance.camera.addScreenShake(ai.mob.position, 1, 1);

				level.addEntity(new MobWeaponTrail(this, new Vector2(21, 21) / 16.0f, MathF.PI * -0.75f, MathF.PI * 0.75f, 32 / 16.0f, 0.1f, dashDistance / dashSpeed + dashCooldownTime));
				Audio.Play(Item.weaponSwing, new Vector3(ai.mob.position, 0), 1, 0.5f);
			};
			dash.actionColliders = [new FloatRect(0, 0, 3, 2)];
		}

		{
			const float jumpAttackSpeed = 7.0f;
			const float jumpTriggerDistance = 16;

			jumpAttack = ai.addAction("jump", 0.5f, 100, 1, jumpAttackSpeed, jumpTriggerDistance, 5);
			jumpAttack.onStarted = (AIAction action) =>
			{
				ai.mob.inputJump = true;
				ai.mob.jumpPower = 16;
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

				Audio.Play(impactSound, new Vector3(ai.mob.position, 0));

				Player player = GameState.instance.player;

				float stunRange = 3;
				if ((player.center - ai.mob.center).length < stunRange && player.isGrounded)
					player.stun(this);
			};
		}

		{
			AIAction stepback = ai.addAction("jump", 0.2f, 100, 0.2f, -8, 5);
			stepback.onStarted = (AIAction action) =>
			{
				ai.mob.inputJump = true;
				ai.mob.jumpPower = 10;

				Audio.Play(jumpSound, new Vector3(ai.mob.position, 0));
			};
			stepback.onAction = (AIAction action, float elapsed, Vector2 toTarget) =>
			{
				if (!ai.mob.inputJump && ai.mob.isGrounded)
					return false;
				//ai.actionDirection = 0;
				//ai.mob.actionInput = new Vector2(-ai.mob.direction, 0);
				return true;
			};
			stepback.onFinished = (AIAction action) =>
			{
				Audio.Play(landSound, new Vector3(ai.mob.position, 0));
			};
		}
	}

	public override void init(Level level)
	{
		base.init(level);

		AdvancedAI ai = this.ai as AdvancedAI;
		ai.triggerAction(jumpAttack);
		ai.actionDirection = 1;
	}

	public override void update()
	{
		base.update();

		AdvancedAI ai = this.ai as AdvancedAI;

		if (health < maxHealth / 2 && phase == 0)
		{
			ai.hesitation = 2;
			speed = 8;
			foreach (AIAction action in ai.actions)
			{
				action.chargeTime *= 0.8f;
				action.walkSpeed *= 1.35f;
			}
			GameState.instance.currentBossRoom.onPhaseTransition();
			phase++;
		}

		if (!isGrounded)
		{
			bool hitsWall = false;
			for (int i = (int)MathF.Floor(collider.min.y + 0.01f); i <= (int)MathF.Floor(collider.max.y - 0.01f); i++)
			{
				TileType forwardTile = GameState.instance.level.getTile(position + new Vector2(ai.walkDirection == 1 ? collider.max.x + 0.1f : ai.walkDirection == -1 ? collider.min.x - 0.1f : 0, 0.5f + i));
				if (forwardTile != null && forwardTile.isSolid)
				{
					hitsWall = true;
					break;
				}
			}
			if (hitsWall)
			{
				ai.walkDirection *= -1;
				ai.actionDirection *= -1;
			}
		}
	}
}
