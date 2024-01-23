using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class MagicExplosionEffect : Entity
{
	ParticleSystem particles;
	Sound sfxHit;

	Vector3 direction;

	long birthTime;


	public MagicExplosionEffect(Vector3 direction)
	{
		this.direction = direction;

		particles = new ParticleSystem(20);
		particles.emissionRate = 0.0f;

		particles.additive = true;
		particles.particleSize = 0.05f;
		particles.spriteTint = new Vector4(0.3f, 0.35f, 1.0f, 1.0f);

		particles.randomRotation = true;
		particles.randomRotationSpeed = true;
		particles.rotationSpeed = 1.0f;
		particles.randomVelocity = true;
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

		sfxHit = Resource.GetSound("res/item/spell/spell_hit.ogg");

		birthTime = Time.currentTime;
	}

	public override void init()
	{
		particles.transform = getModelMatrix();
		particles.emitParticle(direction, 20);

		Audio.PlayOrganic(sfxHit, position);
	}

	public override void destroy()
	{
	}

	public override void update()
	{
		particles.update();
		if (particles.numParticles == 0)
			remove();
	}

	public override void draw(GraphicsDevice graphics)
	{
		particles.draw(graphics);

		float lightIntensity = MathF.Exp(-(Time.currentTime - birthTime) / 1e9f * 8) * 50;
		Renderer.DrawLight(position, new Vector3(0.3f, 0.35f, 1.0f) * lightIntensity);
	}
}
