using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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

public struct Particle
{
	public bool active;
	public Vector3 position;
	public float rotation;
	public Vector3 velocity;
	public float rotationVelocity;
	public float size;
	//public int u0, v0, u1, v1;
	public float animationFrame;
	public Vector4 color;

	public long birthTime;
}

public class ParticleSystem
{
	class ParticleComparator : IComparer<int>
	{
		public Particle[] particles;
		public Vector3 cameraAxis;

		public int Compare(int x, int y)
		{
			ref Particle particle1 = ref particles[x];
			ref Particle particle2 = ref particles[y];

			float d1 = Vector3.Dot(particle1.position, cameraAxis);
			float d2 = Vector3.Dot(particle2.position, cameraAxis);

			return d1 < d2 ? 1 : d1 > d2 ? -1 : 0;
		}
	}


	public Matrix transform = Matrix.Identity;

	public float emissionRate = 0.0f;
	public float lifetime = 1.0f;
	public float particleSize = 0.1f;
	public Vector3 spawnOffset = Vector3.Zero;
	public ParticleSpawnShape spawnShape = ParticleSpawnShape.Point;
	public ParticleFollowMode followMode = ParticleFollowMode.Trail;
	public float gravity = -10.0f;
	public Vector3 initialVelocity = Vector3.Zero;
	public Vector3 constantVelocity = Vector3.Zero;
	public float rotationSpeed = 0.0f;

	public Texture textureAtlas = null;
	//public int atlasColumns = 1;
	public int frameWidth = 0, frameHeight = 0;
	public int numFrames = 1;
	public bool linearFiltering = false;

	public Vector4 spriteTint = Vector4.One;
	public bool additive = false;

	public float spawnRadius = 1.0f;
	public Vector3 point1 = new Vector3(0.0f, 0.0f, 0.0f);
	public Vector3 point2 = new Vector3(1.0f, 0.0f, 0.0f);

	public bool randomVelocity = false;
	public float randomVelocityMultiplier = 1.0f;
	public bool randomRotation = false;
	public bool randomRotationSpeed = false;

	public Gradient<float> particleSizeAnim = null;

	Particle[] particles = null;
	List<int> particleIndices;
	//public int numParticles { get; private set; }
	int maxParticles = 0;

	long lastEmitted;

	Random random;

	ParticleComparator particleComparator;


	public ParticleSystem(int maxParticles)
	{
		this.maxParticles = maxParticles;
		particles = new Particle[maxParticles];
		particleIndices = new List<int>(maxParticles);
		//Array.Fill(particles, new Particle { active= -1 });

		random = new Random();

		particleComparator = new ParticleComparator();

		lastEmitted = Time.currentTime;
	}

	public void copyData(ParticleSystem from)
	{
		emissionRate = from.emissionRate;
		lifetime = from.lifetime;
		particleSize = from.particleSize;
		spawnOffset = from.spawnOffset;
		spawnShape = from.spawnShape;
		followMode = from.followMode;
		gravity = from.gravity;
		initialVelocity = from.initialVelocity;
		constantVelocity = from.constantVelocity;
		rotationSpeed = from.rotationSpeed;
		textureAtlas = from.textureAtlas;
		frameWidth = from.frameWidth;
		frameHeight = from.frameHeight;
		numFrames = from.numFrames;
		linearFiltering = from.linearFiltering;
		spriteTint = from.spriteTint;
		additive = from.additive;
		spawnRadius = from.spawnRadius;
		point1 = from.point1;
		point2 = from.point2;
		randomVelocity = from.randomVelocity;
		randomVelocityMultiplier = from.randomVelocityMultiplier;
		randomRotation = from.randomRotation;
		randomRotationSpeed = from.randomRotationSpeed;
		particleSizeAnim = new Gradient<float>(from.particleSizeAnim);
	}

