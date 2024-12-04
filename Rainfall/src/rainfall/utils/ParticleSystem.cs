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


namespace Rainfall
{
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
	public unsafe struct Gradient_float_3
	{
		public struct Value
		{
			public float value;
			public float position;
		}

		public Value value0;
		public Value value1;
		public Value value2;
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
	public unsafe struct Gradient_Vector4_3
	{
		public struct Value
		{
			public Vector4 value;
			public float position;
		}

		public Value value0;
		public Value value1;
		public Value value2;
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
		public fixed byte name[32];
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
		byte _applyEntityVelocity;
		byte _applyCentrifugalForce;
		public bool applyEntityVelocity { get => _applyEntityVelocity != 0; set { _applyEntityVelocity = (byte)(value ? 1 : 0); } }
		public bool applyCentrifugalForce { get => _applyCentrifugalForce != 0; set { _applyCentrifugalForce = (byte)(value ? 1 : 0); } }

		public fixed byte textureAtlasPath[256];
		public ushort textureAtlas = ushort.MaxValue;
		public Vector2i atlasSize = Vector2i.One;
		public int numFrames = 1;
		public byte randomFrame;
		public byte linearFiltering;

		public Vector4 color = Vector4.One;
		public byte _additive;
		public bool additive { get => _additive != 0; set { _additive = (byte)(value ? 1 : 0); } }
		public float emissiveIntensity;
		public float lightInfluence = 1;

		public Vector3 randomVelocity;
		public float randomRotation;
		public float randomRotationSpeed;
		public float randomLifetime;
		public float velocityNoise;

		public Gradient_float_3 sizeAnim;
		public Gradient_Vector4_3 colorAnim;

		public int numBursts;
		public ParticleBurst* bursts;

		public ParticleSystemData(int _)
		{
			textureAtlasPath[0] = 0;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct ParticleData
	{
		public byte _active;
		public Vector3 position;
		public float rotation;
		public Vector3 velocity;
		public float rotationVelocity;
		public float size;
		public float lifetime;
		public float animationFrame;
		public Vector4 color;

		public long birthTime;

		public bool active
		{
			get => _active != 0;
			set { _active = (byte)(value ? 1 : 0); }
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
			handle = Native.ParticleSystem.ParticleSystem_Create(maxParticles, transform);
		}

		void destroy()
		{
			Native.ParticleSystem.ParticleSystem_Destroy(handle);
			handle = null;
		}

		public unsafe void setData(ParticleSystemData data)
		{
			*handle = data;
		}

		public void restartEffect()
		{
			Native.ParticleSystem.ParticleSystem_Restart(handle);
		}

		public void emitParticle(int num = 1)
		{
			for (int i = 0; i < num; i++)
				Native.ParticleSystem.ParticleSystem_EmitParticle(handle);
		}

		public void setTransform(Matrix transform, bool applyVelocity = false)
		{
			Native.ParticleSystem.ParticleSystem_SetTransform(handle, transform, (byte)(applyVelocity ? 1 : 0));
		}

		public void setCameraAxis(Vector3 cameraAxis)
		{
		}

		public void update()
		{
			Native.ParticleSystem.ParticleSystem_Update(handle);
		}

		public int numParticles
		{
			get => Native.ParticleSystem.ParticleSystem_GetNumParticles(handle);
		}

		public unsafe ParticleData* data
		{
			get => Native.ParticleSystem.ParticleSystem_GetParticleData(handle);
		}

		public bool hasFinished
		{
			get => Native.ParticleSystem.ParticleSystem_HasFinished(handle) != 0;
		}
	}
}

namespace Rainfall.Native
{
	public class ParticleSystem
	{
		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe ParticleSystemData* ParticleSystem_Create(int maxParticles, Matrix transform);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void ParticleSystem_Destroy(ParticleSystemData* system);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void ParticleSystem_Restart(ParticleSystemData* system);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void ParticleSystem_SetTransform(ParticleSystemData* system, Matrix transform, byte applyVelocity);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void ParticleSystem_EmitParticle(ParticleSystemData* system);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void ParticleSystem_Update(ParticleSystemData* system);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe int ParticleSystem_GetNumParticles(ParticleSystemData* system);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe ParticleData* ParticleSystem_GetParticleData(ParticleSystemData* system);

		[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe byte ParticleSystem_HasFinished(ParticleSystemData* system);
	}
}
