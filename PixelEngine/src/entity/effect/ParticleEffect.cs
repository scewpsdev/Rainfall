using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


struct Particle
{
	public long birthTime;
	public Vector2 position;
}

public class ParticleEffect : Entity
{
	Entity follow;
	Vector2 offset;

	int numParticles;
	float duration;
	float lifetime;
	float spawnRadius;
	uint color;

	long startTime;
	int particlesEmitted = 0;

	List<Particle> particles = new List<Particle>();


	public ParticleEffect(Entity follow, int numParticles, float duration, float lifetime, float spawnRadius, uint color)
	{
		this.follow = follow;
		this.numParticles = numParticles;
		this.duration = duration;
		this.lifetime = lifetime;
		this.spawnRadius = spawnRadius;
		this.color = color;
	}

	public override void init()
	{
		startTime = Time.currentTime;
		if (follow != null)
			offset = position - follow.position;
	}

	public override void update()
	{
		position = follow.position + offset;

		float elapsed = (Time.currentTime - startTime) / 1e9f;
		int shouldBeEmitted = (int)(elapsed / duration * numParticles);
		if (shouldBeEmitted > particlesEmitted)
		{
			int amount = shouldBeEmitted - particlesEmitted;

			for (int i = 0; i < amount; i++)
			{
				Particle particle = new Particle();
				particle.birthTime = Time.currentTime;
				particle.position = position + MathHelper.RandomPointOnCircle(Random.Shared) * spawnRadius;
				particles.Add(particle);
			}

			particlesEmitted += amount;
		}

		for (int i = 0; i < particles.Count; i++)
		{
			Particle particle = particles[i];
			particle.position.y += 0.5f * Time.deltaTime;
			float life = (Time.currentTime - particles[i].birthTime) / 1e9f;
			if (life >= lifetime)
				particles.RemoveAt(i--);
			else
				particles[i] = particle;
		}

		if (elapsed >= duration && particlesEmitted == numParticles && particles.Count == 0)
			remove();
	}

	public override void render()
	{
		for (int i = 0; i < particles.Count; i++)
		{
			Particle particle = particles[i];
			Renderer.DrawSprite(particle.position.x, particle.position.y, 1.0f / 16, 1.0f / 16, null, false, color);
		}
	}
}
