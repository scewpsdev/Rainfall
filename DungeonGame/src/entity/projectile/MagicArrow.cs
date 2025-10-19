using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagicArrow : Projectile
{
	const float SPEED = 15.0f;


	public unsafe MagicArrow(Vector3 direction, Vector3 offset, Entity caster, int damage)
		: base(direction * SPEED, offset, caster)
	{
		model = Resource.GetModel("res/entity/projectile/magic_orb/magic_orb.gltf");
		model.scene->materials[0].emissiveStrength = 200;

		this.damage = damage;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Kinematic, (uint)PhysicsFilterGroup.Weapon, (uint)PhysicsFilterMask.Weapon);
		body.addSphereTrigger(0.1f, Vector3.Zero);
	}

	protected override void onProjectileHit(Entity entity)
	{
		DungeonGame.instance.level.addEntity(new MagicExplosionEffect(-velocity.normalized), position, rotation);
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);
		Renderer.DrawLight(position, new Vector3(0.229f, 0.26f, 1.0f) * 8);
	}
}
