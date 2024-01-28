using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class HomingOrb : Projectile
{
	const float SPEED = 7.0f;


	Player player;


	public HomingOrb(Vector3 offset, Player player, int damage)
		: base(player.lookDirection * SPEED, offset, player)
	{
		this.player = player;

		model = Resource.GetModel("res/entity/projectile/magic_orb_small/magic_orb_small.gltf");
		unsafe
		{
			model.sceneDataHandle->materials[0].emissiveStrength = 200;
		}
		this.damage = damage;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Kinematic, (uint)PhysicsFilterGroup.Weapon, (uint)PhysicsFilterMask.Weapon);
		body.addSphereTrigger(0.05f, Vector3.Zero);
	}

	protected override void onProjectileHit(Entity entity)
	{
		DungeonGame.instance.level.addEntity(new MagicExplosionEffect(-velocity.normalized), position, rotation);
	}

	public override void update()
	{
		Vector3 newVelocity = player.lookDirection * SPEED;

		velocity = Vector3.Lerp(velocity, newVelocity, 1.0f * Time.deltaTime);

		Vector3 projectedPosition = player.lookOrigin + Vector3.Dot(position - player.lookOrigin, player.lookDirection) / Vector3.Dot(player.lookDirection, player.lookDirection) * player.lookDirection;
		velocity += (projectedPosition - position).normalized * 8.0f * Time.deltaTime;

		base.update();
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);
		Renderer.DrawLight(position, new Vector3(0.229f, 0.26f, 1.0f) * 5);
	}
}
