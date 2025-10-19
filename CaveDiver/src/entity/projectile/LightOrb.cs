using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


public class LightOrb : Projectile
{
	const float speed = 5;

	public LightOrb(Vector2 direction, Vector2 startVelocity, Vector2 offset, Entity shooter)
		: base(direction * speed, startVelocity, offset, shooter, null, 0)
	{
		acceleration = -1;
		rotationSpeed = 1.0f;
		maxRicochets = 1000;

		sprite = new Sprite(Item.tileset, 6, 4);
		spriteColor = new Vector4(1.2f);
		additive = true;
	}

	public override void onHit(Vector2 normal)
	{
		velocity *= 0.7f;
	}

	public override void render()
	{
		base.render();
		Renderer.DrawLight(position, MathHelper.ARGBToVector(0xFFffecb5).xyz * 4, 10);
	}
}
