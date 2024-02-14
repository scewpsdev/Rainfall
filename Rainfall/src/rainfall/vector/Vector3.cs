using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Rainfall
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector3
	{
		public static readonly Vector3 Zero = new Vector3(0, 0, 0);
		public static readonly Vector3 One = new Vector3(1, 1, 1);
		public static readonly Vector3 Left = new Vector3(-1, 0, 0);
		public static readonly Vector3 Right = new Vector3(1, 0, 0);
		public static readonly Vector3 Down = new Vector3(0, -1, 0);
		public static readonly Vector3 Up = new Vector3(0, 1, 0);
		public static readonly Vector3 Forward = new Vector3(0, 0, -1);
		public static readonly Vector3 Back = new Vector3(0, 0, 1);
		public static readonly Vector3 UnitX = new Vector3(1, 0, 0);
		public static readonly Vector3 UnitY = new Vector3(0, 1, 0);
		public static readonly Vector3 UnitZ = new Vector3(0, 0, 1);


		public float x, y, z;


		public Vector3()
		{
		}

		public Vector3(float f)
		{
			this.x = f;
			this.y = f;
			this.z = f;
		}

		public Vector3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Vector3(Vector2 xy, float z)
		{
			this.x = xy.x;
			this.y = xy.y;
			this.z = z;
		}

		public Vector3(float x, Vector2 yz)
		{
			this.x = x;
			this.y = yz.x;
			this.z = yz.y;
		}

		public Vector2 xy
		{
			get { return new Vector2(x, y); }
			set
			{
				x = value.x;
				y = value.y;
			}
		}

		public Vector2 yz
		{
			get { return new Vector2(y, z); }
			set
			{
				y = value.x;
				z = value.y;
			}
		}

		public Vector2 xz
		{
			get { return new Vector2(x, z); }
			set
			{
				x = value.x;
				z = value.y;
			}
		}

		public Vector2 xx
		{
			get { return new Vector2(x, x); }
		}

		public Vector2 yy
		{
			get { return new Vector2(y, y); }
		}

		public Vector2 zz
		{
			get { return new Vector2(z, z); }
		}

		public Vector3 xzy
		{
			get { return new Vector3(x, z, y); }
			set
			{
				x = value.x;
				z = value.y;
				y = value.z;
			}
		}

		public Vector3 yxz
		{
			get { return new Vector3(y, x, z); }
			set
			{
				y = value.x;
				x = value.y;
				z = value.z;
			}
		}

		public Vector3 yzx
		{
			get { return new Vector3(y, z, x); }
			set
			{
				y = value.x;
				z = value.y;
				x = value.z;
			}
		}

		public Vector3 zxy
		{
			get { return new Vector3(z, x, y); }
			set
			{
				z = value.x;
				x = value.y;
				y = value.z;
			}
		}

		public Vector3 zyx
		{
			get { return new Vector3(z, y, x); }
			set
			{
				z = value.x;
				y = value.y;
				x = value.z;
			}
		}

		public Vector3 xxx
		{
			get { return new Vector3(x, x, x); }
		}

		public Vector3 yyy
		{
			get { return new Vector3(y, y, y); }
		}

		public Vector3 zzz
		{
			get { return new Vector3(z, z, z); }
		}

		public float lengthSquared
		{
			get { return x * x + y * y + z * z; }
		}

		public float length
		{
			get { return MathF.Sqrt(x * x + y * y + z * z); }
		}

		public Vector3 normalized
		{
			get
			{
				if (x * x + y * y + z * z > 0.0f)
				{
					float l = 1.0f / MathF.Sqrt(x * x + y * y + z * z);
					return new Vector3(x * l, y * l, z * l);
				}
				return this;
			}
		}

		public override string ToString()
		{
			return x.ToString(CultureInfo.InvariantCulture) + "," + y.ToString(CultureInfo.InvariantCulture) + "," + z.ToString(CultureInfo.InvariantCulture);
		}

		public override bool Equals(object obj)
		{
			return obj is Vector3 vector && this == vector;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static float Dot(Vector3 a, Vector3 b)
		{
			return a.x * b.x + a.y * b.y + a.z * b.z;
		}

		public static Vector3 Cross(Vector3 a, Vector3 b)
		{
			float x = a.y * b.z - a.z * b.y;
			float y = a.z * b.x - a.x * b.z;
			float z = a.x * b.y - a.y * b.x;
			return new Vector3(x, y, z);
		}

		public static float Distance(Vector3 a, Vector3 b)
		{
			float dx = a.x - b.x;
			float dy = a.y - b.y;
			float dz = a.z - b.z;
			return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
		}

		public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
		{
			return new Vector3(
				(1.0f - t) * a.x + t * b.x,
				(1.0f - t) * a.y + t * b.y,
				(1.0f - t) * a.z + t * b.z
			);
		}

		public static Vector3 Abs(Vector3 v)
		{
			return new Vector3(MathF.Abs(v.x), MathF.Abs(v.y), MathF.Abs(v.z));
		}

		public static Vector3 Min(Vector3 a, Vector3 b)
		{
			return new Vector3(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));
		}

		public static Vector3 Max(Vector3 a, Vector3 b)
		{
			return new Vector3(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));
		}

		public static Vector3 Floor(Vector3 v)
		{
			return new Vector3(MathF.Floor(v.x), MathF.Floor(v.y), MathF.Floor(v.z));
		}

		public static Vector3 Ceil(Vector3 v)
		{
			return new Vector3(MathF.Ceiling(v.x), MathF.Ceiling(v.y), MathF.Ceiling(v.z));
		}

		public static Vector3 Round(Vector3 v)
		{
			return new Vector3(MathF.Round(v.x), MathF.Round(v.y), MathF.Round(v.z));
		}

		public static Vector3 operator -(Vector3 v) { return new Vector3(-v.x, -v.y, -v.z); }

		public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); }
		public static Vector3 operator -(Vector3 a, Vector3 b) { return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); }
		public static Vector3 operator *(Vector3 a, Vector3 b) { return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z); }
		public static Vector3 operator /(Vector3 a, Vector3 b) { return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z); }

		public static Vector3 operator *(Vector3 a, float b) { return new Vector3(a.x * b, a.y * b, a.z * b); }
		public static Vector3 operator /(Vector3 a, float b) { return new Vector3(a.x / b, a.y / b, a.z / b); }
		public static Vector3 operator *(float a, Vector3 b) { return new Vector3(a * b.x, a * b.y, a * b.z); }
		public static Vector3 operator /(float a, Vector3 b) { return new Vector3(a / b.x, a / b.y, a / b.z); }

		public static Vector3 operator +(Vector3 a, float b) { return new Vector3(a.x + b, a.y + b, a.z + b); }
		public static Vector3 operator -(Vector3 a, float b) { return new Vector3(a.x - b, a.y - b, a.z - b); }
		public static Vector3 operator +(float a, Vector3 b) { return new Vector3(a + b.x, a + b.y, a + b.z); }
		public static Vector3 operator -(float a, Vector3 b) { return new Vector3(a - b.x, a - b.y, a - b.z); }

		public static bool operator ==(Vector3 a, Vector3 b) { return a.x == b.x && a.y == b.y && a.z == b.z; }
		public static bool operator !=(Vector3 a, Vector3 b) { return a.x != b.x || a.y != b.y || a.z != b.z; }

		public static explicit operator Vector3i(Vector3 v) => new Vector3i((int)v.x, (int)v.y, (int)v.z);
	}
}
