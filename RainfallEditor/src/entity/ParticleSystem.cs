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

public struct Particle
{
	public bool active;
	public Vector3 position;
	public float rotation;
	public Vector3 velocity;
	public float rotationVelocity;
	public float size;
	public float lifetime;
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

	public string name = null;

	public float lifetime = 1.0f;
	public float size = 0.1f;

	public float emissionRate = 5.0f;
	public ParticleSpawnShape spawnShape = ParticleSpawnShape.Point;
	public Vector3 spawnOffset = Vector3.Zero;
	public float spawnRadius = 1.0f;
	public Vector3 lineEnd = new Vector3(1.0f, 0.0f, 0.0f);
	public bool randomStartRotation = false;

	public bool follow = false;
	public float gravity = 0.0f;
	public Vector3 startVelocity = new Vector3(0.0f, 1.0f, 0.0f);
	public float rotationSpeed = 0.0f;

	public string textureAtlasPath = null;
	public Texture textureAtlas = null;
	public Vector2i atlasSize = new Vector2i(1);
	public int numFrames = 1;
	public bool linearFiltering = false;

	public Vector4 color = Vector4.One;
	public bool additive = false;

	public float randomVelocity = 0.0f;
	public float randomRotationSpeed = 0.0f;
	public float randomLifetime = 0.0f;

	public Gradient<float> sizeAnim = null;
	public Gradient<Vector4> colorAnim = null;

	Particle[] particles = null;
	List<int> particleIndices;
	public readonly int maxParticles = 0;

	long lastEmitted;

	Random random;

	ParticleComparator particleComparator;


	public ParticleSystem(int maxParticles)
	{
		this.maxParticles = maxParticles;
		particles = new Particle[maxParticles];
		particleIndices = new List<int>(maxParticles);

		random = new Random();

		particleComparator = new ParticleComparator();

		lastEmitted = Time.currentTime;
	}

	public void copyData(ParticleSystem from)
	{
		name = from.name;
		emissionRate = from.emissionRate;
		lifetime = from.lifetime;
		size = from.size;
		spawnOffset = from.spawnOffset;
		spawnShape = from.spawnShape;
		follow = from.follow;
		gravity = from.gravity;
		startVelocity = from.startVelocity;
		rotationSpeed = from.rotationSpeed;
		textureAtlasPath = from.textureAtlasPath;
		textureAtlas = from.textureAtlas;
		atlasSize = from.atlasSize;
		numFrames = from.numFrames;
		linearFiltering = from.linearFiltering;
		color = from.color;
		additive = from.additive;
		spawnRadius = from.spawnRadius;
		lineEnd = from.lineEnd;
		randomVelocity = from.randomVelocity;
		randomRotationSpeed = from.randomRotationSpeed;
		randomStartRotation = from.randomStartRotation;
		randomLifetime = from.randomLifetime;
		sizeAnim = from.sizeAnim != null ? new Gradient<float>(from.sizeAnim) : null;
		colorAnim = from.colorAnim != null ? new Gradient<Vector4>(from.colorAnim) : null;
	}

