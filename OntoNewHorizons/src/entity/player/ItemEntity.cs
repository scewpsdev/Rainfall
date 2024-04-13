using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


internal class ItemEntity
{
	public Item item;
	public ParticleSystem particles;
	public RigidBody hitbox;
	public AudioSource audio;

	public Model currentModel;
	Animator animator = null;

	Player player;
	int handID;

	Matrix transform;
	Matrix renderScale;
	Matrix lastTransform;

	long lastSparkEffectTime = 0;


	public ItemEntity(Player player, int handID)
	{
		this.player = player;
		this.handID = handID;

		particles = new ParticleSystem(250);
		hitbox = new RigidBody(player, RigidBodyType.Kinematic);
		audio = Audio.CreateSource(player.position);
	}

	public void setItem(Item item)
	{
		this.item = item;

		hitbox.clearColliders();

		if (item != null)
		{
			particles.emissionRate = 0.0f;
			if (item.particleEmissionRate > 0.0001f)
			{
				particles.textureAtlas = item.particleTexture;
				particles.atlasColumns = item.particleAtlasColumns;
				particles.frameWidth = item.particleFrameSize;
				particles.frameHeight = item.particleFrameSize;
				particles.numFrames = item.particleFrameCount;
				particles.emissionRate = item.particleEmissionRate;
				particles.lifetime = item.particleLifetime;
				particles.spawnOffset = item.particleSpawnOffset;
				particles.spawnRadius = item.particleSpawnRadius;
				particles.spawnShape = item.particleSpawnShape;
				particles.particleSize = item.particleSize;
				particles.followMode = item.particleFollowMode;
				particles.initialVelocity = item.particleInitialVelocity;
				particles.gravity = item.particleGravity;
				particles.additive = item.particleAdditive;
			}

			foreach (Collider collider in item.colliders)
			{
				if (collider.type == ColliderType.Box)
					hitbox.addBoxTrigger(collider.size * 0.5f, collider.offset, Quaternion.Identity);
				else if (collider.type == ColliderType.Sphere)
					hitbox.addSphereTrigger(collider.radius, collider.offset);
				else if (collider.type == ColliderType.Capsule)
					hitbox.addCapsuleTrigger(collider.radius, collider.size.y, collider.offset, Quaternion.Identity);
				else
				{
					Debug.Assert(false);
				}
			}

			animator = new Animator(item.model);
			animator.setState(new AnimationState(item.model, "default"));
		}
		else
		{
			particles.emissionRate = 0.0f;

			animator = null;
		}
	}

	void onContact(SweepHit hit, ContactType contactType, AttackAction attackAction)
	{
		if (contactType == ContactType.Found)
		{
			Entity otherEntity = hit.body.entity as Entity;
			if (otherEntity is Hittable)
			{
				Hittable hittable = otherEntity as Hittable;
				int damage = (int)(attackAction.item.baseDamage * attackAction.attack.damageMultiplier);
				hittable.hit(damage, player);

				if (item.sfxHit != null)
					audio.playSoundOrganic(item.sfxHit);
			}
		}
		else if (contactType == ContactType.Persists)
		{
			Entity otherEntity = hit.body.entity as Entity;
			if (otherEntity == null)
			{
				if ((Time.currentTime - lastSparkEffectTime) / 1e9f > 0.02f)
				{
					Vector3 direction = hit.normal; // ((player.position - hit.position) * new Vector3(1.0f, 0.0f, 1.0f)).normalized;
					OntoNewHorizons.instance.world.addEntity(new SparkEffect(direction), hit.position, Quaternion.Identity);
					lastSparkEffectTime = Time.currentTime;
				}
			}
		}
	}

	void checkCollision(AttackAction attackAction)
	{
		Span<SweepHit> hits = stackalloc SweepHit[16];

		foreach (Collider collider in item.colliders)
		{
			int numHits = 0;
			bool hitFound = false;

			Matrix shapeTransform = transform * Matrix.CreateTranslation(collider.offset);
			Vector3 lastPosition = lastTransform.translation;
			Vector3 nextPosition = shapeTransform.translation;
			Vector3 delta = nextPosition - lastPosition;
			float distance = delta.length;

			if (collider.type == ColliderType.Box)
			{
				numHits = Physics.SweepBox(collider.size * 0.5f, lastPosition, shapeTransform.rotation, delta / distance, distance, hits, QueryFilterFlags.Default);
			}
			else if (collider.type == ColliderType.Sphere)
			{
				numHits = Physics.SweepSphere(collider.radius, lastPosition, delta / distance, distance, hits, QueryFilterFlags.Default);
			}
			else if (collider.type == ColliderType.Capsule)
			{
				numHits = Physics.SweepCapsule(collider.radius, collider.size.y, lastPosition, shapeTransform.rotation, delta / distance, distance, hits, QueryFilterFlags.Default);
			}
			else
			{
				Debug.Assert(false);
			}

			for (int i = 0; i < numHits; i++)
			{
				if (hits[i].body != null)
				{
					if (!attackAction.hitEntities.Contains(hits[i].body.entity))
					{
						onContact(hits[i], ContactType.Found, attackAction);
						attackAction.hitEntities.Add((Entity)hits[i].body.entity);
					}

					onContact(hits[i], ContactType.Persists, attackAction);

					hitFound = true;
				}
			}

			if (hitFound)
			{
				break;
			}
		}
	}

	public void update()
	{
		if (player.currentAction != null && player.currentAction.overrideHandModels[handID])
		{
			currentModel = player.currentAction.handItemModels[handID];
		}
		else
		{
			currentModel = item != null ? item.model : null;
		}
		if (animator != null)
		{
			if (player.currentAction != null && player.currentAction.handItemAnimations[handID] != null)
			{
				animator.getState().layers[0].animationName = player.currentAction.handItemAnimations[handID];
				animator.setTimer(player.currentAction.elapsedTime);
			}
			else
			{
				animator.getState().layers[0].animationName = "default";
			}
		}

		if (item != null)
		{
			if (player.currentAction != null)
			{
				if (player.currentAction.type == ActionType.Attack)
				{
					AttackAction attackAction = (AttackAction)player.currentAction;
					if (attackAction.handID == handID && attackAction.elapsedTime >= attackAction.damageTimeStart && attackAction.elapsedTime <= attackAction.damageTimeEnd)
					{
						checkCollision(attackAction);
					}
				}
			}
			lastTransform = transform;
		}

		if (currentModel != null && animator != null && animator.model == currentModel)
		{
			animator.update();
			animator.applyAnimation();
			//currentModel.applyAnimation(animator.nodeLocalTransforms);
		}

		particles.update();
	}

	public void setTransform(Matrix transform, Matrix renderScale)
	{
		//position = transform.translation;
		//rotation = transform.rotation;
		this.transform = transform;
		this.renderScale = renderScale;
		particles.transform = transform;
		hitbox.setTransform(transform.translation, transform.rotation);
		audio.updateTransform(transform.translation + player.rotation.forward); // adding forward vector to make it easier on the ears
	}

	public void draw(GraphicsDevice graphics)
	{
		if (currentModel != null)
		{
			Renderer.DrawModel(currentModel, renderScale * transform, animator);

			if (item != null && currentModel == item.model)
			{
				foreach (Light light in item.lights)
				{
					Matrix lightTransform = transform * Matrix.CreateTranslation(light.position);
					Renderer.DrawLight(lightTransform.translation, light.color);
				}

				particles.draw(graphics);
			}
		}
	}
}
