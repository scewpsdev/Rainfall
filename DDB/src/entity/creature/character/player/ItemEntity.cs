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

	void onContact(RigidBody other, ContactType contactType, AttackAction attackAction)
	{
		if (!attackAction.hitEntities.Contains(other.entity))
		{
			Entity otherEntity = other.entity as Entity;
			if (otherEntity is Creature)
			{
				Creature creature = otherEntity as Creature;
				int damage = (int)(attackAction.item.baseDamage * attackAction.attack.damageMultiplier);
				creature.hit(damage, player);
				attackAction.hitEntities.Add(otherEntity);

				if (item.sfxHit != null)
					audio.playSoundOrganic(item.sfxHit);
			}
		}
	}

	void checkCollision(AttackAction attackAction)
	{
		foreach (Collider collider in item.colliders)
		{
			OverlapHit[] hits = new OverlapHit[16];
			int numHits = 0;
			bool hitFound = false;

			if (collider.type == ColliderType.Box)
			{
				Matrix shapeTransform = transform * Matrix.CreateTranslation(collider.offset);
				numHits = Physics.OverlapBox(collider.size * 0.5f, shapeTransform.translation, shapeTransform.rotation, hits, 16, QueryFilterFlags.Dynamic);
			}
			else if (collider.type == ColliderType.Sphere)
			{
				Matrix shapeTransform = transform * Matrix.CreateTranslation(collider.offset);
				numHits = Physics.OverlapSphere(collider.radius, shapeTransform.translation, hits, 16, QueryFilterFlags.Dynamic);
			}
			else if (collider.type == ColliderType.Capsule)
			{
				Matrix shapeTransform = transform * Matrix.CreateTranslation(collider.offset);
				numHits = Physics.OverlapCapsule(collider.radius, collider.size.y, shapeTransform.translation, shapeTransform.rotation, hits, 16, QueryFilterFlags.Dynamic);
			}
			else
			{
				Debug.Assert(false);
			}

			for (int i = 0; i < numHits; i++)
			{
				if (hits[i].body != null)
				{
					onContact(hits[i].body, ContactType.Found, attackAction);
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
		}

		if (currentModel != null && animator != null && animator.model == currentModel)
		{
			animator.update();
			animator.applyAnimation();
			//currentModel.applyAnimation(animator.nodeLocalTransforms);
		}

		particles.update();
	}

	public void setTransform(Matrix transform)
	{
		//position = transform.translation;
		//rotation = transform.rotation;
		this.transform = transform;
		particles.transform = transform;
		hitbox.setTransform(transform.translation, transform.rotation);
		audio.updateTransform(transform.translation);
	}

	public void draw(GraphicsDevice graphics)
	{
		if (currentModel != null)
		{
			Renderer.DrawModel(currentModel, transform, animator);

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