	public override bool Equals(object obj)
	{
		return obj is ParticleSystem system &&
			   EqualityComparer<Matrix>.Default.Equals(transform, system.transform) &&
			   name == system.name &&
			   lifetime == system.lifetime &&
			   size == system.size &&
			   emissionRate == system.emissionRate &&
			   spawnShape == system.spawnShape &&
			   EqualityComparer<Vector3>.Default.Equals(spawnOffset, system.spawnOffset) &&
			   spawnRadius == system.spawnRadius &&
			   EqualityComparer<Vector3>.Default.Equals(lineEnd, system.lineEnd) &&
			   follow == system.follow &&
			   gravity == system.gravity &&
			   EqualityComparer<Vector3>.Default.Equals(startVelocity, system.startVelocity) &&
			   rotationSpeed == system.rotationSpeed &&
			   textureAtlasPath == system.textureAtlasPath &&
			   EqualityComparer<Texture>.Default.Equals(textureAtlas, system.textureAtlas) &&
			   atlasSize == system.atlasSize &&
			   numFrames == system.numFrames &&
			   linearFiltering == system.linearFiltering &&
			   EqualityComparer<Vector4>.Default.Equals(color, system.color) &&
			   additive == system.additive &&
			   randomVelocity == system.randomVelocity &&
			   randomRotationSpeed == system.randomRotationSpeed &&
			   randomStartRotation == system.randomStartRotation &&
			   randomLifetime == system.randomLifetime &&
			   EqualityComparer<Gradient<float>>.Default.Equals(sizeAnim, system.sizeAnim) &&
			   EqualityComparer<Gradient<Vector4>>.Default.Equals(colorAnim, system.colorAnim);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public void reload()
	{
		if (textureAtlasPath != null)
		{
			string compiledPath = RainfallEditor.instance.compileAsset(textureAtlasPath);
			textureAtlas = Resource.GetTexture(compiledPath);
		}
		else
		{
			textureAtlas = null;
		}
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

	public void emitParticle(Vector3 particleOffset, Vector3 particleVelocity, int num = 1)
	{
		for (int i = 0; i < num; i++)
		{
			Vector3 position = Vector3.Zero;
			float rotation = randomStartRotation ? Random.Shared.NextSingle() * MathF.PI * 2.0f : 0.0f;

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
					position = Vector3.Lerp(Vector3.Zero, lineEnd, t);
					break;
				default:
					Debug.Assert(false);
					break;
			}

			position += particleOffset;

			if (!follow)
			{
				position = (transform * new Vector4(position + spawnOffset, 1.0f)).xyz;
			}

			Vector3 velocity = startVelocity + particleVelocity;
			if (randomVelocity > 0)
				velocity += MathHelper.RandomVector3(random) * randomVelocity;
			float rotationVelocity = 0.0f;
			if (randomRotationSpeed > 0)
				rotationVelocity += MathHelper.RandomFloat(-1, 1, random) * randomRotationSpeed;

			int particleID = getNewParticle();
			if (particleID != -1)
			{
				ref Particle particle = ref particles[particleID];
				particle.position = position;
				particle.rotation = rotation;
				particle.velocity = velocity;
				particle.rotationVelocity = rotationVelocity;
				particle.size = size;
				particle.lifetime = lifetime * (1 + MathHelper.RandomFloat(-randomLifetime, randomLifetime, random));
				particle.animationFrame = 0;
				particle.color = color;
				particle.birthTime = Time.currentTime;
			}
		}
	}

	public void emitParticle(Vector3 particleVelocity, int num = 1)
	{
		emitParticle(Vector3.Zero, particleVelocity, num);
	}

	public void update()
	{
		long now = Time.currentTime;
		if (emissionRate > 0.0f)
		{
			if (now - lastEmitted > 1e9 / emissionRate)
			{
				emitParticle(Vector3.Zero, Vector3.Zero, 1);
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
				particle.position += particle.velocity * Time.deltaTime;
				particle.velocity.y += 0.5f * gravity * Time.deltaTime;

				particle.rotation += particle.rotationVelocity * Time.deltaTime;

				float progress = particleTimer / particle.lifetime;

				if (sizeAnim != null)
					particle.size = sizeAnim.getValue(progress);

				if (colorAnim != null)
					particle.color = colorAnim.getValue(progress);

				if (textureAtlas != null && numFrames > 0)
				{
					float animationFrame = particleTimer / particle.lifetime * numFrames / (atlasSize.x * atlasSize.y);
					particle.animationFrame = animationFrame;
				}

				if ((now - particle.birthTime) / 1e9f >= particle.lifetime)
				{
					particle.active = false;
				}
				else
				{
					int index = particleIndices.BinarySearch(i, particleComparator);
					if (index < 0)
						index = ~index;
					particleIndices.Insert(index, i);
				}
			}
		}
	}

	public void draw(GraphicsDevice graphics)
	{
		if (particleIndices.Count > 0)
		{
			Renderer.DrawParticleSystem(particles, particleIndices, transform, spawnOffset, follow, textureAtlas, atlasSize, linearFiltering, additive);
		}
	}

	public int numParticles
	{
		get => particleIndices.Count;
	}
}