	int getNewParticle()
	{
		if (particleIndices.Count < maxParticles)
		{
			for (int i = 0; i < particles.Length; i++)
			{
				if (!particles[i].active)
				{
					particles[i].active = true;
					return i;
				}
			}
			Debug.Assert(false);
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
					//position += spawnOffset;
					break;
				default:
					Debug.Assert(false);
					break;
			}

			Vector3 velocity = initialVelocity + particleVelocity;
			if (randomVelocity)
				velocity += MathHelper.RandomVector3(-1.0f, 1.0f) * randomVelocityMultiplier;
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
				particle.animationFrame = 0;
				particle.color = spriteTint;
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

		Vector3 cameraAxis = Vector3.Forward;
		if (Renderer.camera != null)
			cameraAxis = Renderer.camera.rotation.forward;

		particleComparator.particles = particles;
		particleComparator.cameraAxis = cameraAxis;

		particleIndices.Clear();
		for (int i = 0; i < particles.Length; i++)
		{
			ref Particle particle = ref particles[i];
			if (particle.active)
			{
				float particleTimer = (now - particle.birthTime) / 1e9f;

				particle.velocity.y += 0.5f * gravity * Time.deltaTime;
				particle.position += (particle.velocity + constantVelocity) * Time.deltaTime;
				particle.velocity.y += 0.5f * gravity * Time.deltaTime;

				particle.rotation += particle.rotationVelocity * Time.deltaTime;

				if (particleSizeAnim != null)
					particle.size = particleSizeAnim.getValue(particleTimer / lifetime);

				if (textureAtlas != null && numFrames > 0)
				{
					int atlasColumns = 1, atlasRows = 1;
					if (frameWidth != 0 && frameHeight != 0)
					{
						atlasColumns = textureAtlas.width / frameWidth;
						atlasRows = textureAtlas.height / frameHeight;
					}
					float animationFrame = particleTimer / lifetime * numFrames / (atlasColumns * atlasRows);
					particle.animationFrame = animationFrame;
				}

				if ((now - particle.birthTime) / 1e9f >= lifetime)
				{
					//if (particle.id == numParticles - 1)
					//	numParticles--;
					particle.active = false;
					//i--;
				}
				else
				{
					int index = particleIndices.BinarySearch(i, particleComparator);
					if (index < 0)
						index = ~index;
					particleIndices.Insert(index, i);

					/*
					float d1 = Vector3.Dot(particle.position, cameraAxis);
					for (int j = 0; j < particleIndices.Count + 1; j++)
					{
						if (j == particleIndices.Count)
						{
							particleIndices.Add(i);
							break;
						}
					
						ref Particle particle2 = ref particles[particleIndices[j]];
						float d2 = Vector3.Dot(particle2.position, cameraAxis);

						if (d1 > d2)
						{
							particleIndices.Insert(j, i);
							break;
						}
					}
					*/
				}
			}
			else
			{
				//Debug.Assert(false);
				//if (i == numParticles - 1)
				//	numParticles--;
			}
		}

		/*
		Vector3 cameraAxis = Vector3.Forward;
		if (Renderer.camera != null)
			cameraAxis = Renderer.camera.rotation.forward;

		Array.Sort(particles, (particle1, particle2) =>
		{
			if (!particle1.active && !particle2.active)
				return 0;
			if (!particle1.active)
				return 1;
			if (!particle2.active)
				return -1;

			float d1 = Vector3.Dot(particle1.position, cameraAxis);
			float d2 = Vector3.Dot(particle2.position, cameraAxis);

			return d1 < d2 ? 1 : d1 > d2 ? -1 : 0;
		});

		for (int i = 0; i < particles.Length; i++)
		{
			if (!particles[i].active)
			{
				numParticles = i;
				break;
			}
		}
		*/
	}

	public void draw(GraphicsDevice graphics)
	{
		if (particleIndices.Count > 0)
		{
			int atlasColumns = 1, atlasRows = 1;
			if (frameWidth != 0 && frameHeight != 0 && textureAtlas != null)
			{
				atlasColumns = textureAtlas.width / frameWidth;
				atlasRows = textureAtlas.height / frameHeight;
			}
			Renderer.DrawParticleSystem(particles, particleIndices, transform, spawnOffset, followMode, textureAtlas, new Vector2i(atlasColumns, atlasRows), linearFiltering, additive);
		}
	}

	public int numParticles
	{
		get => particleIndices.Count;
	}
}
