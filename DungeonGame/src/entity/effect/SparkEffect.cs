using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class SparkEffect : Entity
{
	Vector3 direction;


	public SparkEffect(Vector3 direction)
	{
		this.direction = direction;

		particles = new ParticleSystem(3);
		particles.emissionRate = 0.0f;

		particles.additive = true;
		//particles.textureAtlas = Resource.GetTexture("res/texture/particle/spark.png");
		particles.size = 0.025f;
		particles.color = new Vector4(new Vector3(1.0f, 0.2f, 0.1f) * 5, 1.0f);

		particles.randomRotation = 1.0f;
		particles.randomRotationSpeed = 1;
		particles.rotationSpeed = 1.0f;
		particles.randomVelocity = 0.1f;
		//particles.randomVelocityMultiplier = 3.0f;

		//particles.particleSizeAnim = new Gradient<float>(1.0f);
		//particles.particleSizeAnim.setValue(1.0f, 0.0f);

		//particles.textureAtlas = Resource.GetTexture("res/texture/particle/cloth.png");
		//particles.atlasColumns = 1;
		//particles.frameWidth = 64;
		//particles.frameHeight = 64;
		//particles.numFrames = 1;
		//particles.emissionRate = 0.0f;
	}

	public override void init()
	{
		particles.setTransform(getModelMatrix());
		particles.emitParticle(direction * 2.0f, 3);
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
	}
}
