using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class MagicArrow : Entity
{
	const float SPEED = 24.0f;


	Entity caster;
	int damage;

	Model model;
	RigidBody body;

	Vector3 velocity;

	bool hit = false;


	public MagicArrow(Vector3 direction, Entity caster, int damage)
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

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (hit)
			return;

		if (body.entity is not Player)
		{
			if (body.entity is Creature)
			{
				Creature creature = body.entity as Creature;
				creature.hit(damage, caster);
			}

			remove();

			hit = true;
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
