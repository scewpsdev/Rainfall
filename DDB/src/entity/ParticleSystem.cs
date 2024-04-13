using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


public enum ParticleSpawnShape
{
	None = 0,

	Point,
	Circle,
	Sphere,
	Line,
}

public enum ParticleFollowMode
{
	None = 0,

	Trail,
	Follow,
}

struct Particle
{
	public int id;
	public Vector3 position;
	public float rotation;
	public Vector3 velocity;
	public float rotationVelocity;
	public float size;
	public Texture texture;
	public int u0, v0, u1, v1;
	public uint color;
	public float lifetime;

	public long birthTime;
}

internal class ParticleSystem
{
	public Matrix transform = Matrix.Identity;

	public ParticleSpawnShape spawnShape = ParticleSpawnShape.Point;
	public ParticleFollowMode followMode = ParticleFollowMode.Trail;
	public Vector3 spawnOffset = Vector3.Zero;
	public float emissionRate = 0.0f;
	public float lifetime = 1.0f;
	public float particleSize = 0.1f;
	public float gravity = -10.0f;
	public Vector3 initialVelocity = Vector3.Zero;
	public Vector3 constantVelocity = Vector3.Zero;
	public float rotationSpeed = 0.0f;

	public Texture textureAtlas = null;
	public int atlasColumns = 1;
	public int frameWidth = 0, frameHeight = 0;
	public int numFrames = 1;

	public uint spriteTint = 0xffffffff;
	public bool additive = false;

	public float spawnRadius = 1.0f;
	public Vector3 point1 = new Vector3(0.0f, 0.0f, 0.0f);
	public Vector3 point2 = new Vector3(1.0f, 0.0f, 0.0f);

	public bool randomVelocity = false;
	public bool randomRotation = false;
	public bool randomRotationSpeed = false;

	public Gradient<float> particleSizeAnim = null;

	Particle[] particles = null;
	public int numParticles { get; private set; }
	int maxParticles = 0;

	long lastEmitted;

	Random random;


	public ParticleSystem(int maxParticles)
	{
		this.maxParticles = maxParticles;
		particles = new Particle[maxParticles];
		Array.Fill(particles, new Particle { id = -1 });

		random = new Random();

		lastEmitted = Time.currentTime;
	}

	int getNewParticle()
	{
		for (int i = 0; i < maxParticles; i++)
		{
			if (particles[i].id == -1)
			{
				if (i >= numParticles)
					numParticles = i + 1;
				particles[i].id = i;
				return i;
			}
		}
		return -1;
	}

	public void emitParticle(Vector3 particleVelocity, int num = 1)
	{
		for (int i = 0; i < num; i++)
		{
			Vector3 position = Vector3.Zero;
			float rotation = randomRotation ? Random.Shared.NextSingle() * MathF.PI * 2.0f : 0.0f;

			switch (spawnShape)
			{
				case ParticleSpawnShape.Point:
					position = Vector3.Zero;
					break;
				case ParticleSpawnShape.Circle:
					float r = spawnRadius * MathF.Sqrt((float)random.NextDouble());
					float theta = (float)random.NextDouble() * 2.0f * MathF.PI;
					position = new Vector3(r * MathF.Cos(theta), 0.0f, r * MathF.Sin(theta));
					break;
				case ParticleSpawnShape.Sphere:
					float d = 2.0f;
					float x = 0.0f, y = 0.0f, z = 0.0f;
					for (int j = 0; j < 8 && d > 1.0f; j++)
					{
						x = (float)random.NextDouble() * 2.0f - 1.0f;
						y = (float)random.NextDouble() * 2.0f - 1.0f;
						z = (float)random.NextDouble() * 2.0f - 1.0f;
						d = x * x + y * y + z * z;
					}
					position = new Vector3(x, y, z) * spawnRadius;
					break;
				case ParticleSpawnShape.Line:
					float t = (float)random.NextDouble();
					position = Vector3.Lerp(point1, point2, t);
					break;
				default:
					Debug.Assert(false);
					break;
			}

			switch (followMode)
			{
				case ParticleFollowMode.Trail:
					position = (transform * new Vector4(position + spawnOffset, 1.0f)).xyz;
					break;
				case ParticleFollowMode.Follow:
					position += spawnOffset;
					break;
				default:
					Debug.Assert(false);
					break;
			}

			Vector3 velocity = (randomVelocity ? MathHelper.RandomVector3(-1.0f, 1.0f).normalized * initialVelocity.length : initialVelocity) + particleVelocity;
			float rotationVelocity = rotationSpeed * (randomRotationSpeed ? (Random.Shared.NextSingle() * 2.0f - 1.0f) : 1.0f);

			int particleID = getNewParticle();
			if (particleID != -1)
			{
				ref Particle particle = ref particles[particleID];
				particle.position = position;
				particle.rotation = rotation;
				particle.velocity = velocity;
				particle.rotationVelocity = rotationVelocity;
				particle.size = particleSize;
				particle.texture = textureAtlas;
				particle.u0 = 0;
				particle.v0 = 0;
				particle.u1 = frameWidth != 0 ? frameWidth : textureAtlas != null ? textureAtlas.info.width : 0;
				particle.v1 = frameHeight != 0 ? frameHeight : textureAtlas != null ? textureAtlas.info.height : 0;
				particle.color = spriteTint;
				particle.lifetime = lifetime;
				particle.birthTime = Time.currentTime;
			}
		}
	}

