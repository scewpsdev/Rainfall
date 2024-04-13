using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


internal class Explosion : Entity
{
	const float RANGE = 1.0f;
	const int DAMAGE = 100;


	Entity shooter;

	ParticleSystem fireParticles;
	ParticleSystem dustParticles;

	float timer = 0.0f;


	public Explosion(Entity shooter)
	{
		this.shooter = shooter;
	}

	public override void init()
	{
		fireParticles = new ParticleSystem(64);
		fireParticles.transform = getModelMatrix();
		fireParticles.emissionRate = 128.0f;
		fireParticles.lifetime = 0.6f;
		fireParticles.initialVelocity = new Vector3(0.0f, 1.5f, 0.0f);
		fireParticles.gravity = 0.0f;
		fireParticles.spawnShape = ParticleSpawnShape.Sphere;
		fireParticles.spawnRadius = 0.5f;
		fireParticles.particleSize = 1.0f;
		fireParticles.textureAtlas = Resource.GetTexture("res/texture/particle/explosion.png");
		//fireParticles.atlasColumns = 1;
		//fireParticles.frameWidth = 256;
		//fireParticles.frameHeight = 0;
		//fireParticles.numFrames = 1;
		fireParticles.additive = true;
		fireParticles.randomRotation = true;
		fireParticles.randomRotationSpeed = true;
		fireParticles.rotationSpeed = 1.0f;
		fireParticles.randomVelocity = true;

		fireParticles.particleSizeAnim = new Gradient<float>(1.0f);
		fireParticles.particleSizeAnim.setValue(1.0f, 0.0f);

		//fireParticles.emitParticle(Vector3.Zero, 64);


		dustParticles = new ParticleSystem(32);
		dustParticles.transform = getModelMatrix();
		dustParticles.emissionRate = 0.0f;
		dustParticles.lifetime = 0.5f;
		dustParticles.particleSize = 0.05f;
		dustParticles.rotationSpeed = 3.0f;
		dustParticles.randomRotation = true;
		dustParticles.randomRotationSpeed = true;

		fireParticles.particleSizeAnim = new Gradient<float>(1.0f);
		fireParticles.particleSizeAnim.setValue(1.0f, 0.0f);

		for (int i = 0; i < 12; i++)
		{
			dustParticles.emitParticle(new Vector3(MathHelper.RandomFloat(-1.0f, 1.0f), 1.0f, MathHelper.RandomFloat(-1.0f, 1.0f)).normalized * 5.0f);
		}


		Span<OverlapHit> hits = stackalloc OverlapHit[8];
		int numHits = Physics.OverlapSphere(RANGE, position, hits, QueryFilterFlags.Dynamic);
		for (int i = 0; i < numHits; i++)
		{
			if (hits[i].body.entity is Creature)
			{
				Creature creature = hits[i].body.entity as Creature;
				if (hits[i].body == creature.body)
				{
					creature.hit(DAMAGE, shooter);
				}
			}
		}
	}

	public override void destroy()
	{
	}

	public override void update()
	{
		timer += Time.deltaTime;

		fireParticles.update();
		dustParticles.update();

		if (timer >= 0.5f)
			fireParticles.emissionRate = 0.0f;

		if (fireParticles.numParticles == 0 && dustParticles.numParticles == 0)
			remove();
	}

	public override void draw(GraphicsDevice graphics)
	{
		fireParticles.draw(graphics);
		dustParticles.draw(graphics);
	}
}
