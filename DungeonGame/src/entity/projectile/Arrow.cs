using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Arrow : Entity, Interactable
{
	const float ARROW_SPEED = 30.0f;
	const float GRAVITY = -5.0f;


	Item item;
	Item bow;

	public readonly Entity shooter;
	Model model;
	RigidBody body;

	Vector3 velocity;
	Vector3 currentOffset;

	bool hit = false;
	bool hitHandled = false;
	RigidBody hitTarget;
	Creature hitTargetCreature;
	int hitTargetLinkID = -1;

	Matrix transformRelativeToHit;


	public Arrow(Item item, Item bow, Entity shooter, Vector3 direction, Vector3 offset)
	{
		this.item = item;
		this.bow = bow;
		this.shooter = shooter;

		model = item.model;

		velocity = direction * ARROW_SPEED;
		currentOffset = offset;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Kinematic, (uint)PhysicsFilterGroup.Weapon | (uint)PhysicsFilterGroup.Interactable, (uint)PhysicsFilterMask.Weapon);
		body.addBoxTrigger(new Vector3(0.02f, 0.02f, 0.8f), new Vector3(0.0f, 0.0f, -0.6f), Quaternion.Identity);
	}

	public override void destroy()
	{
		if (body != null)
			body.destroy();
	}

	public bool canInteract(Entity by)
	{
		return true;
	}

	public void interact(Entity by)
	{
		if (by is Player)
		{
			Player player = by as Player;
			player.inventory.addItem(item, 1);
			remove();
		}
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
		/*
		else if (body.entity is RagdollEntity)
		{
			RagdollEntity ragdoll = body.entity as RagdollEntity;
			for (int i = 0; i < ragdoll.ragdoll.hitboxes.Count; i++)
			{
				if (ragdoll.ragdoll.hitboxes[i] == body)
					return i;
			}
		}
		*/
		return -1;
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (hit)
			return;

		if (contactType != ContactType.Found)
			return;

		if (other != null)
		{
			if (other.entity is not Player)
			{
				hit = true;
				hitTarget = other;
				hitTargetLinkID = getLinkIDFromShape(other);
				if (hitTarget.entity is Creature)
					hitTargetCreature = hitTarget.entity as Creature;

				AIManager.NotifySound(position, 4.0f);
			}
		}
	}

	public override void update()
	{
		if (!hit)
		{
			velocity.y += 0.5f * GRAVITY * Time.deltaTime;

			position += velocity * Time.deltaTime;
			rotation = Quaternion.LookAt(velocity);
			body.setTransform(position, rotation);

			currentOffset = Vector3.Lerp(currentOffset, Vector3.Zero, 10.0f * Time.deltaTime);

			velocity.y += 0.5f * GRAVITY * Time.deltaTime;
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
						creature.hit(bow.baseDamage, this, force, hitTargetLinkID != -1 ? hitTargetLinkID : 0);
						//remove();
					}
					else
					{
						hitTarget.addForce(force);
					}
				}
				else if (hitTarget.entity is Hittable)
				{
					Hittable hittable = hitTarget.entity as Hittable;
					hittable.hit(bow.baseDamage, this, force, hitTargetLinkID != -1 ? hitTargetLinkID : 0);
				}

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
							DungeonGame.instance.level.addEntity(new ItemPickup(Item.Get("arrow")), position, rotation);
						});
					}

					body.clearColliders();
					body.destroy();
					body = null;
				}
				//else
				{
					//remove();
					//DungeonGame.instance.level.addEntity(new ItemPickup(Item.Get("arrow")), position, rotation);
				}

				currentOffset = Vector3.Zero;

				hitHandled = true;
			}
			else
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

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix(currentOffset));
	}
}
