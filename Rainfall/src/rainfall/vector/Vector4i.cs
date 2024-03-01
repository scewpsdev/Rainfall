using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public struct Vector4i
	{
		public static readonly Vector4i Zero = new Vector4i(0, 0, 0, 0);
		public static readonly Vector4i UnitX = new Vector4i(1, 0, 0, 0);
		public static readonly Vector4i UnitY = new Vector4i(0, 1, 0, 0);
		public static readonly Vector4i UnitZ = new Vector4i(0, 0, 1, 0);
		public static readonly Vector4i UnitW = new Vector4i(0, 0, 0, 1);
		public static readonly Vector4i Left = new Vector4i(-1, 0, 0, 0);
		public static readonly Vector4i Right = new Vector4i(1, 0, 0, 0);
		public static readonly Vector4i Up = new Vector4i(0, 1, 0, 0);
		public static readonly Vector4i Down = new Vector4i(0, -1, 0, 0);
		public static readonly Vector4i Forward = new Vector4i(0, 0, -1, 0);
		public static readonly Vector4i Back = new Vector4i(0, 0, 1, 0);


		public int x, y, z, w;


		public Vector4i(int i)
		{
			this.x = i;
			this.y = i;
			this.z = i;
			this.w = i;
		}

		public Vector4i(int x, int y, int z, int w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public override string ToString()
		{
			return x.ToString(CultureInfo.InvariantCulture) + "," + y.ToString(CultureInfo.InvariantCulture) + "," + z.ToString(CultureInfo.InvariantCulture) + "," + w.ToString(CultureInfo.InvariantCulture);
		}

		public override bool Equals(object obj)
		{
			if (obj is Vector4i)
				return this == (Vector4i)obj;
			return false;
		}

		public override int GetHashCode()
		{
			int hash = x.GetHashCode();
			hash = hash * 19 + y.GetHashCode();
			hash = hash * 19 + z.GetHashCode();
			hash = hash * 19 + w.GetHashCode();
			return hash;
		}

		public static Vector4i Min(Vector4i a, Vector4i b)
		{
			return new Vector4i(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z), Math.Min(a.w, b.w));
		}

		public static Vector4i Max(Vector4i a, Vector4i b)
		{
			return new Vector4i(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z), Math.Max(a.w, b.w));
		}

		public static Vector4i operator +(Vector4i a, Vector4i b) => new Vector4i(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
		public static Vector4i operator -(Vector4i a, Vector4i b) => new Vector4i(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
		public static Vector4i operator *(Vector4i a, Vector4i b) => new Vector4i(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
		public static Vector4i operator /(Vector4i a, Vector4i b) => new Vector4i(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);

		public static Vector4 operator +(Vector4i a, Vector4 b) => new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
		public static Vector4 operator -(Vector4i a, Vector4 b) => new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
		public static Vector4 operator *(Vector4i a, Vector4 b) => new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
		public static Vector4 operator /(Vector4i a, Vector4 b) => new Vector4(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);

		public static Vector4i operator +(Vector4i a, int b) => new Vector4i(a.x + b, a.y + b, a.z + b, a.w + b);
		public static Vector4i operator -(Vector4i a, int b) => new Vector4i(a.x - b, a.y - b, a.z - b, a.w - b);
		public static Vector4i operator *(Vector4i a, int b) => new Vector4i(a.x * b, a.y * b, a.z * b, a.w * b);
		public static Vector4i operator /(Vector4i a, int b) => new Vector4i(a.x / b, a.y / b, a.z / b, a.w / b);

		public static Vector4i operator +(int a, Vector4i b) => new Vector4i(a + b.x, a + b.y, a + b.y, a + b.w);
		public static Vector4i operator -(int a, Vector4i b) => new Vector4i(a - b.x, a - b.y, a - b.y, a - b.w);
		public static Vector4i operator *(int a, Vector4i b) => new Vector4i(a * b.x, a * b.y, a * b.y, a * b.w);
		public static Vector4i operator /(int a, Vector4i b) => new Vector4i(a / b.x, a / b.y, a / b.y, a / b.w);

		public static Vector4 operator +(Vector4i a, float b) => new Vector4(a.x + b, a.y + b, a.z + b, a.w + b);
		public static Vector4 operator -(Vector4i a, float b) => new Vector4(a.x - b, a.y - b, a.z - b, a.w - b);
		public static Vector4 operator *(Vector4i a, float b) => new Vector4(a.x * b, a.y * b, a.z * b, a.w * b);
		public static Vector4 operator /(Vector4i a, float b) => new Vector4(a.x / b, a.y / b, a.z / b, a.w / b);

		public static Vector4 operator +(float a, Vector4i b) => new Vector4(a + b.x, a + b.y, a + b.z, a + b.w);
		public static Vector4 operator -(float a, Vector4i b) => new Vector4(a - b.x, a - b.y, a - b.z, a - b.w);
		public static Vector4 operator *(float a, Vector4i b) => new Vector4(a * b.x, a * b.y, a * b.z, a * b.w);
		public static Vector4 operator /(float a, Vector4i b) => new Vector4(a / b.x, a / b.y, a / b.z, a / b.w);

		public static bool operator >(Vector4i a, Vector4i b) => a.x > b.x && a.y > b.y && a.z > b.z && a.w > b.w;
		public static bool operator <(Vector4i a, Vector4i b) => a.x < b.x && a.y < b.y && a.z < b.z && a.w < b.w;
		public static bool operator >=(Vector4i a, Vector4i b) => a.x >= b.x && a.y >= b.y && a.z >= b.z && a.w >= b.w;
		public static bool operator <=(Vector4i a, Vector4i b) => a.x <= b.x && a.y <= b.y && a.z <= b.z && a.w <= b.w;

		public static Vector4i operator -(Vector4i v) { return new Vector4i(-v.x, -v.y, -v.z, -v.w); }

		public static bool operator ==(Vector4i a, Vector4i b) => a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
		public static bool operator !=(Vector4i a, Vector4i b) => a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w;

		public static explicit operator Vector4(Vector4i v) => new Vector4(v.x, v.y, v.z, v.w);
	}
}
