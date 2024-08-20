using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public static class MathHelper
	{
		public const float PiOver2 = MathF.PI * 0.5f;
		public const float Sqrt2 = 1.414213562373095f;
		public const float Sqrt5 = 2.23606797749979f;


		public static int IPow(int x, int exp)
		{
			int ret = 1;
			while (exp != 0)
			{
				if ((exp & 1) == 1)
					ret *= x;
				x *= x;
				exp >>= 1;
			}
			return ret;
		}

		public static float ToRadians(float degrees)
		{
			return degrees / 180.0f * MathF.PI;
		}

		public static float ToDegrees(float radians)
		{
			return radians / MathF.PI * 180.0f;
		}

		public static float Linear(float v, float dst, float d)
		{
			if (dst > v)
				return MathF.Min(v + d, dst);
			else if (dst < v)
				return MathF.Max(v - d, dst);
			return dst;
		}

		public static float Lerp(float a, float b, float t)
		{
			return a + (b - a) * t;
		}

		public static float LerpAngle(float a, float b, float t)
		{
			a = (a + MathF.PI * 2.0f) % (MathF.PI * 2.0f);
			b = (b + MathF.PI * 2.0f) % (MathF.PI * 2.0f);
			if (a - b > MathF.PI)
				a -= MathF.PI * 2.0f;
			else if (b - a > MathF.PI)
				b -= MathF.PI * 2.0f;

			return a + (b - a) * t;
		}

		public static bool CompareAngles(float a, float b, float epsilon = float.Epsilon)
		{
			a = (a + MathF.PI * 2.0f) % (MathF.PI * 2.0f);
			b = (b + MathF.PI * 2.0f) % (MathF.PI * 2.0f);
			if (a - b > MathF.PI)
				a -= MathF.PI * 2.0f;
			else if (b - a > MathF.PI)
				b -= MathF.PI * 2.0f;

			return MathF.Abs(a - b) < epsilon;
		}

		public static float Remap(float f, float min, float max, float newMin, float newMax)
		{
			return (f - min) / (max - min) * (newMax - newMin) + newMin;
		}

		public static float Clamp(float f, float min, float max)
		{
			return Math.Max(Math.Min(f, max), min);
		}

		public static int Clamp(int i, int min, int max)
		{
			return Math.Max(Math.Min(i, max), min);
		}

		public static int Step(float edge, float x)
		{
			return x < edge ? 0 : 1;
		}

		public static float Smoothstep(float edge0, float edge1, float x)
		{
			float t = Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
			return t * t * (3.0f - 2.0f * t);
		}

		public static void Swap(ref float a, ref float b)
		{
			float tmp = a;
			a = b;
			b = tmp;
		}

		public static Vector4 ARGBToVector(uint argb)
		{
			byte r = (byte)((argb & 0xFF0000) >> 16);
			byte g = (byte)((argb & 0xFF00) >> 8);
			byte b = (byte)((argb & 0xFF));
			byte a = (byte)((argb & 0xFF000000) >> 24);
			return new Vector4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
		}

		public static uint VectorToARGB(Vector4 v)
		{
			byte r = (byte)(MathF.Min(v.x * 255, 255));
			byte g = (byte)(MathF.Min(v.y * 255, 255));
			byte b = (byte)(MathF.Min(v.z * 255, 255));
			byte a = (byte)(MathF.Min(v.w * 255, 255));
			uint color = (uint)((a << 24) | (r << 16) | (g << 8) | b);
			return color;
		}

		public static Vector3 SRGBToLinear(float r, float g, float b)
		{
			float gamma = 2.2f;
			float rr = MathF.Pow(r, gamma);
			float gg = MathF.Pow(g, gamma);
			float bb = MathF.Pow(b, gamma);
			return new Vector3(rr, gg, bb);
		}

		public static uint ColorAlpha(uint color, float alpha)
		{
			byte a = (byte)(alpha * 255);
			color = (color & 0x00FFFFFFu) | (uint)(a << 24);
			return color;
		}

		public static Vector2i WorldToScreenSpace(Vector3 p, Matrix pv, Vector2i displaySize)
		{
			Vector4 clipSpacePosition = pv * new Vector4(p, 1.0f);
			Vector3 ndcSpacePosition = clipSpacePosition.xyz / clipSpacePosition.w;
			if (ndcSpacePosition.z >= -1.0f && ndcSpacePosition.z <= 1.0f)
			{
				Vector2 windowSpacePosition = ndcSpacePosition.xy * 0.5f + 0.5f;
				Vector2i pixelPosition = new Vector2i(
					(int)(windowSpacePosition.x * displaySize.x + 0.5f),
					displaySize.y - (int)(windowSpacePosition.y * displaySize.y + 0.5f)
				);
				return pixelPosition;
			}
			else
			{
				return new Vector2i(-1, -1);
			}
		}

		public static float BarryCentric(Vector3 p1, Vector3 p2, Vector3 p3, Vector2 pos)
		{
			float det = (p2.z - p3.z) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.z - p3.z);
			float l1 = ((p2.z - p3.z) * (pos.x - p3.x) + (p3.x - p2.x) * (pos.y - p3.z)) / det;
			float l2 = ((p3.z - p1.z) * (pos.x - p3.x) + (p1.x - p3.x) * (pos.y - p3.z)) / det;
			float l3 = 1.0f - l1 - l2;
			return l1 * p1.y + l2 * p2.y + l3 * p3.y;
		}

		public static int RandomInt(int min, int max, Random random)
		{
			return min + random.Next() % (max - min + 1);
		}

		public static int RandomInt(int min, int max)
		{
			return RandomInt(min, max, Random.Shared);
		}

		public static float RandomFloat(float min, float max, Random random)
		{
			return min + random.NextSingle() * (max - min);
		}

		public static float RandomFloat(float min, float max)
		{
			return RandomFloat(min, max, Random.Shared);
		}

		public static float RandomFloatGaussian()
		{
			float f = RandomFloat(-1.0f, 1.0f, Random.Shared);
			return f * MathF.Abs(f);
		}

		public static Vector2 RandomVector2(float min, float max, Random random)
		{
			return new Vector2(RandomFloat(min, max, random), RandomFloat(min, max, random));
		}

		public static Vector2 RandomVector2(float min, float max)
		{
			return RandomVector2(min, max, Random.Shared);
		}

		public static Vector3 RandomVector3(float min, float max, Random random)
		{
			return new Vector3(RandomFloat(min, max, random), RandomFloat(min, max, random), RandomFloat(min, max, random));
		}

		public static Vector3 RandomVector3(float min, float max)
		{
			return RandomVector3(min, max, Random.Shared);
		}

		public static Quaternion RandomQuaternion(Random random)
		{
			return Quaternion.FromAxisAngle(RandomVector3(-1, 1, random).normalized, random.NextSingle() * MathF.PI * 2);
		}

		public static Quaternion RandomQuaternion()
		{
			return RandomQuaternion(Random.Shared);
		}

		static float nextGaussian = float.MaxValue;
		public static float RandomGaussian(Random random)
		{
			if (nextGaussian == float.MaxValue)
			{
				float u1 = random.NextSingle();
				float u2 = random.NextSingle();
				float r = MathF.Sqrt(-2 * MathF.Log(u1));
				float t = 2 * MathF.PI * u2;
				float x = r * MathF.Cos(t);
				float y = r * MathF.Sin(t);
				nextGaussian = y;
				return x;
			}
			else
			{
				float r = nextGaussian;
				nextGaussian = float.MaxValue;
				return r;
			}
		}

		public static Vector3 RandomPointInSphere(Random random)
		{
			float x = RandomGaussian(random);
			float y = RandomGaussian(random);
			float z = RandomGaussian(random);
			return new Vector3(x, y, z);
		}

		public static Vector3 RandomPointOnSphere(Random random)
		{
			float x = RandomGaussian(random);
			float y = RandomGaussian(random);
			float z = RandomGaussian(random);
			Vector3 p = new Vector3(x, y, z);
			return p.normalized;
		}

		public static Vector3 RandomPointOnSphere()
		{
			return RandomPointOnSphere(Random.Shared);
		}

		public static Vector2 RandomPointInCircle(Random random)
		{
			float x = RandomGaussian(random);
			float y = RandomGaussian(random);
			Vector2 p = new Vector2(x, y);
			return p;
		}

		public static Vector2 RandomPointOnCircle(Random random)
		{
			float x = RandomGaussian(random);
			float y = RandomGaussian(random);
			Vector2 p = new Vector2(x, y);
			return p.normalized;
		}

		public static void ShuffleList<T>(List<T> list, Random random = null)
		{
			if (random == null)
				random = Random.Shared;
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = random.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}
}
