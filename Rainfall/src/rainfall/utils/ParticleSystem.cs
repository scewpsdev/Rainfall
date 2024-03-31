using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static System.Runtime.InteropServices.JavaScript.JSType;


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

public struct ParticleBurst
{
	public float time;
	public int count;
	public float duration;

	public int emitted;

	public ParticleBurst(float time, int count, float duration)
	{
		this.time = time;
		this.count = count;
		this.duration = duration;

		emitted = 0;
	}
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

	static ParticleComparator particleComparator = new ParticleComparator();


	public Matrix transform { get; private set; } = Matrix.Identity;
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

	public float gravity = 0.0f;
	public float drag = 0.0f;
	public Vector3 startVelocity = Vector3.Zero;
	public float radialVelocity = 0.0f;
	public float startRotation = 0.0f;
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
	public float emissiveIntensity = 0.0f;

	public Vector3 randomVelocity = Vector3.Zero;
	public float randomRotation = 0.0f;
	public float randomRotationSpeed = 0.0f;
	public float randomLifetime = 0.0f;
	public float velocityNoise = 0.0f;

	public Gradient<float> sizeAnim = null;
	public Gradient<Vector4> colorAnim = null;

	public List<ParticleBurst> bursts = null;

	public Particle[] particles { get; private set; } = null;
	public List<int> particleIndices { get; private set; }
	public readonly int maxParticles = 0;

	long systemStarted, lastEmitted;

	public BoundingSphere boundingSphere { get; private set; }

	Random random;
	Simplex simplex;


	public ParticleSystem(int maxParticles, Matrix transform)
	{
		this.transform = transform;

		this.maxParticles = maxParticles;
		particles = new Particle[maxParticles];
		particleIndices = new List<int>(maxParticles);

		random = new Random();
		simplex = new Simplex(0);

		restartEffect();
	}

	public ParticleSystem(int maxParticles)
		: this(maxParticles, Matrix.Identity)
	{
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

		gravity = from.gravity;
		drag = from.drag;
		startVelocity = from.startVelocity;
		radialVelocity = from.radialVelocity;
		startRotation = from.startRotation;
		rotationSpeed = from.rotationSpeed;
		applyEntityVelocity = from.applyEntityVelocity;
		applyCentrifugalForce = from.applyCentrifugalForce;

		textureAtlasPath = from.textureAtlasPath;
		textureAtlas = from.textureAtlas;
		atlasSize = from.atlasSize;
		numFrames = from.numFrames;
		linearFiltering = from.linearFiltering;

		color = from.color;
		additive = from.additive;
		emissiveIntensity = from.emissiveIntensity;

		randomVelocity = from.randomVelocity;
		randomRotation = from.randomRotation;
		randomRotationSpeed = from.randomRotationSpeed;
		randomLifetime = from.randomLifetime;
		velocityNoise = from.velocityNoise;

		sizeAnim = from.sizeAnim != null ? new Gradient<float>(from.sizeAnim) : null;
		colorAnim = from.colorAnim != null ? new Gradient<Vector4>(from.colorAnim) : null;

		bursts = from.bursts != null ? new List<ParticleBurst>(from.bursts) : null;
	}

	public void restartEffect()
	{
		systemStarted = Time.currentTime;
		lastEmitted = Time.currentTime;

		if (bursts != null)
		{
			for (int i = 0; i < bursts.Count; i++)
			{
				ParticleBurst burst = bursts[i];
				burst.emitted = 0;
				bursts[i] = burst;
			}
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
		}
		return -1;
	}