	public void update()
	{
		long now = Time.currentTime;
		if (emissionRate > 0.0f)
		{
			if (now - lastEmitted > 1e9 / emissionRate)
			{
				emitParticle(Vector3.Zero, 1);
				lastEmitted = now;
			}
		}

		for (int i = numParticles - 1; i >= 0; i--)
		{
			ref Particle particle = ref particles[i];
			if (particle.id != -1)
			{
				float particleTimer = (now - particle.birthTime) / 1e9f;

				particle.position += (particle.velocity + constantVelocity) * Time.deltaTime;
				particle.velocity.y += gravity * Time.deltaTime;

				particle.rotation += particle.rotationVelocity * Time.deltaTime;

				if (particleSizeAnim != null)
					particle.size = particleSizeAnim.getValue(particleTimer / particle.lifetime);

				if (textureAtlas != null && numFrames > 0 && atlasColumns > 0)
				{
					int frameIdx = (int)(particleTimer / particle.lifetime * numFrames);
					int x = frameIdx % atlasColumns;
					int y = frameIdx / atlasColumns;
					int width = frameWidth != 0 ? frameWidth : textureAtlas != null ? textureAtlas.info.width : 0;
					int height = frameHeight != 0 ? frameHeight : textureAtlas != null ? textureAtlas.info.height : 0;
					particle.u0 = x * width;
					particle.v0 = y * height;
					particle.u1 = x * width + width;
					particle.v1 = y * height + height;
				}

				if ((now - particle.birthTime) / 1e9f >= particle.lifetime)
				{
					if (particle.id == numParticles - 1)
						numParticles--;
					particle.id = -1;
					i--;
				}
			}
			else
			{
				if (i == numParticles - 1)
					numParticles--;
			}
		}
	}

	public void draw(GraphicsDevice graphics)
	{
		for (int i = 0; i < numParticles; i++)
		{
			Particle particle = particles[i];
			if (particle.id != -1)
			{
				switch (followMode)
				{
					case ParticleFollowMode.Trail:
						Renderer.DrawParticle(particle.position, particle.texture, particle.u0, particle.v0, particle.u1, particle.v1, particle.size, particle.rotation, particle.color, additive);
						break;
					case ParticleFollowMode.Follow:
						Vector3 position = (transform * new Vector4(particle.position, 1.0f)).xyz;
						Renderer.DrawParticle(position, particle.texture, particle.u0, particle.v0, particle.u1, particle.v1, particle.size, particle.rotation, particle.color, additive);
						break;
					default:
						Debug.Assert(false);
						break;
				}
			}
		}
	}
}
