using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
		gravity = -50;
		awareness = 1;
		itemDropChance = 2;
		itemDropValueMultiplier = 2;

		sprite = new Sprite(Resource.GetTexture("sprites/mob/gardens/raya.png", false), 0, 0, 128, 64);
		collider = new FloatRect(-0.25f, 0, 0.5f, 1.4f);
		rect = new FloatRect(-4, -1, 8, 4);

		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 2, true);
		animator.addAnimation("run", 8, 1, true);
		animator.addAnimation("dash0", 11, 1, 1, false);
		animator.addAnimation("dash1", 13, 1, 1, false);
		animator.addAnimation("dash2", 6, 1, false);
		animator.addAnimation("jump0", 2, 1, false);
		animator.addAnimation("jump1", 1, 1, false);
		animator.addAnimation("jump2", 1, 1, false);
		animator.addAnimation("divejump0", 20, 2, 1, false);
		animator.addAnimation("divejump1", 1, 1, false);
		animator.addAnimation("divejump2", 1, 1, false);
		animator.addAnimation("dive0", 2, 1, false);
		animator.addAnimation("dive1", 1, 1, false);
		animator.addAnimation("dive2", 1, 1, false);
		animator.addAnimation("thrust0", 1, 1, false);
		animator.addAnimation("thrust1", 1, 1, false);
		animator.addAnimation("thrust2", 29, 1, 1, false);
		animator.setAnimation("idle");

		Sound[] stepSound = Resource.GetSounds("sounds/step", 6);
		animator.addAnimationEvent("run", 3, () => { Audio.Play(stepSound, new Vector3(position, 0)); });
		animator.addAnimationEvent("run", 7, () => { Audio.Play(stepSound, new Vector3(position, 0)); });

		AdvancedAI ai = new AdvancedAI(this);
		this.ai = ai;

		ai.hesitation = 5;

		Sound jumpSound = Resource.GetSound("sounds/jump_bare.ogg");
		Sound landSound = Resource.GetSound("sounds/land.ogg");
		Sound impactSound = Resource.GetSound("sounds/explosion.ogg");

		{
			const float dashDuration = 0.4f;
			const float dashDistance = 8;
			const float dashSpeed = dashDistance / dashDuration;
			const float dashTriggerDistance = 8;
			const float dashCharge = 1.0f;
			const float dashCooldown = 0.8f;

			AIAction dash = ai.addAction("dash", dashCharge, dashDuration, dashCooldown, dashSpeed, dashTriggerDistance);
			dash.actionColliders = [new FloatRect(0, 0, 2, 2)];

			dash.onStarted = (AIAction action) =>
			{
				level.addEntity(new MobWeaponTrail(this, new Vector2(7, 10) / 16.0f, MathF.PI * 0.75f, MathF.PI * -0.75f, 32 / 16.0f, 0.1f, dashDuration + dashCooldown));
				Audio.Play(Item.weaponSwing, new Vector3(ai.mob.position, 0));
			};
		}

		{
			const float thrustDuration = 0.2f;
			const float thrustDistance = 1;
			const float thrustSpeed = thrustDistance / thrustDuration;
			const float thrustCharge = 0.65f;
			const float thrustCooldown = 0.7f;

			AIAction thrust = ai.addAction("thrust", thrustCharge, thrustDuration, thrustCooldown, thrustSpeed, 4, 0);
			thrust.actionColliders = [new FloatRect(0, 0, 2.5f, 0.7f)];
			thrust.onStarted = (AIAction action) =>
			{
				Audio.Play(Item.weaponThrust, new Vector3(ai.mob.position, 0));
			};
		}

		{
			AIAction stepback = ai.addAction("jump", 0.2f, 100, 0.2f, -15, 5);
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

		{
			const float jumpCharge = 1.0f;
			const float jumpCooldown = 1.0f;
			const float jumpAttackSpeed = 10.0f;
			const float jumpMaxDistance = 24;
			const float jumpMinDistance = 4;

			AIAction jumpAttack = ai.addAction("jump", jumpCharge, 100, jumpCooldown, jumpAttackSpeed, jumpMaxDistance, jumpMinDistance);
			jumpAttack.onStarted = (AIAction action) =>
			{
				float time = MathF.Abs(ai.target.position.x - position.x) / jumpAttackSpeed;
				ai.mob.jumpPower = -gravity * 0.5f * time;
				ai.mob.inputJump = true;
				//jumpAttack.walkSpeed = MathF.Abs(ai.target.position.x - position.x) * 0.9f;
				Audio.Play(jumpSound, new Vector3(ai.mob.position, 0));
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
				Audio.Play(landSound, new Vector3(ai.mob.position, 0));
			};
		}

		{
			const float jumpCharge = 0.5f;
			const float jumpCooldown = 0.7f;
			const float jumpAttackSpeed = 30;
			const float jumpMaxDistance = 14;
			const float jumpMinDistance = 4;

			AIAction diveJump = ai.addAction("divejump", jumpCharge, 100, jumpCooldown, jumpAttackSpeed, jumpMaxDistance, jumpMinDistance);
			AIAction dive = ai.addAction("dive", 0.5f, 100, 1, 30, 0);

			diveJump.onStarted = (AIAction action) =>
			{
				float time = MathF.Abs(ai.target.position.x - position.x) / jumpAttackSpeed;
				diveJump.duration = time;
				//ai.mob.jumpPower = -gravity * time;
				ai.mob.jumpPower = 30;
				ai.mob.inputJump = true;
				//jumpAttack.walkSpeed = MathF.Abs(ai.target.position.x - position.x) * 0.9f;
				Audio.Play(jumpSound, new Vector3(ai.mob.position, 0));
			};
			diveJump.onAction = (AIAction action, float elapsed, Vector2 toTarget) =>
			{
				float xdist = MathF.Abs(ai.target.position.x - position.x);
				if (xdist < 1)
					return false;
				return true;
			};
			diveJump.onFinished = (AIAction action) =>
			{
				ai.triggerAction(dive);
				ai.mob.canFly = true;
			};

			dive.onStarted = (AIAction action) =>
			{
				ai.mob.speed = dive.walkSpeed;
				ai.actionDirection = 0;
			};
			dive.onAction = (AIAction action, float elapsed, Vector2 toTarget) =>
			{
				if (ai.mob.isGrounded)
					return false;

				ai.mob.inputDown = true;

				return true;
			};
			dive.onFinished = (AIAction action) =>
			{
				ai.mob.canFly = false;
				TileType tile = GameState.instance.level.getTile(ai.mob.position - new Vector2(0, 0.5f));
				if (tile != null)
					GameState.instance.level.addEntity(ParticleEffects.CreateImpactEffect(Vector2.Up, 6, 16, MathHelper.ARGBToVector(tile.particleColor).xyz), ai.mob.position + ai.mob.direction * Vector2.Right);

				GameState.instance.camera.addScreenShake(ai.mob.position + ai.mob.direction * Vector2.Right, 1, 1);

				Audio.Play(impactSound, new Vector3(ai.mob.position, 0));

				level.addEntity(new RayaBladeEffect(this), new Vector2(ai.target.position.x, ai.mob.position.y));
			};
		}
	}
}

