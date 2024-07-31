using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public struct Vector3i
	{
		public static readonly Vector3i Zero = new Vector3i(0, 0, 0);
		public static readonly Vector3i UnitX = new Vector3i(1, 0, 0);
		public static readonly Vector3i UnitY = new Vector3i(0, 1, 0);
		public static readonly Vector3i UnitZ = new Vector3i(0, 0, 1);
		public static readonly Vector3i Left = new Vector3i(-1, 0, 0);
		public static readonly Vector3i Right = new Vector3i(1, 0, 0);
		public static readonly Vector3i Up = new Vector3i(0, 1, 0);
		public static readonly Vector3i Down = new Vector3i(0, -1, 0);
		public static readonly Vector3i Forward = new Vector3i(0, 0, -1);
		public static readonly Vector3i Back = new Vector3i(0, 0, 1);


		public int x, y, z;


		public Vector3i(int i)
		{
			this.x = i;
			this.y = i;
			this.z = i;
		}

		public Vector3i(int x, int y, int z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Vector2i xy
		{
			get => new Vector2i(x, y);
		}

		public override string ToString()
		{
			return x.ToString(CultureInfo.InvariantCulture) + "," + y.ToString(CultureInfo.InvariantCulture) + "," + z.ToString(CultureInfo.InvariantCulture);
		}

		public override bool Equals(object obj)
		{
			if (obj is Vector3i)
				return this == (Vector3i)obj;
			return false;
		}

		public override int GetHashCode()
		{
			int hash = x.GetHashCode();
			hash = hash * 19 + y.GetHashCode();
			hash = hash * 19 + z.GetHashCode();
			return hash;
		}

		public static Vector3i Min(Vector3i a, Vector3i b)
		{
			return new Vector3i(Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));
		}

		public static Vector3i Max(Vector3i a, Vector3i b)
		{
			return new Vector3i(Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));
		}

		public static Vector3i operator +(Vector3i a, Vector3i b) => new Vector3i(a.x + b.x, a.y + b.y, a.z + b.z);
		public static Vector3i operator -(Vector3i a, Vector3i b) => new Vector3i(a.x - b.x, a.y - b.y, a.z - b.z);
		public static Vector3i operator *(Vector3i a, Vector3i b) => new Vector3i(a.x * b.x, a.y * b.y, a.z * b.z);
		public static Vector3i operator /(Vector3i a, Vector3i b) => new Vector3i(a.x / b.x, a.y / b.y, a.z / b.z);

		public static Vector3 operator +(Vector3i a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
		public static Vector3 operator -(Vector3i a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
		public static Vector3 operator *(Vector3i a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
		public static Vector3 operator /(Vector3i a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);

		public static Vector3i operator +(Vector3i a, int b) => new Vector3i(a.x + b, a.y + b, a.z + b);
		public static Vector3i operator -(Vector3i a, int b) => new Vector3i(a.x - b, a.y - b, a.z - b);
		public static Vector3i operator *(Vector3i a, int b) => new Vector3i(a.x * b, a.y * b, a.z * b);
		public static Vector3i operator /(Vector3i a, int b) => new Vector3i(a.x / b, a.y / b, a.z / b);

		public static Vector3i operator +(int a, Vector3i b) => new Vector3i(a + b.x, a + b.y, a + b.y);
		public static Vector3i operator -(int a, Vector3i b) => new Vector3i(a - b.x, a - b.y, a - b.y);
		public static Vector3i operator *(int a, Vector3i b) => new Vector3i(a * b.x, a * b.y, a * b.y);
		public static Vector3i operator /(int a, Vector3i b) => new Vector3i(a / b.x, a / b.y, a / b.y);

		public static Vector3 operator +(Vector3i a, float b) => new Vector3(a.x + b, a.y + b, a.z + b);
		public static Vector3 operator -(Vector3i a, float b) => new Vector3(a.x - b, a.y - b, a.z - b);
		public static Vector3 operator *(Vector3i a, float b) => new Vector3(a.x * b, a.y * b, a.z * b);
		public static Vector3 operator /(Vector3i a, float b) => new Vector3(a.x / b, a.y / b, a.z / b);

		public static Vector3 operator +(float a, Vector3i b) => new Vector3(a + b.x, a + b.y, a + b.z);
		public static Vector3 operator -(float a, Vector3i b) => new Vector3(a - b.x, a - b.y, a - b.z);
		public static Vector3 operator *(float a, Vector3i b) => new Vector3(a * b.x, a * b.y, a * b.z);
		public static Vector3 operator /(float a, Vector3i b) => new Vector3(a / b.x, a / b.y, a / b.z);

		public static bool operator >(Vector3i a, Vector3i b) => a.x > b.x && a.y > b.y && a.z > b.z;
		public static bool operator <(Vector3i a, Vector3i b) => a.x < b.x && a.y < b.y && a.z < b.z;
		public static bool operator >=(Vector3i a, Vector3i b) => a.x >= b.x && a.y >= b.y && a.z >= b.z;
		public static bool operator <=(Vector3i a, Vector3i b) => a.x <= b.x && a.y <= b.y && a.z <= b.z;

		public static Vector3i operator -(Vector3i v) { return new Vector3i(-v.x, -v.y, -v.z); }

		public static bool operator ==(Vector3i a, Vector3i b) => a.x == b.x && a.y == b.y && a.z == b.z;
		public static bool operator !=(Vector3i a, Vector3i b) => a.x != b.x || a.y != b.y || a.z != b.z;

		public static explicit operator Vector3(Vector3i v) => new Vector3(v.x, v.y, v.z);
	}
}
