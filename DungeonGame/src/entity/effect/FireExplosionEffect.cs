using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class FireExplosionEffect : Entity
{
	const float LIFETIME = 1.5f;


	Sound sfxHit;

	Vector3 direction;

	long birthTime;


	public FireExplosionEffect(Vector3 direction)
	{
		this.direction = direction;

		particles = new ParticleSystem(200);
		particles.emissionRate = 120.0f;

		particles.additive = true;
		particles.size = 0.05f;
		particles.color = new Vector4(1.0f, 0.35f, 0.3f, 1.0f);

		particles.randomRotation = 1.0f;
		particles.randomRotationSpeed = 1;
		particles.rotationSpeed = 1.0f;
		particles.spawnShape = ParticleSpawnShape.Circle;
		particles.spawnRadius = 0.2f;
		particles.randomVelocity = 0.05f;
		particles.gravity = 1.0f;
		particles.lifetime = 1.0f;
		particles.randomLifetime = 0.1f;
		//particles.randomVelocityMultiplier = 3.0f;

		//particles.particleSizeAnim = new Gradient<float>(1.0f);
		//particles.particleSizeAnim.setValue(1.0f, 0.0f);

		//particles.textureAtlas = Resource.GetTexture("res/texture/particle/cloth.png");
		//particles.atlasColumns = 1;
		//particles.frameWidth = 64;
		//particles.frameHeight = 64;
		//particles.numFrames = 1;
		//particles.emissionRate = 0.0f;

		sfxHit = Resource.GetSound("res/item/sfx/ignite.ogg");

		birthTime = Time.currentTime;
	}

	public override void init()
	{
		particles.setTransform(getModelMatrix());
		//particles.emitParticle(direction, 100);

		Audio.PlayOrganic(sfxHit, position, 1.0f, 0.5f);
	}

	public override void destroy()
	{
	}

	public override void update()
	{
		float elapsed = (Time.currentTime - birthTime) / 1e9f;
		particles.emissionRate = MathHelper.Lerp(200, 0, elapsed / LIFETIME);
		particles.update(getModelMatrix());
		if (elapsed > LIFETIME && particles.numParticles == 0)
			remove();
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		float lightIntensity = MathF.Exp(-(Time.currentTime - birthTime) / 1e9f * 8) * 50;
		Renderer.DrawLight(position, new Vector3(1.0f, 0.35f, 0.3f) * lightIntensity);
	}
}
