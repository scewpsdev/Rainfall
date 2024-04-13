using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


internal class MobItemEntity
{
	public Item item;
	public ParticleSystem particles;
	public RigidBody hitbox;

	Creature creature;
	int handID;

	Matrix transform;


	public MobItemEntity(Creature creature, int handID)
	{
		this.creature = creature;
		this.handID = handID;

		particles = new ParticleSystem(250);
		hitbox = new RigidBody(creature, RigidBodyType.Kinematic);
	}

	public void setItem(Item item)
	{
		this.item = item;

		if (item != null && item.particleEmissionRate > 0.0f)
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
		else
		{
			particles.emissionRate = 0.0f;
		}

		if (item != null)
		{
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
		}
		else
		{
			hitbox.clearColliders();
		}
	}

	void onContact(RigidBody other, ContactType contactType, MobAttackAction attackAction)
	{
		if (other.entity != creature && !attackAction.hitEntities.Contains(other))
		{
			int damage = (int)(creature.stats.damage);

			Entity otherEntity = other.entity as Entity;
			if (otherEntity is Player)
			{
				Player player = otherEntity as Player;
				player.hit(damage, creature);
			}
			else if (otherEntity is Creature)
			{
				Creature otherCreature = otherEntity as Creature;
				otherCreature.hit(damage, creature);
			}

			attackAction.hitEntities.Add(other);
		}
	}

	void checkCollision(MobAttackAction attackAction)
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
		if (item != null)
		{
			if (creature.currentAction != null && creature.currentAction.type == MobActionType.Attack)
			{
				MobAttackAction attackAction = (MobAttackAction)creature.currentAction;
				if (attackAction.handID == handID && attackAction.elapsedTime >= attackAction.damageTimeStart && attackAction.elapsedTime <= attackAction.damageTimeEnd)
				{
					checkCollision(attackAction);
				}
			}
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
	}

	public void draw(GraphicsDevice graphics)
	{
		if (item != null)
		{
			Renderer.DrawModel(item.model, transform);

			foreach (Light light in item.lights)
			{
				Matrix lightTransform = transform * Matrix.CreateTranslation(light.position);
				Renderer.DrawLight(lightTransform.translation, light.color);
			}

			particles.draw(graphics);
		}
	}
}
