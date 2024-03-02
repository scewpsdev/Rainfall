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


	Matrix transform = Matrix.Identity;
	Vector3 entityVelocity = Vector3.Zero;
	Quaternion entityRotationVelocity = Quaternion.Identity;

	public string name = null;

	public float lifetime = 1.0f;
	public float size = 0.1f;
	public bool follow = false;

	public float emissionRate = 5.0f;
	public ParticleSpawnShape spawnShape = ParticleSpawnShape.Point;
	public Vector3 spawnOffset = Vector3.Zero;
	public float spawnRadius = 1.0f;
	public Vector3 lineEnd = new Vector3(1.0f, 0.0f, 0.0f);
	public bool randomStartRotation = false;

	public float gravity = 0.0f;
	public float drag = 0.0f;
	public Vector3 startVelocity = new Vector3(0.0f, 1.0f, 0.0f);
	public float radialVelocity = 0.0f;
	public float rotationSpeed = 0.0f;
	public bool applyEntityVelocity = false;
	public bool applyCentrifugalForce = false;

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
	public float velocityNoise = 0.0f;

	public Gradient<float> sizeAnim = null;
	public Gradient<Vector4> colorAnim = null;

	Particle[] particles = null;
	List<int> particleIndices;
	public readonly int maxParticles = 0;

	long lastEmitted;

	Random random;
	Simplex simplex;

	ParticleComparator particleComparator;


	public ParticleSystem(int maxParticles)
	{
		this.maxParticles = maxParticles;
		particles = new Particle[maxParticles];
		particleIndices = new List<int>(maxParticles);

		random = new Random();
		simplex = new Simplex(0);

		particleComparator = new ParticleComparator();

		lastEmitted = Time.currentTime;
	}

	public void copyData(ParticleSystem from)
	{
		name = from.name;

		lifetime = from.lifetime;
		size = from.size;
		follow = from.follow;

		emissionRate = from.emissionRate;
		spawnShape = from.spawnShape;
		spawnOffset = from.spawnOffset;
		spawnRadius = from.spawnRadius;
		lineEnd = from.lineEnd;
		randomStartRotation = from.randomStartRotation;

		gravity = from.gravity;
		drag = from.drag;
		startVelocity = from.startVelocity;
		rotationSpeed = from.rotationSpeed;
		applyEntityVelocity = from.applyEntityVelocity;

		textureAtlasPath = from.textureAtlasPath;
		textureAtlas = from.textureAtlas;
		atlasSize = from.atlasSize;
		numFrames = from.numFrames;
		linearFiltering = from.linearFiltering;

		color = from.color;
		additive = from.additive;

		randomVelocity = from.randomVelocity;
		randomRotationSpeed = from.randomRotationSpeed;
		randomLifetime = from.randomLifetime;

		sizeAnim = from.sizeAnim != null ? new Gradient<float>(from.sizeAnim) : null;
		colorAnim = from.colorAnim != null ? new Gradient<Vector4>(from.colorAnim) : null;
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

	public void emitParticle(Vector3 particleVelocity, int num = 1)
	{
		for (int i = 0; i < num; i++)
		{
			Vector3 localPosition = Vector3.Zero;
			float rotation = randomStartRotation ? random.NextSingle() * MathF.PI * 2.0f : 0.0f;

			switch (spawnShape)
			{
				case ParticleSpawnShape.Point:
					localPosition = Vector3.Zero;
					break;
				case ParticleSpawnShape.Circle:
					float r = spawnRadius * MathF.Sqrt((float)random.NextDouble());
					float theta = (float)random.NextDouble() * 2.0f * MathF.PI;
					localPosition = new Vector3(r * MathF.Cos(theta), 0.0f, r * MathF.Sin(theta));
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
					localPosition = new Vector3(x, y, z) * spawnRadius;
					break;
				case ParticleSpawnShape.Line:
					float t = (float)random.NextDouble();
					localPosition = Vector3.Lerp(Vector3.Zero, lineEnd, t);
					break;
				default:
					Debug.Assert(false);
					break;
			}

			//position += particleOffset;
			Vector3 position = localPosition;

			if (!follow)
				position = (transform * new Vector4(position + spawnOffset, 1.0f)).xyz;

			Vector3 velocity = startVelocity + particleVelocity;
			if (applyEntityVelocity)
				velocity += entityVelocity;
			if (applyCentrifugalForce)
			{
				float rotationAngle = entityRotationVelocity.angle;
				if (rotationAngle > 0)
				{
					Vector3 rotationAxis = entityRotationVelocity.axis;
					float angularVelocity = rotationAngle / Time.deltaTime; // to be exact, angular velocity would be w = angle / 2pi / t. but we would multiply by 2pi anyways for calculating the linear velocity (v = w * 2pi * r)
					Vector3 fromCenter = position - transform.translation;
					Vector3 projectedCenter = transform.translation + rotationAxis * Vector3.Dot(rotationAxis, fromCenter);
					Vector3 fromRotationAxis = position - projectedCenter;
					Vector3 centrifugalVelocity = angularVelocity * fromRotationAxis; // w * 2pi * r
					velocity += centrifugalVelocity;
				}
			}
			if (randomVelocity > 0)
				velocity += MathHelper.RandomVector3(random) * randomVelocity;
			if (radialVelocity > 0)
				velocity += (transform * new Vector4(localPosition, 0.0f)).xyz.normalized * radialVelocity;
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

	Vector3 sampleVelocityNoise(float t)
	{
		return new Vector3(simplex.sample1f(t),
			simplex.sample1f(t + 100),
			simplex.sample1f(t + 200)).normalized;
	}

	public void update(Matrix transform)
	{
		entityVelocity = (transform.translation - this.transform.translation) / Time.deltaTime;
		entityRotationVelocity = transform.rotation * this.transform.rotation.conjugated;
		this.transform = transform;


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
				particle.velocity += drag * particle.velocity.lengthSquared * -particle.velocity.normalized / 2;
				particle.position += particle.velocity * Time.deltaTime;
				if (velocityNoise > 0)
					particle.position += velocityNoise * sampleVelocityNoise((Time.currentTime + Hash.hash(i)) / 1e9f) * Time.deltaTime;
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
