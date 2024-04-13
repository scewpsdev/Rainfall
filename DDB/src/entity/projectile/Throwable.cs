using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Throwable : Entity
{
	const float SPEED = 16.0f;
	const float GRAVITY = -10.0f;


	Item item;

	Entity shooter;
	Model model;
	RigidBody body;

	Vector3 velocity;
	Vector3 currentOffset;

	bool hit = false;
	Entity hitTarget;


	public Throwable(Item item, Entity shooter, Vector3 direction, Vector3 offset)
	{
		this.item = item;
		this.shooter = shooter;

		model = item.model;

		velocity = direction * SPEED;
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

	public override void onContact(RigidBody other, ContactType contactType)
	{
		if (contactType != ContactType.Found)
			return;

		if (!(other.entity is Player))
		{
			hit = true;
			hitTarget = other.entity as Entity;

			level.addEntity(new Explosion(shooter), position, rotation);

			currentOffset = Vector3.Zero;

			body.clearColliders();
			remove();
		}
	}

	public override void update()
	{
		if (hit)
		{

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
