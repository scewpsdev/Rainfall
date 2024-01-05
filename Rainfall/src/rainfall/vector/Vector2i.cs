using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public struct Vector2i
	{
		public static readonly Vector2i Zero = new Vector2i(0, 0);
		public static readonly Vector2i One = new Vector2i(1, 1);
		public static readonly Vector2i UnitX = new Vector2i(1, 0);
		public static readonly Vector2i UnitY = new Vector2i(0, 1);
		public static readonly Vector2i Left = new Vector2i(-1, 0);
		public static readonly Vector2i Right = new Vector2i(1, 0);
		public static readonly Vector2i Up = new Vector2i(0, 1);
		public static readonly Vector2i Down = new Vector2i(0, -1);


		public int x, y;


		public Vector2i(int i)
		{
			this.x = i;
			this.y = i;
		}

		public Vector2i(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public override string ToString()
		{
			return x.ToString(CultureInfo.InvariantCulture) + "," + y.ToString(CultureInfo.InvariantCulture);
		}

		public override bool Equals(object obj)
		{
			if (obj is Vector2i)
				return this == (Vector2i)obj;
			return false;
		}

		public override int GetHashCode()
		{
			int hash = x.GetHashCode();
			hash = hash * 19 + y.GetHashCode();
			return hash;
		}

		public static Vector2i operator +(Vector2i a, Vector2i b) => new Vector2i(a.x + b.x, a.y + b.y);
		public static Vector2i operator -(Vector2i a, Vector2i b) => new Vector2i(a.x - b.x, a.y - b.y);
		public static Vector2i operator *(Vector2i a, Vector2i b) => new Vector2i(a.x * b.x, a.y * b.y);
		public static Vector2i operator /(Vector2i a, Vector2i b) => new Vector2i(a.x / b.x, a.y / b.y);

		public static Vector2i operator +(Vector2i a, int b) => new Vector2i(a.x + b, a.y + b);
		public static Vector2i operator -(Vector2i a, int b) => new Vector2i(a.x - b, a.y - b);
		public static Vector2i operator *(Vector2i a, int b) => new Vector2i(a.x * b, a.y * b);
		public static Vector2i operator /(Vector2i a, int b) => new Vector2i(a.x / b, a.y / b);

		public static Vector2 operator +(Vector2i a, float b) => new Vector2(a.x + b, a.y + b);
		public static Vector2 operator -(Vector2i a, float b) => new Vector2(a.x - b, a.y - b);
		public static Vector2 operator *(Vector2i a, float b) => new Vector2(a.x * b, a.y * b);
		public static Vector2 operator /(Vector2i a, float b) => new Vector2(a.x / b, a.y / b);

		public static bool operator ==(Vector2i a, Vector2i b) => a.x == b.x && a.y == b.y;
		public static bool operator !=(Vector2i a, Vector2i b) => a.x != b.x || a.y != b.y;

		public static explicit operator Vector2(Vector2i v) => new Vector2(v.x, v.y);
	}
}
