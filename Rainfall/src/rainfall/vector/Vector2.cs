using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector2
	{
		public static readonly Vector2 Zero = new Vector2(0);
		public static readonly Vector2 One = new Vector2(1);
		public static readonly Vector2 UnitX = new Vector2(1, 0);
		public static readonly Vector2 UnitY = new Vector2(0, 1);


		public float x, y;

		public Vector2(float f)
		{
			this.x = f;
			this.y = f;
		}

		public Vector2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public Vector2 yx
		{
			get { return new Vector2(y, x); }
			set
			{
				y = value.x;
				x = value.y;
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

		public float lengthSquared
		{
			get { return x * x + y * y; }
		}

		public float length
		{
			get { return MathF.Sqrt(x * x + y * y); }
		}

		public Vector2 normalized
		{
			get
			{
				if (x * x + y * y > 0.0f)
				{
					float l = 1.0f / length;
					return new Vector2(x * l, y * l);
				}
				return this;
			}
		}

		public float angle
		{
			get { return MathF.Atan2(y, x); }
		}

		public override string ToString()
		{
			return x + "," + y;
		}

		public static float Dot(Vector2 a, Vector2 b)
		{
			return a.x * b.x + a.y * b.y;
		}

		public static Vector2 Abs(Vector2 v)
		{
			return new Vector2(MathF.Abs(v.x), MathF.Abs(v.y));
		}

		public static Vector2 Min(Vector2 a, Vector2 b)
		{
			return new Vector2(MathF.Min(a.x, b.x), MathF.Min(a.y, b.y));
		}

		public static Vector2 Max(Vector2 a, Vector2 b)
		{
			return new Vector2(MathF.Max(a.x, b.x), MathF.Max(a.y, b.y));
		}

		public static Vector2 Rotate(Vector2 v, float angle)
		{
			float s = MathF.Sin(angle);
			float c = MathF.Cos(angle);
			float x = c * v.x - s * v.y;
			float y = s * v.x + c * v.y;
			return new Vector2(x, y);
		}

		public static Vector2 operator +(Vector2 a, Vector2 b) { return new Vector2(a.x + b.x, a.y + b.y); }
		public static Vector2 operator -(Vector2 a, Vector2 b) { return new Vector2(a.x - b.x, a.y - b.y); }
		public static Vector2 operator *(Vector2 a, Vector2 b) { return new Vector2(a.x * b.x, a.y * b.y); }
		public static Vector2 operator /(Vector2 a, Vector2 b) { return new Vector2(a.x / b.x, a.y / b.y); }

		public static Vector2 operator *(Vector2 a, float b) { return new Vector2(a.x * b, a.y * b); }
		public static Vector2 operator /(Vector2 a, float b) { return new Vector2(a.x / b, a.y / b); }
		public static Vector2 operator *(float a, Vector2 b) { return new Vector2(a * b.x, a * b.y); }
		public static Vector2 operator /(float a, Vector2 b) { return new Vector2(a / b.x, a / b.y); }

		public static Vector2 operator +(Vector2 a, float b) { return new Vector2(a.x + b, a.y + b); }
		public static Vector2 operator -(Vector2 a, float b) { return new Vector2(a.x - b, a.y - b); }

		public static Vector2 operator +(Vector2i a, Vector2 b) { return new Vector2(a.x + b.x, a.y + b.y); }
		public static Vector2 operator -(Vector2i a, Vector2 b) { return new Vector2(a.x - b.x, a.y - b.y); }
		public static Vector2 operator *(Vector2i a, Vector2 b) { return new Vector2(a.x * b.x, a.y * b.y); }
		public static Vector2 operator /(Vector2i a, Vector2 b) { return new Vector2(a.x / b.x, a.y / b.y); }

		public static Vector2 operator +(Vector2 a, Vector2i b) { return new Vector2(a.x + b.x, a.y + b.y); }
		public static Vector2 operator -(Vector2 a, Vector2i b) { return new Vector2(a.x - b.x, a.y - b.y); }
		public static Vector2 operator *(Vector2 a, Vector2i b) { return new Vector2(a.x * b.x, a.y * b.y); }
		public static Vector2 operator /(Vector2 a, Vector2i b) { return new Vector2(a.x / b.x, a.y / b.y); }

		public static explicit operator Vector2i(Vector2 v) => new Vector2i((int)v.x, (int)v.y);
	}
}
