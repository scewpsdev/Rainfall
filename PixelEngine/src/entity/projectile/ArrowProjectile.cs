using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ArrowProjectile : Projectile
{
	public ArrowProjectile(Vector2 direction, Vector2 offset, Entity shooter, Item bow)
		: base(direction * bow.attackRange, Vector2.Zero, offset, shooter, null)
	{
		maxSpeed = bow.attackRange;
		gravity = -20;
		acceleration = 0;
		maxRicochets = 0;
		damage = bow.attackDamage;

		sprite = new Sprite(Item.tileset, 2, 0);
	}

	public override void onHit(Vector2 normal)
	{
		//GameState.instance.level.addEntity(Effects.CreateImpactEffect(normal, velocity.length, MathHelper.ARGBToVector(0xFFb88865).xyz * 2), position - velocity * Time.deltaTime);
	}
}
