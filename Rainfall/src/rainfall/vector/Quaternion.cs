using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Quaternion
	{
		public static readonly Quaternion Identity = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);


		public float x, y, z, w;


		public Quaternion(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public float lengthSquared
		{
			get { return x * x + y * y + z * z + w * w; }
		}

		public float length
		{
			get { return MathF.Sqrt(x * x + y * y + z * z + w * w); }
		}

		public Vector3 axis
		{
			get
			{
				if (w < 1)
				{
					float s = 1.0f / MathF.Sqrt(1.0f - w * w);
					return new Vector3(x * s, y * s, z * s);
				}
				return Vector3.Zero;
			}
		}

		public float angle
		{
			get { return 2.0f * MathF.Acos(w); }
		}

		public Quaternion normalized
		{
			get
			{
				if (x * x + y * y + z * z + w * w > 0.0f ||
					MathF.Abs(x * x + y * y + z * z + w * w - 1.0f) > 0.0f)
				{
					float l = 1.0f / MathF.Sqrt(x * x + y * y + z * z + w * w);
					return new Quaternion(x * l, y * l, z * l, w * l);
				}
				return this;
			}
		}

		public Quaternion conjugated
		{
			get { return new Quaternion(-x, -y, -z, w); }
		}

		public Vector3 eulers
		{
			get
			{
				float ry, rx, rz;
				float test = x * w + y * z;
				if (test > 0.499f)
				{ // singularity at north pole
					ry = 2 * MathF.Atan2(y, w);
					rx = MathF.PI / 2;
					rz = 0;
					return new Vector3(rx, ry, rz);
				}
				if (test < -0.499)
				{ // singularity at south pole
					ry = -2 * MathF.Atan2(y, w);
					rx = -MathF.PI / 2;
					rz = 0;
					return new Vector3(rx, ry, rz);
				}
				float sqx = x * x;
				float sqy = y * y;
				float sqz = z * z;
				ry = MathF.Atan2(2 * y * w - 2 * x * z, 1 - 2 * sqy - 2 * sqx);
				//rx = MathF.Atan2(2 * x * w - 2 * y * z, 1 - 2 * sqx - 2 * sqz);
				rx = MathF.Asin(2 * test);
				rz = MathF.Atan2(2 * z * w - 2 * x * y, 1 - 2 * sqz - 2 * sqx);
				return new Vector3(rx, ry, rz);
			}
		}

		public Vector3 left { get { return this * Vector3.Left; } }
		public Vector3 right { get { return this * Vector3.Right; } }
		public Vector3 down { get { return this * Vector3.Down; } }
		public Vector3 up { get { return this * Vector3.Up; } }
		public Vector3 forward { get { return this * Vector3.Forward; } }
		public Vector3 back { get { return this * Vector3.Back; } }

		public override string ToString()
		{
			return x + "," + y + "," + z + "," + w;
		}

		/*
		public static Quaternion Slerp(Quaternion a, Quaternion b, float t)
		{
			float cosHalfTheta = a.w * b.w + a.x * b.x + a.y * b.y + a.z * b.z;
			if (MathF.Abs(cosHalfTheta) >= 1.0f)
				return a;

			float halfTheta = MathF.Acos(cosHalfTheta);
			float sinHalfTheta = MathF.Sqrt(1.0f - cosHalfTheta * cosHalfTheta);
			if (MathF.Abs(sinHalfTheta) < 0.001f)
				return 0.5f * a + 0.5f * b;

			float ratioA = MathF.Sin((1.0f - t) * halfTheta) / sinHalfTheta;
			float ratioB = MathF.Sin(t * halfTheta) / sinHalfTheta;

			return ratioA * a + ratioB * b;
		}
		*/

		public static Quaternion Slerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
		{
			const float SlerpEpsilon = 1e-6f;

			float t = amount;

			float cosOmega = quaternion1.x * quaternion2.x + quaternion1.y * quaternion2.y +
							 quaternion1.z * quaternion2.z + quaternion1.w * quaternion2.w;

			bool flip = false;

			if (cosOmega < 0.0f)
			{
				flip = true;
				cosOmega = -cosOmega;
			}

			float s1, s2;

			if (cosOmega > (1.0f - SlerpEpsilon))
			{
				// Too close, do straight linear interpolation.
				s1 = 1.0f - t;
				s2 = (flip) ? -t : t;
			}
			else
			{
				float omega = MathF.Acos(cosOmega);
				float invSinOmega = 1 / MathF.Sin(omega);

				s1 = MathF.Sin((1.0f - t) * omega) * invSinOmega;
				s2 = (flip)
					? -MathF.Sin(t * omega) * invSinOmega
					: MathF.Sin(t * omega) * invSinOmega;
			}

			Quaternion ans;

			ans.x = s1 * quaternion1.x + s2 * quaternion2.x;
			ans.y = s1 * quaternion1.y + s2 * quaternion2.y;
			ans.z = s1 * quaternion1.z + s2 * quaternion2.z;
			ans.w = s1 * quaternion1.w + s2 * quaternion2.w;

			return ans;
		}

		public static Quaternion operator *(Quaternion a, Quaternion b)
		{
			float w = a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z;
			float x = a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y;
			float y = a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x;
			float z = a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w;
			return new Quaternion(x, y, z, w);
		}

		public static Vector3 operator *(Quaternion a, Vector3 b)
		{
			Quaternion a1 = a.normalized;
			Quaternion a2 = a1.conjugated;

			Quaternion q;
			q.w = -a1.x * b.x - a1.y * b.y - a1.z * b.z;
			q.x = +a1.w * b.x + a1.y * b.z - a1.z * b.y;
			q.y = +a1.w * b.y - a1.x * b.z + a1.z * b.x;
			q.z = +a1.w * b.z + a1.x * b.y - a1.y * b.x;

			Quaternion q2;
			q2.w = q.w * a2.w - q.x * a2.x - q.y * a2.y - q.z * a2.z;
			q2.x = q.w * a2.x + q.x * a2.w + q.y * a2.z - q.z * a2.y;
			q2.y = q.w * a2.y - q.x * a2.z + q.y * a2.w + q.z * a2.x;
			q2.z = q.w * a2.z + q.x * a2.y - q.y * a2.x + q.z * a2.w;

			return new Vector3(q2.x, q2.y, q2.z);
		}

		public static Quaternion operator +(Quaternion a, Quaternion b) { return new Quaternion(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w); }
		public static Quaternion operator *(Quaternion a, float b) { return new Quaternion(a.x * b, a.y * b, a.z * b, a.w * b); }
		public static Quaternion operator *(float a, Quaternion b) { return new Quaternion(a * b.x, a * b.y, a * b.z, a * b.w); }
		public static Quaternion operator /(Quaternion a, float b) { return new Quaternion(a.x / b, a.y / b, a.z / b, a.w / b); }


		public static Quaternion FromAxisAngle(Vector3 axis, float radians)
		{
			float half = radians * 0.5f;
			float s = MathF.Sin(half);
			float x = axis.x * s;
			float y = axis.y * s;
			float z = axis.z * s;
			float w = MathF.Cos(half);

			return new Quaternion(x, y, z, w);
		}

		public static Quaternion FromEulerAngles(Vector3 eulers)
		{
			return FromAxisAngle(Vector3.UnitY, eulers.y) * FromAxisAngle(Vector3.UnitX, eulers.x) * FromAxisAngle(Vector3.UnitZ, eulers.z);
		}

		public static Quaternion LookAt(Vector3 forward)
		{
			/*
			Vector3 forward = (at - eye).normalized;
			Vector3 right = Vector3.Cross(forward, up).normalized;
			Vector3 up2 = Vector3.Cross(right, forward);

			Matrix rotation = Matrix.Identity;
			rotation.column0 = new Vector4(right, rotation.column0.w);
			rotation.column1 = new Vector4(up2, rotation.column1.w);
			rotation.column2 = new Vector4(-forward, rotation.column2.w);

			return rotation.rotation;
			*/

			///*
			if (forward == Vector3.Zero)
				return Identity;

			forward = forward.normalized;

			float d = Vector3.Dot(Vector3.Forward, forward);

			if (MathF.Abs(d - -1.0f) < 0.000001f)
				return FromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), MathF.PI);
			if (MathF.Abs(d - 1.0f) < 0.000001f)
				return Identity;

			float angle = MathF.Acos(d);
			Vector3 axis = Vector3.Cross(Vector3.Forward, forward).normalized;
			Quaternion q = FromAxisAngle(axis, angle);

			return q;
			//*/
		}

		public static Quaternion LookAt(Vector3 eye, Vector3 at)
		{
			Vector3 forward = (at - eye).normalized;
			return LookAt(forward);
		}
	}
}