public class RayaBladeEffect : Entity
{
	const float appearDelay = 0.5f;
	const float lingerDuration = 0.5f;
	const float stabDuration = 0.1f;

	Raya raya;
	Sprite sprite;

	long spawnTime;
	bool particlesSpawned = false;

	public RayaBladeEffect(Raya raya)
	{
		this.raya = raya;
		sprite = new Sprite(Resource.GetTexture("sprites/mob/gardens/raya_blade.png", false));
	}

	public override void init(Level level)
	{
		level.addEntity(ParticleEffects.CreateImpactEffect(Vector2.Up, 3, 10, Vector3.One), position);
		spawnTime = Time.currentTime;
	}

	public override void update()
	{
		float elapsed = (Time.currentTime - spawnTime) / 1e9f;
		if (elapsed >= appearDelay && elapsed < appearDelay + lingerDuration)
		{
			HitData[] hits = new HitData[16];
			int numHits = level.overlap(position + new Vector2(-0.125f, 0), position + new Vector2(0.125f, 2), hits, FILTER_PLAYER | FILTER_OBJECT | FILTER_MOB);
			for (int i = 0; i < numHits; i++)
			{
				if (hits[i].entity != null && hits[i].entity != this && hits[i].entity != raya && hits[i].entity is Hittable)
				{
					Hittable hittable = hits[i].entity as Hittable;
					hittable.hit(raya.damage, raya);
				}
			}

			if (!particlesSpawned)
			{
				level.addEntity(ParticleEffects.CreateImpactEffect(Vector2.Up, 8, 30, Vector3.One), position);
				GameState.instance.camera.addScreenShake(position, 1, 1);
				Audio.Play(Item.weaponHit, new Vector3(position, 0));

				particlesSpawned = true;
			}
		}
		if (elapsed >= appearDelay + lingerDuration)
			remove();
	}

	public override void render()
	{
		float elapsed = (Time.currentTime - spawnTime) / 1e9f;
		if (elapsed >= appearDelay)
		{
			float yanim = MathF.Min(elapsed - appearDelay, stabDuration) / stabDuration * 2;
			Renderer.DrawSprite(position.x - 1, position.y - 2 + yanim, 2, 2, sprite);
		}
	}
}
