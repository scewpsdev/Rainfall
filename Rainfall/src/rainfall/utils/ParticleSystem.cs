using Rainfall;
using Rainfall.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
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

[StructLayout(LayoutKind.Sequential)]
public unsafe struct Gradient_float_2
{
	public struct Value
	{
		public float value;
		public float position;
	}

	public Value value0;
	public Value value1;
	public int count;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct Gradient_Vector4_2
{
	public struct Value
	{
		public Vector4 value;
		public float position;
	}

	public Value value0;
	public Value value1;
	public int count;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ParticleBurst
{
	public float time;
	public int count;
	public float duration;

	public int emitted;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ParticleSystemData
{
	public Matrix transform = Matrix.Identity;
	public Vector3 entityVelocity;
	public Quaternion entityRotationVelocity = Quaternion.Identity;

	public float lifetime = 1;
	public float size = 0.1f;
	public byte follow;

	public float emissionRate = 5;
	public ParticleSpawnShape spawnShape;
	public Vector3 spawnOffset;
	public float spawnRadius = 1;
	public Vector3 lineSpawnEnd = Vector3.Right;

	public float gravity;
	public float drag;
	public Vector3 startVelocity;
	public float radialVelocity;
	public float startRotation;
	public float rotationSpeed;
	public bool applyEntityVelocity;
	public bool applyCentrifugalForce;

	public fixed byte textureAtlasPath[256];
	public ushort textureAtlas = ushort.MaxValue;
	public Vector2i atlasSize = Vector2i.One;
	public int numFrames = 1;
	public byte linearFiltering;

	public Vector4 color = Vector4.One;
	public byte additive;
	public float emissiveIntensity;
	public float lightInfluence = 1;

	public Vector3 randomVelocity;
	public float randomRotation;
	public float randomRotationSpeed;
	public float randomLifetime;
	public float velocityNoise;

	public Gradient_float_2 sizeAnim;
	public Gradient_Vector4_2 colorAnim;

	public int numBursts;
	public ParticleBurst* bursts;

	public ParticleSystemData(int _)
	{
	}
}

public unsafe class ParticleSystem
{
	static List<ParticleSystem> particleSystems = new List<ParticleSystem>();

	public static ParticleSystem Create(Matrix transform, int maxParticles = 400)
	{
		ParticleSystem system = new ParticleSystem(maxParticles, transform);
		particleSystems.Add(system);
		return system;
	}

	public static void Destroy(ParticleSystem particleSystem)
	{
		particleSystem.destroy();
		if (particleSystems.Contains(particleSystem))
			particleSystems.Remove(particleSystem);
	}

	public static void Update(Vector3 cameraPosition)
	{
		/*
		for (int i = 0; i < particleSystems.Count; i++)
		{
			Vector3 toCamera = cameraPosition - particleSystems[i].transform.translation;
			float distanceSq = toCamera.lengthSquared;
			float maxDistance = 20;
			if (distanceSq < maxDistance * maxDistance)
			particleSystems[i].update();
		}
		*/

		void updateParticleSystem(int idx)
		{
			particleSystems[idx].update();
		}
		Parallel.For(0, particleSystems.Count, updateParticleSystem);
	}

	public static int numParticleSystems
	{
		get => particleSystems.Count;
	}


	public ParticleSystemData* handle;


	ParticleSystem(int maxParticles, Matrix transform)
	{
		handle = ParticleSystem_Create(maxParticles, transform);
	}

	void destroy()
	{
		ParticleSystem_Destroy(handle);
		handle = null;
	}

	public unsafe void setData(ParticleSystemData data)
	{
		*handle = data;
	}

	public void restartEffect()
	{
		ParticleSystem_Restart(handle);
	}

	public void emitParticle(int num = 1)
	{
		for (int i = 0; i < num; i++)
			ParticleSystem_EmitParticle(handle);
	}

	public void setTransform(Matrix transform, bool applyVelocity = false)
	{
		ParticleSystem_SetTransform(handle, transform, (byte)(applyVelocity ? 1 : 0));
	}

	public void setCameraAxis(Vector3 cameraAxis)
	{
	}

	void update()
	{
		ParticleSystem_Update(handle);
	}

	public int numParticles
	{
		get => ParticleSystem_GetNumParticles(handle);
	}

	public bool hasFinished
	{
		get => ParticleSystem_HasFinished(handle) != 0;
	}

	[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
	static extern unsafe ParticleSystemData* ParticleSystem_Create(int maxParticles, Matrix transform);

	[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
	static extern unsafe void ParticleSystem_Destroy(ParticleSystemData* system);

	[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
	static extern unsafe void ParticleSystem_Restart(ParticleSystemData* system);

	[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
	static extern unsafe void ParticleSystem_SetTransform(ParticleSystemData* system, Matrix transform, byte applyVelocity);

	[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
	static extern unsafe void ParticleSystem_EmitParticle(ParticleSystemData* system);

	[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
	static extern unsafe void ParticleSystem_Update(ParticleSystemData* system);

	[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
	static extern unsafe int ParticleSystem_GetNumParticles(ParticleSystemData* system);

	[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
	static extern unsafe byte ParticleSystem_HasFinished(ParticleSystemData* system);
}
