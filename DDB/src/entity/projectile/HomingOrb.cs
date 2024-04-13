using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class HomingOrb : Entity
{
	const float SPEED = 6.0f;


	Player player;
	int damage;

	Model model;
	RigidBody body;

	Vector3 velocity;

	bool hit = false;

	List<Entity> hitEntities = new List<Entity>();


	public HomingOrb(Player player, int damage)
	{
		this.player = player;
		this.damage = damage;

		model = Resource.GetModel("res/entity/projectile/magic_orb/magic_orb.gltf");

		velocity = player.lookDirection * SPEED;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Kinematic);
		body.addSphereTrigger(0.05f, Vector3.Zero);
	}

	public override void destroy()
	{
		body.destroy();
	}

	public override void onContact(RigidBody other, ContactType contactType)
	{
		if (hit)
			return;

		if (other.entity is not Player && !hitEntities.Contains(other.entity))
		{
			if (other.entity is Creature)
			{
				Creature creature = other.entity as Creature;
				creature.hit(damage, player);
			}

			hitEntities.Add((Entity)other.entity);

			remove();

			hit = true;
		}
	}

	public override void update()
	{
		Vector3 newVelocity = player.lookDirection * SPEED;

		velocity = Vector3.Lerp(velocity, newVelocity, 1.0f * Time.deltaTime);

		Vector3 projectedPosition = player.lookOrigin + Vector3.Dot(position - player.lookOrigin, player.lookDirection) / Vector3.Dot(player.lookDirection, player.lookDirection) * player.lookDirection;
		velocity += (projectedPosition - position) * 8.0f * Time.deltaTime;

		position += velocity * Time.deltaTime;

		body.setTransform(position, Quaternion.Identity);
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix());
	}
}
