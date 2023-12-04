using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class MagicBullet : Entity
{
	Sprite sprite;

	Vector3 direction;


	public MagicBullet(Vector3 direction)
	{
		this.direction = direction;

		Tileset tileset = new Tileset(Resource.GetTexture("res/entity/projectile/magic_bullet.png"), 16, 16);
		sprite = new Sprite(tileset, 0, 0);
	}

	public override void update()
	{
		position += direction * 10 * Time.deltaTime;
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawProjectile(position, new Vector2(1.0f), -direction.xz.angle, sprite, new Vector4(1.0f, 1.0f, 1.0f, 10));
	}
}
