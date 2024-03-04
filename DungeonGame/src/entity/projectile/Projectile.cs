using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


public class Projectile : Entity
{
	protected float gravity = 0;
	protected int damage = 1;
	protected bool stickToWalls = false;
	protected bool piercing = false;
	protected Item itemDrop = null;

	public readonly Entity shooter;
	protected Model model;
	protected RigidBody body;

	protected Vector3 velocity;
	Vector3 currentOffset;

	bool hit = false;
	bool hitHandled = false;
	RigidBody hitTarget;
	Vector3 hitPosition;
	Creature hitTargetCreature;
	int hitTargetLinkID = -1;

	Matrix transformRelativeToHit;

	List<Entity> hitEntities = new List<Entity>();


	public Projectile(Vector3 velocity, Vector3 offset, Entity shooter)
	{
		this.velocity = velocity;
		currentOffset = offset;

		this.shooter = shooter;
	}

	public override void init()
	{
	}

	public override void destroy()
	{
		model?.destroy();
		body?.destroy();
	}

	int getLinkIDFromShape(RigidBody body)
	{
		if (body.entity is Creature)
		{
			Creature creature = body.entity as Creature;
			if (creature.isAlive)
			{
				for (int i = 0; i < creature.hitboxes.Count; i++)
				{
					if (creature.hitboxes[i] == body)
						return i;
				}
			}
			else
			{
				for (int i = 0; i < creature.ragdoll.hitboxes.Count; i++)
				{
					if (creature.ragdoll.hitboxes[i] == body)
						return i;
				}
			}
		}
		return -1;
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (hit)
			return;

		if (contactType == ContactType.Found)
		{
			if (other != null)
			{
				if (other.entity is not Player)
				{
					if (!hitEntities.Contains(other.entity))
					{
						hit = true;
						hitTarget = other;
						hitHandled = false;
						hitPosition = position - velocity.normalized * 0.02f; // + rotation.forward * 1.6f;
						hitTargetLinkID = getLinkIDFromShape(other);
						if (hitTarget.entity is Creature)
							hitTargetCreature = hitTarget.entity as Creature;

						hitEntities.Add((Entity)other.entity);

						AIManager.NotifySound(position, 4.0f);
					}
				}
			}
		}
		else if (contactType == ContactType.Lost)
		{
			if (other != null)
			{
				if (hitEntities.Contains(other.entity) && piercing)
					hitEntities.Remove((Entity)other.entity);
			}
		}
	}

	protected virtual void onProjectileHit(Entity entity)
	{
	}

	public override void update()
	{
		base.update();

		if (!hit || piercing)
		{
			velocity.y += 0.5f * gravity * Time.deltaTime;

			position += velocity * Time.deltaTime;
			rotation = Quaternion.LookAt(velocity);
			body.setTransform(position, rotation);

			currentOffset = Vector3.Lerp(currentOffset, Vector3.Zero, 3.0f * Time.deltaTime);
			base.particleOffset = currentOffset;

			velocity.y += 0.5f * gravity * Time.deltaTime;
		}
		if (hit && hitTarget != null)
		{
			if (!hitHandled)
			{
				Vector3 force = velocity * 0.1f;

				if (hitTarget.entity is Creature)
				{
					Creature creature = hitTarget.entity as Creature;
					if (creature.isAlive)
					{
						creature.hit(damage, this, hitPosition, force, hitTargetLinkID != -1 ? hitTargetLinkID : 0);
					}
					else
					{
						hitTarget.addForce(force);
					}
					if (!piercing)
						remove();
				}
				else if (hitTarget.entity is Hittable)
				{
					Hittable hittable = hitTarget.entity as Hittable;
					hittable.hit(damage, this, force, hitPosition, hitTargetLinkID != -1 ? hitTargetLinkID : 0);
					if (!piercing)
						remove();
				}
				else
				{
					if (!stickToWalls)
						remove();
				}

				onProjectileHit((Entity)hitTarget.entity);

				if (stickToWalls)
				//if (hitTarget.entity is Creature || hitTarget.entity == null)
				{
					hitTarget.getTransform(out Vector3 bodyPosition, out Quaternion bodyRotation);
					transformRelativeToHit = Matrix.CreateTransform(bodyPosition, bodyRotation).inverted * getModelMatrix();

					if (hitTarget.entity != null)
					{
						((Entity)hitTarget.entity).addRemoveCallback(() =>
						{
							hitTarget = null;
							hitTargetLinkID = -1;
							hitTargetCreature = null;
							remove();
							if (itemDrop != null)
								DungeonGame.instance.level.addEntity(new ItemPickup(itemDrop), position, rotation);
						});
					}
				}

				if (piercing)
				{
					hit = false;
					hitHandled = false;
				}
				else
				{
					hitHandled = true;
				}
			}
			else
			{
				if (stickToWalls)
				{
					if (hitTargetCreature != null)
					{
						if (hitTargetCreature.ragdoll != null)
							hitTarget = hitTargetCreature.ragdoll.hitboxes[hitTargetLinkID];
					}
					else
					{
						hitTarget = null;
					}

					if (hitTarget != null)
					{
						hitTarget.getTransform(out Vector3 bodyPosition, out Quaternion bodyRotation);
						Matrix hitBodyTransform = Matrix.CreateTransform(bodyPosition, bodyRotation);
						Matrix transform = hitBodyTransform * transformRelativeToHit;
						position = transform.translation;
						rotation = transform.rotation;
					}
				}
			}
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		if (model != null)
			Renderer.DrawModel(model, getModelMatrix(currentOffset));
	}
}
