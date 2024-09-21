using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MagicProjectile : Projectile
{
	const float speed = 2;

	public MagicProjectile(Vector2 direction, Vector2 startVelocity, Vector2 offset, Entity shooter, Item item)
		: base(direction * speed, startVelocity, offset, shooter, item)
	{
		maxSpeed = 40;
		acceleration = 50;
		maxRicochets = 0;
		damage = 1;

		sprite = new Sprite(Item.tileset, 9, 1);
		spriteColor = new Vector4(1.5f);
		additive = true;
	}

	public override void onHit(Vector2 normal)
	{
		GameState.instance.level.addEntity(Effects.CreateImpactEffect(normal, velocity.length, MathHelper.ARGBToVector(0xFF99eeee).xyz * 2), position - velocity * Time.deltaTime);
	}

	public override void render()
	{
		base.render();
		Renderer.DrawLight(position, MathHelper.ARGBToVector(0xFF99eeee).xyz * 3, 4);
	}
}
