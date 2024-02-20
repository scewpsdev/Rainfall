using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


internal class Explosion : Entity
{
	const float RANGE = 2.5f;
	const int DAMAGE = 70;
	const float EXPLOSION_FORCE = 15.0f;


	Entity shooter;

	ParticleSystem fireParticles;
	ParticleSystem dustParticles;

	Sound sfxHit;

	List<Entity> hitEntities = new List<Entity>();

	float timer = 0.0f;

	float lightIntensity = 0.0f;


	public Explosion(Entity shooter)
	{
		this.shooter = shooter;

		sfxHit = Resource.GetSound("res/sfx/ignite.ogg");
	}

	public override void init()
	{
		fireParticles = new ParticleSystem(128);
		fireParticles.transform = getModelMatrix();
		fireParticles.emissionRate = 128.0f;
		fireParticles.lifetime = 0.6f;
		fireParticles.initialVelocity = new Vector3(0.0f, 1.5f, 0.0f);
		fireParticles.gravity = 0.0f;
		fireParticles.spawnShape = ParticleSpawnShape.Sphere;
		fireParticles.spawnRadius = 0.5f;
		fireParticles.particleSize = 1.0f;
		//fireParticles.textureAtlas = Resource.GetTexture("res/texture/particle/explosion.png");
		//fireParticles.atlasColumns = 1;
		//fireParticles.frameWidth = 256;
		//fireParticles.frameHeight = 0;
		//fireParticles.numFrames = 1;
		fireParticles.additive = true;
		fireParticles.randomRotation = true;
		fireParticles.randomRotationSpeed = true;
		fireParticles.rotationSpeed = 1.0f;
		fireParticles.randomVelocity = true;

		fireParticles.particleSizeAnim = new Gradient<float>(0.4f, 0.01f);
		fireParticles.spriteTint = new Vector4(MathHelper.SRGBToLinear(0.965f, 0.604f * 0.8f, 0.329f * 0.8f), 1.0f);

		//fireParticles.emitParticle(Vector3.Zero, 64);


		dustParticles = new ParticleSystem(32);
		dustParticles.transform = getModelMatrix();
		dustParticles.emissionRate = 0.0f;
		dustParticles.lifetime = 1.5f;
		dustParticles.particleSize = 0.05f;
		dustParticles.rotationSpeed = 3.0f;
		dustParticles.randomRotation = true;
		dustParticles.randomRotationSpeed = true;
		dustParticles.spriteTint = new Vector4(0.01f, 0.01f, 0.01f, 1.0f);

		for (int i = 0; i < 12; i++)
		{
			dustParticles.emitParticle(Vector3.Zero, new Vector3(MathHelper.RandomFloat(-1.0f, 1.0f), MathHelper.RandomFloat(-1.0f, 1.0f), MathHelper.RandomFloat(-1.0f, 1.0f)).normalized * 2.5f);
		}



		Span<HitData> hits = stackalloc HitData[64];
		int numHits = Physics.OverlapSphere(RANGE, position, hits, QueryFilterFlags.Default);
		for (int i = 0; i < numHits; i++)
		{
			RigidBody body = hits[i].body;
			if (body != null && !hitEntities.Contains(body.entity))
			{
				if (hits[i].body.entity is Hittable)
				{
					Hittable hittable = hits[i].body.entity as Hittable;
					Entity entity = hits[i].body.entity as Entity;

					Vector3 direction = (entity.position + new Vector3(0.0f, 1.0f, 0.0f) - position).normalized;
					Vector3 force = direction * EXPLOSION_FORCE;

					hittable.hit(DAMAGE, shooter, hits[i].position, force, 0);

					if (entity is Creature)
					{
						Creature creature = entity as Creature;
						if (!creature.isAlive)
							creature.ragdoll.hitboxes[0].addForce(force);
					}
				}
				/*
				else if (hits[i].body.entity is RagdollEntity)
				{
					Vector3 direction = (body.entity.getPosition() - position).normalized;
					Vector3 force = direction * EXPLOSION_FORCE;

					Ragdoll ragdoll = hits[i].body.ragdoll;
					ragdoll.hitboxes[0].addForce(force);
				}
				*/

				hitEntities.Add((Entity)hits[i].body.entity);
			}
		}

		Audio.PlayOrganic(sfxHit, position, 1.0f, 0.5f);
		AIManager.NotifySound(position, 5.0f);
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

		lightIntensity = timer < 0.5f ? 1.0f : MathF.Exp(-(timer - 0.5f) * 6.0f);

		if (lightIntensity < 0.001f)
		{
			remove();
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		fireParticles.draw(graphics);
		dustParticles.draw(graphics);

		Renderer.DrawLight(position, new Vector3(10.0f, 4.0f, 1.0f) * lightIntensity);
	}
}
