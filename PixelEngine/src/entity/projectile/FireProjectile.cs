using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class FireProjectile : Projectile
{
	const float speed = 2;

	public FireProjectile(Vector2 direction, Vector2 startVelocity, Vector2 offset, Entity shooter)
		: base(direction * speed, startVelocity, offset, shooter, null, 1)
	{
		maxSpeed = 20;
		gravity = 0;
		acceleration = 30;
		maxRicochets = 0;

		sprite = new Sprite(Item.tileset, 1, 2);
		spriteColor = new Vector4(2.0f);
		additive = true;
	}

	public override void onHit(Vector2 normal)
	{
		GameState.instance.level.addEntity(Effects.CreateImpactEffect(normal, velocity.length, MathHelper.ARGBToVector(0xFFb88865).xyz * 2), position - velocity * Time.deltaTime);
	}
}
