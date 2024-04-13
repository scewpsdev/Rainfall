using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class MagicOrb : Entity
{
	const float SPEED = 4.0f;
	const int NUM_PIERCES = 3;


	Entity caster;
	int damage;

	Model model;
	RigidBody body;

	Vector3 velocity;

	int remainingPierces = NUM_PIERCES;

	List<Entity> hitEntities = new List<Entity>();


	public MagicOrb(Vector3 direction, Entity caster, int damage)
	{
		this.caster = caster;
		this.damage = damage;

		model = Resource.GetModel("res/entity/projectile/magic_orb/magic_orb.gltf");

		velocity = direction * SPEED;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Kinematic);
		body.addSphereTrigger(0.1f, Vector3.Zero);
	}

	public override void destroy()
	{
		body.destroy();
	}

	public override void onContact(RigidBody body, CharacterController controller, ContactType contactType, bool trigger)
	{
		if (remainingPierces == 0)
			return;

		if (body.entity is not Player && !hitEntities.Contains(body.entity))
		{
			if (body.entity is Creature)
			{
				Creature creature = body.entity as Creature;
				creature.hit(damage, caster);

				remainingPierces--;
			}
			else
			{
				remainingPierces = 0;
			}

			hitEntities.Add((Entity)body.entity);

			if (remainingPierces == 0)
				remove();
		}
	}

	public override void update()
	{
		position += velocity * Time.deltaTime;

		body.setTransform(position, Quaternion.Identity);
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix());
	}
}
