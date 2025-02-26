using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class Projectile : Entity
{
	string path;
	Vector3 offset;
	Vector3 velocity;

	Vector3 particleOffset;

	protected float speed = 5.0f;
	protected bool spins = false;
	protected float gravity = 0;


	public Projectile(string path, Vector3 offset)
	{
		this.path = path;
		this.offset = offset;
	}

	public override unsafe void init()
	{
		load(path, 0, PhysicsFilter.Default | PhysicsFilter.CreatureHitbox | PhysicsFilter.Ragdoll);
		velocity = rotation.forward * speed;
		particleOffset = particles.Count > 0 ? particles[0].handle->spawnOffset : Vector3.Zero;
	}

	public override void update()
	{
		base.update();

		offset = Vector3.Lerp(offset, Vector3.Zero, 5 * Time.deltaTime);
		if (spins)
			modelTransform = Matrix.CreateRotation(Vector3.UnitZ, Time.currentTime / 1e9f * 12);

		velocity.y += gravity * Time.deltaTime;
		position += velocity * Time.deltaTime;
		rotation = Quaternion.LookAt(velocity.normalized);
		body.setTransform(position, rotation);
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (other.entity is Hittable)
		{
			Hittable hittable = other.entity as Hittable;
			hittable.hit(this, null);
		}

		remove();
	}

	public override unsafe void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();
		if (model != null)
			Renderer.DrawModel(model, Matrix.CreateTranslation(offset) * transform * modelTransform, animator, isStatic);
		for (int i = 0; i < lights.Count; i++)
			Renderer.DrawPointLight(lights[i], transform);
		for (int i = 0; i < particles.Count; i++)
		{
			particles[i].handle->spawnOffset = particleOffset + rotation.conjugated * offset;
			Renderer.DrawParticleSystem(particles[i]);
		}
	}
}
