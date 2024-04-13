using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Arrow : Entity, Interactable
{
	const float ARROW_SPEED = 20.0f;
	const float GRAVITY = -5.0f;


	Item item;
	Item bow;

	Entity shooter;
	Model model;
	RigidBody body;

	Vector3 velocity;
	Vector3 currentOffset;

	bool hit = false;
	Entity hitTarget;


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
		body = new RigidBody(this, RigidBodyType.Kinematic);
		body.addBoxTrigger(new Vector3(0.02f, 0.02f, 0.8f), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.Identity);
	}

	public override void destroy()
	{
		body.destroy();
	}

	public bool canInteract()
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

	public override void onContact(RigidBody body, CharacterController controller, ContactType contactType, bool trigger)
	{
		if (contactType != ContactType.Found)
			return;

		if (body != null)
		{
			if (body.entity is not Player)
			{
				hit = true;
				hitTarget = body.entity as Entity;
			}
		}
	}

	public override void update()
	{
		if (hit && hitTarget != null)
		{
			if (hitTarget is Creature)
			{
				Creature creature = hitTarget as Creature;
				creature.hit(bow.baseDamage, shooter);
				remove();
			}

			float speed = velocity.length;
			Vector3 raycastDir = velocity / speed;
			Vector3 raycastStart = position - velocity * Time.deltaTime;
			Span<RaycastHit> hits = stackalloc RaycastHit[16];
			int numHits = Physics.Raycast(raycastStart, raycastDir, speed * Time.deltaTime, hits, QueryFilterFlags.Default);
			for (int i = 0; i < numHits; i++)
			{
				RaycastHit hit = hits[i];
				if (hit.body.entity == hitTarget)
				{
					position = hit.position - raycastDir * 0.7f;
					break;
				}
			}

			body.clearColliders();

			currentOffset = Vector3.Zero;

			hitTarget = null;
		}
		if (!hit)
		{
			velocity.y += 0.5f * GRAVITY * Time.deltaTime;

			position += velocity * Time.deltaTime;
			rotation = Quaternion.LookAt(velocity);
			body.setTransform(position, rotation);

			currentOffset = Vector3.Lerp(currentOffset, Vector3.Zero, 10.0f * Time.deltaTime);

			velocity.y += 0.5f * GRAVITY * Time.deltaTime;
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix(currentOffset));
	}
}
