using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class TestFire : Entity
{
	public TestFire()
	{
		particles = new ParticleSystem(250);
		particles.textureAtlas = Resource.GetTexture("res/texture/particle/torch_flame.png");
		particles.atlasSize = new Vector2i(4);
		particles.numFrames = 12;
		particles.linearFiltering = true;
		particles.emissionRate = 200.0f;
		particles.lifetime = 1.0f;
		particles.spawnOffset = new Vector3(0.0f, 0.3f, 0.2f);
		particles.spawnRadius = 0.1f;
		particles.spawnShape = ParticleSpawnShape.Sphere;
		particles.size = 0.2f;
		particles.startVelocity = new Vector3(0.0f, 0.0f, 0.0f);
		particles.gravity = 1.0f;
		particles.additive = true;
		particles.color = new Vector4(1.0f);
	}

	public override void update()
	{
		base.update();
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		uint hash = Hash.hash(position);
		Vector3 offset = new Vector3(MathF.Sin(Time.currentTime / 1e9f + hash / 10000) * 0.5f, MathF.Sin(Time.currentTime / 1e9f * 0.1f + hash / 10000), MathF.Cos(Time.currentTime / 1e9f + hash / 10000) * 0.5f);
		particleOffset = offset;

		Renderer.DrawLight(position + offset, new Vector3(0.965f, 0.604f, 0.329f) * 2.0f);
	}
}
