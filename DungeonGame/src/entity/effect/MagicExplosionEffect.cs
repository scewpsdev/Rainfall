using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class MagicExplosionEffect : Entity
{
	Sound sfxHit;

	Vector3 direction;

	long birthTime;


	public MagicExplosionEffect(Vector3 direction)
	{
		this.direction = direction;

		particles = new ParticleSystem(20);
		particles.emissionRate = 0.0f;

		particles.additive = true;
		particles.size = 0.05f;
		particles.color = new Vector4(0.3f, 0.35f, 1.0f, 1.0f);

		particles.randomRotation = 1.0f;
		particles.randomRotationSpeed = 1;
		particles.rotationSpeed = 1.0f;
		particles.randomVelocity = 1.0f;
		particles.gravity = -4.0f;
		particles.lifetime = 3.0f;
		//particles.randomVelocityMultiplier = 3.0f;

		//particles.particleSizeAnim = new Gradient<float>(1.0f);
		//particles.particleSizeAnim.setValue(1.0f, 0.0f);

		//particles.textureAtlas = Resource.GetTexture("res/texture/particle/cloth.png");
		//particles.atlasColumns = 1;
		//particles.frameWidth = 64;
		//particles.frameHeight = 64;
		//particles.numFrames = 1;
		//particles.emissionRate = 0.0f;

		sfxHit = Resource.GetSound("res/sfx/splash.ogg");

		birthTime = Time.currentTime;
	}

	public override void init()
	{
		particles.setTransform(getModelMatrix());
		particles.emitParticle(direction, 20);

		Audio.PlayOrganic(sfxHit, position);
	}

	public override void destroy()
	{
	}

	public override void update()
	{
		particles.update(getModelMatrix());
		if (particles.numParticles == 0)
			remove();
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		float lightIntensity = MathF.Exp(-(Time.currentTime - birthTime) / 1e9f * 8) * 50;
		Renderer.DrawLight(position, new Vector3(0.3f, 0.35f, 1.0f) * lightIntensity);
	}
}