	public void emitParticle(Vector3 particleVelocity, int num = 1)
	{
		for (int i = 0; i < num; i++)
		{
			int particleID = getNewParticle();
			if (particleID != -1)
			{
				Vector3 localPosition = Vector3.Zero;
				float rotation = startRotation;
				if (randomRotation > 0)
					rotation += MathHelper.RandomFloat(-MathF.PI, MathF.PI, random) * randomRotation;

				switch (spawnShape)
				{
					case ParticleSpawnShape.Point:
						localPosition = Vector3.Zero;
						break;
					case ParticleSpawnShape.Circle:
						{
							// sqrt the random number to get an even distribution in the circle
							float r = spawnRadius * MathF.Sqrt(random.NextSingle());
							float theta = random.NextSingle() * 2.0f * MathF.PI;
							localPosition = new Vector3(r * MathF.Cos(theta), 0.0f, r * MathF.Sin(theta));
						}
						break;
					case ParticleSpawnShape.Sphere:
						{
							float r = spawnRadius * MathF.Pow(random.NextSingle(), 0.333333f);
							float theta = random.NextSingle() * 2.0f * MathF.PI;
							float phi = random.NextSingle() * MathF.PI;
							localPosition = new Vector3(r * MathF.Sin(phi) * MathF.Cos(theta), r * MathF.Sin(phi) * MathF.Sin(theta), r * MathF.Cos(phi));
						}
						break;
					case ParticleSpawnShape.Line:
						float t = random.NextSingle();
						localPosition = Vector3.Lerp(Vector3.Zero, lineEnd, t);
						if (spawnRadius > 0)
						{

							float r = spawnRadius * random.NextSingle();
							float theta = random.NextSingle() * 2.0f * MathF.PI;
							float phi = random.NextSingle() * MathF.PI;
							localPosition += new Vector3(r * MathF.Sin(phi) * MathF.Cos(theta), r * MathF.Sin(phi) * MathF.Sin(theta), r * MathF.Cos(phi));
						}
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
				if (randomVelocity.lengthSquared > 0)
					velocity += MathHelper.RandomVector3(-1, 1, random).normalized * randomVelocity;
				if (radialVelocity != 0)
				{
					if (spawnShape == ParticleSpawnShape.Point)
						velocity += MathHelper.RandomPointOnSphere() * radialVelocity;
					else
						velocity += (transform * new Vector4(localPosition, 0.0f)).xyz.normalized * radialVelocity;
				}

				float rotationVelocity = 0.0f;
				if (randomRotationSpeed > 0)
					rotationVelocity += MathHelper.RandomFloat(-1, 1, random) * randomRotationSpeed;

				float particleLifetime = lifetime;
				if (randomLifetime > 0)
					particleLifetime *= 1 + MathHelper.RandomFloat(-randomLifetime, randomLifetime, random);

				ref Particle particle = ref particles[particleID];
				particle.position = position;
				particle.rotation = rotation;
				particle.velocity = velocity;
				particle.rotationVelocity = rotationVelocity;
				particle.size = size;
				particle.lifetime = particleLifetime;
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

	public void setTransform(Matrix transform, bool applyVelocity = false)
	{
		if (applyVelocity)
		{
			entityVelocity = (transform.translation - this.transform.translation) / Time.deltaTime;
			entityRotationVelocity = transform.rotation * this.transform.rotation.conjugated;
		}
		this.transform = transform;
	}

	public void setCameraAxis(Vector3 cameraAxis)
	{
		particleComparator.cameraAxis = cameraAxis;
	}

	public void update(Matrix transform)
	{
		setTransform(transform, true);


		long now = Time.currentTime;
		if (emissionRate > 0.0f)
		{
			if (now - lastEmitted > 1e9 / emissionRate)
			{
				int numParticles = (int)MathF.Floor((now - lastEmitted) / 1e9f * emissionRate);
				emitParticle(Vector3.Zero, numParticles);
				lastEmitted = now;
			}
		}
		if (bursts != null)
		{
			float elapsed = (Time.currentTime - systemStarted) / 1e9f;
			for (int i = 0; i < bursts.Count; i++)
			{
				ParticleBurst burst = bursts[i];
				if (elapsed > burst.time && burst.emitted < burst.count)
				{
					int shouldEmitted = burst.duration > 0 ? (int)(MathF.Min((elapsed - burst.time) / burst.duration, 1.0f) * burst.count) : burst.count;
					int delta = shouldEmitted - burst.emitted;
					if (delta > 0)
					{
						emitParticle(Vector3.Zero, delta);
						burst.emitted = shouldEmitted;
					}
				}
				bursts[i] = burst;
			}
		}

		Vector3 min = new Vector3(float.MaxValue), max = new Vector3(float.MinValue);
		float maxRadiusSq = 0.0f;

		particleComparator.particles = particles;
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

				min = Vector3.Min(min, particle.position - particle.size);
				max = Vector3.Max(max, particle.position + particle.size);
				maxRadiusSq = MathF.Max(maxRadiusSq, (particle.position - boundingSphere.center).lengthSquared);

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

		boundingSphere = new BoundingSphere { center = (min + max) * 0.5f, radius = MathF.Sqrt(maxRadiusSq) };
	}

	public int numParticles
	{
		get => particleIndices.Count;
	}

	public bool hasFinished
	{
		get
		{
			if (emissionRate > 0)
				return false;
			if (numParticles > 0)
				return false;

			bool allBurstsEmitted = true;
			if (bursts != null && bursts.Count > 0 && numParticles == 0)
			{
				for (int j = 0; j < bursts.Count; j++)
				{
					if (bursts[j].emitted < bursts[j].count)
					{
						allBurstsEmitted = false;
						break;
					}
				}
			}

			if (!allBurstsEmitted)
				return false;

			return true;
		}
	}
}
