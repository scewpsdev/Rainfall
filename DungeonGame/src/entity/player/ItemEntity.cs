using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


public class ItemEntity
{
	public Item item;
	public List<ParticleSystem> particles;
	public RigidBody hitbox;
	public AudioSource audio;

	public Model currentModel;
	Animator animator = null;
	bool flipTransform = false;

	Player player;
	int handID;

	public Matrix transform;
	Matrix renderScale;
	Matrix lastTransform;

	long lastSparkEffectTime = 0;


	public ItemEntity(Player player, int handID)
	{
		this.player = player;
		this.handID = handID;

		particles = new List<ParticleSystem>();
		hitbox = new RigidBody(player, RigidBodyType.Kinematic);
		audio = new AudioSource(player.position);
	}

	public void destroy()
	{
		hitbox.destroy();
		audio.destroy();
	}

	public void setItem(Item item)
	{
		this.item = item;

		hitbox.clearColliders();
		if (animator != null)
		{
			animator.destroy();
			animator = null;
		}

		particles.Clear();

		if (item != null)
		{
			foreach (ParticleSystem itemParticles in item.particleSystems)
			{
				ParticleSystem particleSystem = new ParticleSystem(256);
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

			hitbox.addSphereTrigger(0.2f, Vector3.Zero);

			animator = new Animator(item.model);
			animator.setAnimation(new AnimationState(item.model, "default"));
		}
		else
		{
			animator = null;
		}
	}

	void onContact(HitData hit, ContactType contactType, Vector3 contactPosition, AttackAction attackAction, Item item)
	{
		Entity otherEntity = hit.body.entity as Entity;

		if (contactType == ContactType.Found)
		{
			if (otherEntity is Hittable)
			{
				float knockbackMultiplier = attackAction.attack.type == AttackType.Heavy ? 1.5f : 1.0f;
				Vector3 force = (otherEntity.position - player.position).normalized * 2.0f * knockbackMultiplier; // TODO influenced by weapon weight
				int damage = (int)(attackAction.item.baseDamage * attackAction.attack.damageMultiplier);

				if (otherEntity is Creature)
				{
					Creature creature = otherEntity as Creature;
					if (creature.isAlive)
					{
						int linkID = creature.hitboxes.IndexOf(hit.body);
						creature.hit(damage, player, contactPosition, force, linkID);
					}
					else
					{
						hit.body.addForce(force);
					}
				}
				else
				{
					Hittable hittable = otherEntity as Hittable;
					hittable.hit(damage, player, contactPosition, force, 0);
				}
			}

			if (item.sfxHit != null)
			{
				audio.playSoundOrganic(item.sfxHit);
				AIManager.NotifySound(transform.translation, 4.0f);
			}
		}
		else if (contactType == ContactType.Persists)
		{
			//Entity otherEntity = hit.body.entity as Entity;
			//if (otherEntity == null)
			{
				if ((Time.currentTime - lastSparkEffectTime) / 1e9f > 0.016f)
				{
					Vector3 direction = hit.normal; // ((player.position - hit.position) * new Vector3(1.0f, 0.0f, 1.0f)).normalized;
					DungeonGame.instance.level.addEntity(new SparkEffect(direction), hit.position, Quaternion.Identity);
					lastSparkEffectTime = Time.currentTime;
				}
			}

			attackAction.onHit(otherEntity, hit, player, handID);
		}
	}

	void checkCollision(AttackAction attackAction, Item item)
	{
		Span<HitData> hits = stackalloc HitData[32];
		int numHits = 0;

		/*
		if (item.hitboxRange != 0)
		{
			if (item.hitbox.type == ColliderType.Box)
			{
				numHits += Physics.OverlapBox(item.hitbox.size * 0.5f, transform.translation, transform.rotation, hits, QueryFilterFlags.Default | QueryFilterFlags.NoBlock);
			}
			else if (item.hitbox.type == ColliderType.Sphere)
			{
				numHits += Physics.OverlapSphere(item.hitbox.radius, transform.translation, hits, QueryFilterFlags.Default | QueryFilterFlags.NoBlock);
			}
			else if (item.hitbox.type == ColliderType.Capsule)
			{
				numHits += Physics.OverlapCapsule(item.hitbox.radius, item.hitbox.size.y, transform.translation, transform.rotation, hits, QueryFilterFlags.Default | QueryFilterFlags.NoBlock);
			}

			numHits += Physics.Raycast(transform.translation, transform.rotation.up, item.hitboxRange, hits.Slice(numHits), QueryFilterFlags.Default | QueryFilterFlags.NoBlock);
		}
		else
		{
			Debug.Assert(false);
		}
		*/

		if (item.hitboxRange != 0)
		{
			Vector3 lastPosition = lastTransform.translation;
			Vector3 delta = transform.translation - lastPosition;
			float distance = delta.length;
			Vector3 direction = delta.lengthSquared > 0 ? delta / distance : Vector3.UnitX;
			Quaternion rotation = transform.rotation;

			if (item.hitbox.type == ColliderType.Box)
			{
				numHits = Physics.SweepBox(item.hitbox.size * 0.5f, lastPosition, rotation, direction, distance, hits, QueryFilterFlags.Default | QueryFilterFlags.NoBlock);
			}
			else if (item.hitbox.type == ColliderType.Sphere)
			{
				numHits = Physics.SweepSphere(item.hitbox.radius, lastPosition, direction, distance, hits, QueryFilterFlags.Default | QueryFilterFlags.NoBlock);
			}
			else if (item.hitbox.type == ColliderType.Capsule)
			{
				numHits = Physics.SweepCapsule(item.hitbox.radius, item.hitbox.size.y, lastPosition, rotation, direction, distance, hits, QueryFilterFlags.Default | QueryFilterFlags.NoBlock);
			}
			else
			{
				Debug.Assert(false);
			}
		}
		else
		{
			Debug.Assert(false);
		}

		float shortestDistance = 1000.0f;
		HitData hit = new HitData();
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].body != null &&
				(hits[i].body.filterMask & (uint)PhysicsFilterGroup.Weapon) != 0 &&
				hits[i].body.entity is not Player)
			{
				if (hits[i].distance < shortestDistance || hits[i].distance < shortestDistance + 0.1f && hits[i].position != Vector3.Zero)
				{
					shortestDistance = hits[i].distance;
					hit = hits[i];
				}
			}
		}

		if (hit.body != null)
		{
			Vector3 hitPosition = hit.position != Vector3.Zero ? hit.position : hit.body.getPosition();

			if (!attackAction.hitEntities.Contains(hit.body.entity))
			{
				onContact(hit, ContactType.Found, hitPosition, attackAction, item);
				attackAction.hitEntities.Add((Entity)hit.body.entity);
			}

			onContact(hit, ContactType.Persists, hitPosition, attackAction, item);
		}
	}

	public void update()
	{
		if (player.currentAction != null && player.currentAction.overrideHandModels[handID])
		{
			currentModel = player.currentAction.handItemModels[handID]?.model;
			flipTransform = player.currentAction.flipHandModels[handID];
		}
		else
		{
			currentModel = item != null ? item.model : null;
			flipTransform = false;
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

		//if (item != null)
		{
			if (player.currentAction != null)
			{
				if (player.currentAction.type == ActionType.Attack)
				{
					AttackAction attackAction = (AttackAction)player.currentAction;
					if (attackAction.handID == handID && attackAction.elapsedTime >= attackAction.attack.damageTimeStart && attackAction.elapsedTime <= attackAction.attack.damageTimeEnd)
					{
						checkCollision(attackAction, item != null ? item : Item.Get("default"));
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

		foreach (ParticleSystem particleSystem in particles)
		{
			particleSystem.update(particleSystem.transform);
		}
	}

	public void setTransform(Matrix transform, Matrix renderScale)
	{
		//position = transform.translation;
		//rotation = transform.rotation;
		transform *= Matrix.CreateRotation(Quaternion.FromAxisAngle(Vector3.UnitX, MathF.PI));
		if (item != null && item.flipOnLeft && handID == 1 || flipTransform)
		{
			transform *= Matrix.CreateRotation(Vector3.UnitY, MathF.PI);
		}
		this.transform = transform;
		this.renderScale = renderScale;

		foreach (ParticleSystem particleSystem in particles)
		{
			particleSystem.setTransform(renderScale * transform, true);
		}

		hitbox.setTransform(transform.translation, transform.rotation);
		audio.updateTransform(transform.translation + player.rotation.forward * 2); // adding forward vector to make it easier on the ears
	}

	public void draw(GraphicsDevice graphics)
	{
		if (currentModel != null)
		{
			if (item.renderModel)
				Renderer.DrawModel(currentModel, renderScale * transform, animator);

			if (item != null && currentModel == item.model)
			{
				foreach (ItemLight light in item.lights)
				{
					Matrix lightTransform = renderScale * transform * Matrix.CreateTranslation(light.position);
					Renderer.DrawLight(lightTransform.translation, light.color);
				}

				foreach (ParticleSystem particleSystem in particles)
				{
					Renderer.DrawParticleSystem(particleSystem);
				}
			}
		}
	}
}
