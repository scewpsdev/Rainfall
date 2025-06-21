using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector4 : System.Numerics.IMultiplyOperators<Vector4, float, Vector4>, System.Numerics.IAdditionOperators<Vector4, Vector4, Vector4>
	{
		public static readonly Vector4 Zero = new Vector4(0, 0, 0, 0);
		public static readonly Vector4 One = new Vector4(1, 1, 1, 1);


		public float x, y, z, w;


		public Vector4(float f)
		{
			this.x = f;
			this.y = f;
			this.z = f;
			this.w = f;
		}

		public Vector4(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public Vector4(Vector3 xyz, float w)
		{
			this.x = xyz.x;
			this.y = xyz.y;
			this.z = xyz.z;
			this.w = w;
		}

		public Vector4(float x, Vector3 yzw)
		{
			this.x = x;
			this.y = yzw.x;
			this.z = yzw.y;
			this.w = yzw.z;
		}

		public Vector4(Vector2 xy, float z, float w)
		{
			this.x = xy.x;
			this.y = xy.y;
			this.z = z;
			this.w = w;
		}

		public Vector4(float x, Vector2 yz, float w)
		{
			this.x = x;
			this.y = yz.x;
			this.z = yz.y;
			this.w = w;
		}

		public Vector4(float x, float y, Vector2 zw)
		{
			this.x = x;
			this.y = y;
			this.z = zw.x;
			this.w = zw.y;
		}

		public Vector4(Vector2 xy, Vector2 zw)
		{
			this.x = xy.x;
			this.y = xy.y;
			this.z = zw.x;
			this.w = zw.y;
		}

		public Vector3 xyz
		{
			get { return new Vector3(x, y, z); }
			set
			{
				x = value.x;
				y = value.y;
				z = value.z;
			}
		}

		public override string ToString()
		{
			return "{" + x.ToString(CultureInfo.InvariantCulture) + "," + y.ToString(CultureInfo.InvariantCulture) + "," + z.ToString(CultureInfo.InvariantCulture) + "," + w.ToString(CultureInfo.InvariantCulture) + "}";
		}

		public static Vector4 Lerp(Vector4 a, Vector4 b, float t)
		{
			return new Vector4(
				(1.0f - t) * a.x + t * b.x,
				(1.0f - t) * a.y + t * b.y,
				(1.0f - t) * a.z + t * b.z,
				(1.0f - t) * a.w + t * b.w
			);
		}

		public static Vector4 operator *(Vector4 a, float b)
		{
			return new Vector4(a.x * b, a.y * b, a.z * b, a.w * b);
		}

		public static Vector4 operator /(Vector4 a, float b)
		{
			return new Vector4(a.x / b, a.y / b, a.z / b, a.w / b);
		}

		public static Vector4 operator +(Vector4 a, Vector4 b)
		{
			return new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
		}

		public static Vector4 operator *(Vector4 a, Vector4 b)
		{
			return new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
		}

		public static implicit operator Vector4(uint argb)
		{
			return MathHelper.ARGBToVector(argb);
		}
	}
}
