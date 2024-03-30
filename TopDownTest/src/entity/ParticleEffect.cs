using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ParticleEffect : Entity
{
	struct Particle
	{
		public Vector3 position;
		public Vector3 velocity;
		public float rotation;
		public bool alive;
	}


	Particle[] particles;
	int numAliveParticles;
	uint color;

	Vector2 direction;

	long birthTime;

	AudioSource audio;


	public ParticleEffect(int numParticles, uint color, Sound sound, Vector2 position, Vector2 direction)
	{
		this.position = position;
		this.direction = direction;
		particles = new Particle[numParticles];
		this.color = color;

		for (int i = 0; i < numParticles; i++)
		{
			particles[i] = new Particle();
			particles[i].position = new Vector3(position, 1);
			particles[i].velocity = new Vector3((direction + MathHelper.RandomFloat(-3.0f, 3.0f) * new Vector2(-direction.y, direction.x)).normalized, MathHelper.RandomFloat(-0.2f, 0.2f));
			particles[i].rotation = Random.Shared.NextSingle() * MathF.PI * 2;
			particles[i].alive = true;
		}
		numAliveParticles = numParticles;

		if (sound != null)
		{
			audio = new AudioSource(new Vector3(position, 0.0f));
			audio.playSoundOrganic(sound, 0.5f, 1, 0.2f, 0.5f);
		}

		birthTime = Time.currentTime;
	}

	public override void destroy()
	{
		if (audio != null)
			audio.destroy();
	}

	public override void update()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			if (particles[i].alive)
			{
				particles[i].velocity.z += -10 * Time.deltaTime;
				particles[i].position += particles[i].velocity * Time.deltaTime;
				if (particles[i].position.z <= 0)
				{
					particles[i].alive = false;
					numAliveParticles--;
				}
			}
		}
		if (numAliveParticles <= 0)
			removed = true;
	}

	public override void draw()
	{
		for (int i = 0; i < particles.Length; i++)
		{
			if (particles[i].alive)
			{
				Renderer.DrawSprite(particles[i].position.x - 0.0625f, particles[i].position.y - 0.0625f, particles[i].position.z, 0.125f, 0.125f, particles[i].rotation, null, false, color);
			}
		}

		float brightness = MathF.Exp(-(Time.currentTime - birthTime) / 1e9f * 6) * 3;
		Renderer.DrawLight(position, new Vector3(1.0f, 1.0f, 1.7f) * brightness, 3.0f);
	}
}
