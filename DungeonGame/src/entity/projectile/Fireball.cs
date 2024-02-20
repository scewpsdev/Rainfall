using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Fireball : Projectile
{
	const float SPEED = 10;


	public Fireball(Vector3 direction, Vector3 offset, Player player, int damage)
		: base(direction * SPEED, offset, player)
	{
		gravity = -4;
		this.damage = damage;

		particles = Particles.CreateFire(256);
		particles.lifetime = 0.3f;
		particles.follow = false;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Kinematic, (uint)PhysicsFilterGroup.Weapon, (uint)PhysicsFilterMask.Weapon);
		body.addSphereTrigger(0.1f, Vector3.Zero);
	}

	protected override void onProjectileHit(Entity entity)
	{
		//DungeonGame.instance.level.addEntity(new FireExplosionEffect(-velocity.normalized), position, Quaternion.Identity);
		DungeonGame.instance.level.addEntity(new Explosion(shooter), position, Quaternion.Identity);
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);
		Renderer.DrawLight(position, new Vector3(1.0f, 0.26f, 0.229f) * 8);
	}
}
