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
	public List<ParticleSystem> particles;
	public RigidBody hitbox;

	Creature creature;
	int handID;

	Matrix transform;


	public MobItemEntity(Creature creature, int handID)
	{
		this.creature = creature;
		this.handID = handID;

		particles = new List<ParticleSystem>();
		hitbox = new RigidBody(creature, RigidBodyType.Kinematic);
	}

	public void destroy()
	{
		hitbox.destroy();
	}

	public void setItem(Item item)
	{
		this.item = item;

		particles.Clear();

		if (item != null)
		{
			foreach (ParticleSystem itemParticles in item.particleSystems)
			{
				ParticleSystem particleSystem = new ParticleSystem(itemParticles.maxParticles);
				particleSystem.copyData(itemParticles);
				particles.Add(particleSystem);
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
		}
		else
		{
			hitbox.clearColliders();
		}
	}

	void onContact(RigidBody body, CharacterController controller, ContactType contactType, Vector3 contactPosition, MobAttackAction attackAction)
	{
		if (body != null && body.entity != creature && !attackAction.hitEntities.Contains(body.entity))
		{
			int damage = (int)(creature.stats.damage);

			Entity otherEntity = body.entity as Entity;
			if (otherEntity is Player)
			{
				Player player = otherEntity as Player;
				player.hit(damage, creature);
			}
			else if (otherEntity is Creature)
			{
				Creature otherCreature = otherEntity as Creature;
				otherCreature.hit(damage, creature, contactPosition, Vector3.Zero, 0);
			}

			attackAction.hitEntities.Add((Entity)body.entity);
		}
	}

	void checkCollision(MobAttackAction attackAction)
	{
		Span<HitData> hits = stackalloc HitData[16];

		foreach (Collider collider in item.colliders)
		{
			int numHits = 0;
			bool hitFound = false;

			if (collider.type == ColliderType.Box)
			{
				Matrix shapeTransform = transform * Matrix.CreateTranslation(collider.offset);
				numHits = Physics.OverlapBox(collider.size * 0.5f, shapeTransform.translation, shapeTransform.rotation, hits, QueryFilterFlags.Default);
			}
			else if (collider.type == ColliderType.Sphere)
			{
				Matrix shapeTransform = transform * Matrix.CreateTranslation(collider.offset);
				numHits = Physics.OverlapSphere(collider.radius, shapeTransform.translation, hits, QueryFilterFlags.Dynamic);
			}
			else if (collider.type == ColliderType.Capsule)
			{
				Matrix shapeTransform = transform * Matrix.CreateTranslation(collider.offset);
				numHits = Physics.OverlapCapsule(collider.radius, collider.size.y, shapeTransform.translation, shapeTransform.rotation, hits, QueryFilterFlags.Dynamic);
			}
			else
			{
				Debug.Assert(false);
			}

			for (int i = 0; i < numHits; i++)
			{
				if (hits[i].body != null)
				{
					onContact(hits[i].body, hits[i].controller, ContactType.Found, hits[i].position, attackAction);
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

		foreach (ParticleSystem particleSystem in particles)
		{
			particleSystem.update();
		}
	}

	public void setTransform(Matrix transform)
	{
		//position = transform.translation;
		//rotation = transform.rotation;
		this.transform = transform;

		foreach (ParticleSystem particleSystem in particles)
		{
			particleSystem.transform = transform;
		}

		hitbox.setTransform(transform.translation, transform.rotation);
	}

	public void draw(GraphicsDevice graphics)
	{
		if (item != null)
		{
			Renderer.DrawModel(item.model, transform);

			foreach (ItemLight light in item.lights)
			{
				Matrix lightTransform = transform * Matrix.CreateTranslation(light.position);
				Renderer.DrawLight(lightTransform.translation, light.color);
			}

			foreach (ParticleSystem particleSystem in particles)
			{
				particleSystem.draw(graphics);
			}
		}
	}
}
