using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GlowingFlower : Entity
{
	Sprite sprite;

	ParticleEffect particles;


	public GlowingFlower()
	{
		sprite = new Sprite(tileset, 10, 2, 1, 2);
	}

	public override void init(Level level)
	{
		level.addEntity(particles = new ParticleEffect(this, "effects/glowing_flower.rfs"), position + Vector2.Up * 1.25f);
		particles.collision = true;
	}

	public override void destroy()
	{
		particles.remove();
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, LAYER_BG, 1, 2, 0, sprite);
		Renderer.DrawLight(position + Vector2.Up * 1.25f, new Vector3(0.7f, 1.0f, 0.9f) * 10, 9);
	}
}
