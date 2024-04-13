using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class SparkEffect : Entity
{
	ParticleSystem particles;

	Vector3 direction;


	public SparkEffect(Vector3 direction)
	{
		this.direction = direction;

		particles = new ParticleSystem(3);
		particles.additive = true;
		particles.emissionRate = 0.0f;
		particles.randomVelocity = true;

		//particles.textureAtlas = Resource.GetTexture("res/texture/particle/cloth.png");
		//particles.atlasColumns = 1;
		//particles.frameWidth = 64;
		//particles.frameHeight = 64;
		//particles.numFrames = 1;
		//particles.emissionRate = 0.0f;
	}

	public override void init()
	{
		particles.transform = getModelMatrix();
		particles.emitParticle(direction, 3);
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
	}
}
