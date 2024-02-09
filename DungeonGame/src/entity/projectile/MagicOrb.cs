using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class MagicOrb : Projectile
{
	const float SPEED = 1.2f;


	List<Entity> hitEntities = new List<Entity>();


	public unsafe MagicOrb(Vector3 direction, Entity caster, int damage)
		: base(direction * SPEED, Vector3.Zero, caster)
	{
		model = Resource.GetModel("res/entity/projectile/magic_orb/magic_orb.gltf");
		model.scene->materials[0].emissiveStrength = 200;

		this.damage = damage;
		piercing = true;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Kinematic, (uint)PhysicsFilterGroup.Weapon, (uint)PhysicsFilterMask.Weapon);
		body.addSphereTrigger(0.25f, Vector3.Zero);
	}

	protected override void onProjectileHit(Entity entity)
	{
		DungeonGame.instance.level.addEntity(new MagicExplosionEffect(-velocity.normalized), position, rotation);
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix() * Matrix.CreateScale(2.5f));
		Renderer.DrawLight(position, new Vector3(0.229f, 0.26f, 1.0f) * 20);
	}
}
