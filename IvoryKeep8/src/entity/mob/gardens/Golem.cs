using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;


public class Golem : Mob
{
	public Golem()
		: base("golem")
	{
		displayName = "Golem";

		sprite = new Sprite(Resource.GetTexture("sprites/golem.png", false), 0, 0, 48, 48);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 4, 2, true);
		animator.addAnimation("run", 4, 1, true);
		animator.addAnimation("jump1", 1, 1, false);
		animator.addAnimation("attack0", 1, 1, false);
		animator.addAnimation("attack1", 2, 1, false);
		animator.addAnimation("attack2", 1, 1, false);
		animator.addAnimation("dead", 1, 1, true);
		animator.addAnimation("dead_falling", 1, 1, true);
		animator.setAnimation("idle");

		collider = new Hitbox(-0.5f, 0.0f, 1.0f, 1.8f);
		rect = new FloatRect(-1.5f, 0, 3, 3);

		health = 20;
		poise = 3;
		speed = 3;
		damage = 1.5f;
		jumpPower = 10;
		gravity = -20;
		itemDropChance = 0.8f;
		//relicDropChance = 0.5f;

		AdvancedAI ai = new AdvancedAI(this);
		this.ai = ai;

		ai.loseRange = 100;
		ai.patrol = false;
		//ai.hesitation = 4;
		ai.hesitation = 3;
		ai.minRunDistance = 4;
		ai.awareness = 1;


		Sound impactSound = Resource.GetSound("sounds/explosion.ogg");
		Sound jumpSound = Resource.GetSound("sounds/jump_bare.ogg");
		Sound landSound = Resource.GetSound("sounds/land.ogg");


		{
			const float slamCharge = 1.2f;
			const float slamCooldown = 1.0f;
			const float slamDistance = 1;
			const float slamDuration = 0.1f;
			const float slamTrigger = 4;

			AIAction slam = ai.addAction("attack", slamCharge, slamDuration, slamCooldown, slamDistance / slamDuration, slamTrigger);
			slam.onStarted = (AIAction action) =>
			{
				level.addEntity(new MobWeaponTrail(this, new Vector2(0, 0), MathF.PI * 0.5f, 0, 3, 0.1f, slamDuration + slamCooldown, false));
				Audio.Play(Item.weaponSwing, new Vector3(ai.mob.position, 0), 1, 0.5f);
			};
			slam.onFinished = (AIAction action) =>
			{
				TileType tile = GameState.instance.level.getTile(ai.mob.position - new Vector2(0, 0.5f));
				if (tile != null)
					GameState.instance.level.addEntity(ParticleEffects.CreateImpactEffect(Vector2.Up, 6, 40, Mathf.ARGBToVector(tile.particleColor).xyz), ai.mob.position + ai.mob.direction * Vector2.Right);
				GameState.instance.camera.addScreenShake(ai.mob.position, 1, 1);
				Audio.Play(impactSound, new Vector3(ai.mob.position, 0));
			};
			slam.actionColliders = [new FloatRect(0, 0, 1, 3), new FloatRect(0, 0, 2, 1)];
		}

		{
			const float jumpAttackSpeed = 7.0f;
			const float jumpTriggerDistance = 16;

			AIAction jumpAttack = ai.addAction("jump", 0.0f, 100, 0.0f, jumpAttackSpeed, jumpTriggerDistance, 5);
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
					GameState.instance.level.addEntity(ParticleEffects.CreateImpactEffect(Vector2.Up, 6, 40, Mathf.ARGBToVector(tile.particleColor).xyz), ai.mob.position + ai.mob.direction * Vector2.Right);
				GameState.instance.camera.addScreenShake(ai.mob.position + ai.mob.direction * Vector2.Right, 1, 1);

				Audio.Play(impactSound, new Vector3(ai.mob.position, 0));

				Player player = GameState.instance.player;

				float stunRange = 3;
				if ((player.center - ai.mob.center).length < stunRange && player.isGrounded)
					player.stun(this);
			};
		}

		{
			AIAction stepback = ai.addAction("jump", 0.0f, 100, 0.0f, -8, 5);
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
}
