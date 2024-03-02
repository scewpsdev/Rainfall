using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Matrix
	{
		public static readonly Matrix Identity = new Matrix(1.0f);


		public float m00, m01, m02, m03;
		public float m10, m11, m12, m13;
		public float m20, m21, m22, m23;
		public float m30, m31, m32, m33;


		public Matrix(float diagonal)
		{
			m00 = diagonal;
			m01 = 0.0f;
			m02 = 0.0f;
			m03 = 0.0f;

			m10 = 0.0f;
			m11 = diagonal;
			m12 = 0.0f;
			m13 = 0.0f;

			m20 = 0.0f;
			m21 = 0.0f;
			m22 = diagonal;
			m23 = 0.0f;

			m30 = 0.0f;
			m31 = 0.0f;
			m32 = 0.0f;
			m33 = diagonal;
		}

		public Matrix(
			float m00, float m01, float m02, float m03,
			float m10, float m11, float m12, float m13,
			float m20, float m21, float m22, float m23,
			float m30, float m31, float m32, float m33
		)
		{
			this.m00 = m00;
			this.m01 = m01;
			this.m02 = m02;
			this.m03 = m03;

			this.m10 = m10;
			this.m11 = m11;
			this.m12 = m12;
			this.m13 = m13;

			this.m20 = m20;
			this.m21 = m21;
			this.m22 = m22;
			this.m23 = m23;

			this.m30 = m30;
			this.m31 = m31;
			this.m32 = m32;
			this.m33 = m33;
		}

		public override bool Equals(object obj)
		{
			if (obj is Matrix)
				return ((Matrix)obj) == this;
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public Vector4 column0
		{
			get { return new Vector4(m00, m01, m02, m03); }
			set
			{
				m00 = value.x;
				m01 = value.y;
				m02 = value.z;
				m03 = value.w;
			}
		}

		public Vector4 column1
		{
			get { return new Vector4(m10, m11, m12, m13); }
			set
			{
				m10 = value.x;
				m11 = value.y;
				m12 = value.z;
				m13 = value.w;
			}
		}

		public Vector4 column2
		{
			get { return new Vector4(m20, m21, m22, m23); }
			set
			{
				m20 = value.x;
				m21 = value.y;
				m22 = value.z;
				m23 = value.w;
			}
		}

		public Vector4 column3
		{
			get { return new Vector4(m30, m31, m32, m33); }
			set
			{
				m30 = value.x;
				m31 = value.y;
				m32 = value.z;
				m33 = value.w;
			}
		}

		public Vector3 translation
		{
			get { return new Vector3(m30, m31, m32); }
		}

		public Quaternion rotation
		{
			get
			{
				Vector3 s = scale;

				float c00 = m00 / s.x;
				float c11 = m11 / s.y;
				float c22 = m22 / s.z;

				float qw = MathF.Sqrt(MathF.Max(0.0f, 1.0f + c00 + c11 + c22)) / 2.0f;
				float qx = MathF.Sqrt(MathF.Max(0.0f, 1.0f + c00 - c11 - c22)) / 2.0f;
				float qy = MathF.Sqrt(MathF.Max(0.0f, 1.0f - c00 + c11 - c22)) / 2.0f;
				float qz = MathF.Sqrt(MathF.Max(0.0f, 1.0f - c00 - c11 + c22)) / 2.0f;

				qx = MathF.CopySign(qx, m12 - m21);
				qy = MathF.CopySign(qy, m20 - m02);
				qz = MathF.CopySign(qz, m01 - m10);

				return new Quaternion(qx, qy, qz, qw).normalized;
			}
		}

		public Vector3 scale
		{
			get
			{
				float x = MathF.Sqrt(m00 * m00 + m01 * m01 + m02 * m02);
				float y = MathF.Sqrt(m10 * m10 + m11 * m11 + m12 * m12);
				float z = MathF.Sqrt(m20 * m20 + m21 * m21 + m22 * m22);
				return new Vector3(x, y, z);
			}
		}

		public void decompose(out Vector3 position, out Quaternion rotation, out Vector3 scale)
		{
			position = new Vector3(m30, m31, m32);

			float sx = MathF.Sqrt(m00 * m00 + m01 * m01 + m02 * m02);
			float sy = MathF.Sqrt(m10 * m10 + m11 * m11 + m12 * m12);
			float sz = MathF.Sqrt(m20 * m20 + m21 * m21 + m22 * m22);
			scale = new Vector3(sx, sy, sz);

			float c00 = m00 / sx;
			float c11 = m11 / sy;
			float c22 = m22 / sz;

			float qw = MathF.Sqrt(MathF.Max(0.0f, 1.0f + c00 + c11 + c22)) / 2.0f;
			float qx = MathF.Sqrt(MathF.Max(0.0f, 1.0f + c00 - c11 - c22)) / 2.0f;
			float qy = MathF.Sqrt(MathF.Max(0.0f, 1.0f - c00 + c11 - c22)) / 2.0f;
			float qz = MathF.Sqrt(MathF.Max(0.0f, 1.0f - c00 - c11 + c22)) / 2.0f;

			qx = MathF.CopySign(qx, m12 - m21);
			qy = MathF.CopySign(qy, m20 - m02);
			qz = MathF.CopySign(qz, m01 - m10);

			rotation = new Quaternion(qx, qy, qz, qw).normalized;
		}

		public float determinant
		{
			get
			{
				return
					m03 * m12 * m21 * m30 - m02 * m13 * m21 * m30 - m03 * m11 * m22 * m30 + m01 * m13 * m22 * m30 +
					m02 * m11 * m23 * m30 - m01 * m12 * m23 * m30 - m03 * m12 * m20 * m31 + m02 * m13 * m20 * m31 +
					m03 * m10 * m22 * m31 - m00 * m13 * m22 * m31 - m02 * m10 * m23 * m31 + m00 * m12 * m23 * m31 +
					m03 * m11 * m20 * m32 - m01 * m13 * m20 * m32 - m03 * m10 * m21 * m32 + m00 * m13 * m21 * m32 +
					m01 * m10 * m23 * m32 - m00 * m11 * m23 * m32 - m02 * m11 * m20 * m33 + m01 * m12 * m20 * m33 +
					m02 * m10 * m21 * m33 - m00 * m12 * m21 * m33 - m01 * m10 * m22 * m33 + m00 * m11 * m22 * m33;
			}
		}

		public Matrix inverted
		{
			get
			{
				Matrix result;
				float f = 1.0f / determinant;

				result.m00 = (m12 * m23 * m31 - m13 * m22 * m31 + m13 * m21 * m32 - m11 * m23 * m32 - m12 * m21 * m33 + m11 * m22 * m33) * f;
				result.m01 = (m03 * m22 * m31 - m02 * m23 * m31 - m03 * m21 * m32 + m01 * m23 * m32 + m02 * m21 * m33 - m01 * m22 * m33) * f;
				result.m02 = (m02 * m13 * m31 - m03 * m12 * m31 + m03 * m11 * m32 - m01 * m13 * m32 - m02 * m11 * m33 + m01 * m12 * m33) * f;
				result.m03 = (m03 * m12 * m21 - m02 * m13 * m21 - m03 * m11 * m22 + m01 * m13 * m22 + m02 * m11 * m23 - m01 * m12 * m23) * f;
				result.m10 = (m13 * m22 * m30 - m12 * m23 * m30 - m13 * m20 * m32 + m10 * m23 * m32 + m12 * m20 * m33 - m10 * m22 * m33) * f;
				result.m11 = (m02 * m23 * m30 - m03 * m22 * m30 + m03 * m20 * m32 - m00 * m23 * m32 - m02 * m20 * m33 + m00 * m22 * m33) * f;
				result.m12 = (m03 * m12 * m30 - m02 * m13 * m30 - m03 * m10 * m32 + m00 * m13 * m32 + m02 * m10 * m33 - m00 * m12 * m33) * f;
				result.m13 = (m02 * m13 * m20 - m03 * m12 * m20 + m03 * m10 * m22 - m00 * m13 * m22 - m02 * m10 * m23 + m00 * m12 * m23) * f;
				result.m20 = (m11 * m23 * m30 - m13 * m21 * m30 + m13 * m20 * m31 - m10 * m23 * m31 - m11 * m20 * m33 + m10 * m21 * m33) * f;
				result.m21 = (m03 * m21 * m30 - m01 * m23 * m30 - m03 * m20 * m31 + m00 * m23 * m31 + m01 * m20 * m33 - m00 * m21 * m33) * f;
				result.m22 = (m01 * m13 * m30 - m03 * m11 * m30 + m03 * m10 * m31 - m00 * m13 * m31 - m01 * m10 * m33 + m00 * m11 * m33) * f;
				result.m23 = (m03 * m11 * m20 - m01 * m13 * m20 - m03 * m10 * m21 + m00 * m13 * m21 + m01 * m10 * m23 - m00 * m11 * m23) * f;
				result.m30 = (m12 * m21 * m30 - m11 * m22 * m30 - m12 * m20 * m31 + m10 * m22 * m31 + m11 * m20 * m32 - m10 * m21 * m32) * f;
				result.m31 = (m01 * m22 * m30 - m02 * m21 * m30 + m02 * m20 * m31 - m00 * m22 * m31 - m01 * m20 * m32 + m00 * m21 * m32) * f;
				result.m32 = (m02 * m11 * m30 - m01 * m12 * m30 - m02 * m10 * m31 + m00 * m12 * m31 + m01 * m10 * m32 - m00 * m11 * m32) * f;
				result.m33 = (m01 * m12 * m20 - m02 * m11 * m20 + m02 * m10 * m21 - m00 * m12 * m21 - m01 * m10 * m22 + m00 * m11 * m22) * f;

				return result;
			}
		}

		public override string ToString()
		{
			return
				"[" + m00 + "," + m10 + "," + m20 + "," + m30 + "\n" +
				" " + m01 + "," + m11 + "," + m21 + "," + m31 + "\n" +
				" " + m02 + "," + m12 + "," + m22 + "," + m32 + "\n" +
				" " + m03 + "," + m13 + "," + m23 + "," + m33 + "]";
		}

		public Vector4 this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return column0;
					case 1: return column1;
					case 2: return column2;
					case 3: return column3;
					default: return Vector4.Zero;
				}
			}
			set
			{
				switch (index)
				{
					case 0: column0 = value; break;
					case 1: column1 = value; break;
					case 2: column2 = value; break;
					case 3: column3 = value; break;
					default: break;
				}
			}
		}


		public static bool operator ==(Matrix a, Matrix b)
		{
			return
				a.m00 == b.m00 &&
				a.m01 == b.m01 &&
				a.m02 == b.m02 &&
				a.m03 == b.m03 &&
				a.m10 == b.m10 &&
				a.m11 == b.m11 &&
				a.m12 == b.m12 &&
				a.m13 == b.m13 &&
				a.m20 == b.m20 &&
				a.m21 == b.m21 &&
				a.m22 == b.m22 &&
				a.m23 == b.m23 &&
				a.m30 == b.m30 &&
				a.m31 == b.m31 &&
				a.m32 == b.m32 &&
				a.m33 == b.m33;
		}

		public static bool operator !=(Matrix a, Matrix b)
		{
			return !(a == b);
		}

		public static Matrix operator *(Matrix a, Matrix b)
		{
			Matrix result;

			result.m00 = a.m00 * b.m00 + a.m10 * b.m01 + a.m20 * b.m02 + a.m30 * b.m03;
			result.m01 = a.m01 * b.m00 + a.m11 * b.m01 + a.m21 * b.m02 + a.m31 * b.m03;
			result.m02 = a.m02 * b.m00 + a.m12 * b.m01 + a.m22 * b.m02 + a.m32 * b.m03;
			result.m03 = a.m03 * b.m00 + a.m13 * b.m01 + a.m23 * b.m02 + a.m33 * b.m03;
			result.m10 = a.m00 * b.m10 + a.m10 * b.m11 + a.m20 * b.m12 + a.m30 * b.m13;
			result.m11 = a.m01 * b.m10 + a.m11 * b.m11 + a.m21 * b.m12 + a.m31 * b.m13;
			result.m12 = a.m02 * b.m10 + a.m12 * b.m11 + a.m22 * b.m12 + a.m32 * b.m13;
			result.m13 = a.m03 * b.m10 + a.m13 * b.m11 + a.m23 * b.m12 + a.m33 * b.m13;
			result.m20 = a.m00 * b.m20 + a.m10 * b.m21 + a.m20 * b.m22 + a.m30 * b.m23;
			result.m21 = a.m01 * b.m20 + a.m11 * b.m21 + a.m21 * b.m22 + a.m31 * b.m23;
			result.m22 = a.m02 * b.m20 + a.m12 * b.m21 + a.m22 * b.m22 + a.m32 * b.m23;
			result.m23 = a.m03 * b.m20 + a.m13 * b.m21 + a.m23 * b.m22 + a.m33 * b.m23;
			result.m30 = a.m00 * b.m30 + a.m10 * b.m31 + a.m20 * b.m32 + a.m30 * b.m33;
			result.m31 = a.m01 * b.m30 + a.m11 * b.m31 + a.m21 * b.m32 + a.m31 * b.m33;
			result.m32 = a.m02 * b.m30 + a.m12 * b.m31 + a.m22 * b.m32 + a.m32 * b.m33;
			result.m33 = a.m03 * b.m30 + a.m13 * b.m31 + a.m23 * b.m32 + a.m33 * b.m33;

			return result;
		}

		public static Vector4 operator *(Matrix a, Vector4 b)
		{
			Vector4 result;

			result.x = a.m00 * b.x + a.m10 * b.y + a.m20 * b.z + a.m30 * b.w;
			result.y = a.m01 * b.x + a.m11 * b.y + a.m21 * b.z + a.m31 * b.w;
			result.z = a.m02 * b.x + a.m12 * b.y + a.m22 * b.z + a.m32 * b.w;
			result.w = a.m03 * b.x + a.m13 * b.y + a.m23 * b.z + a.m33 * b.w;

			return result;
		}

		public static Vector3 operator *(Matrix a, Vector3 b)
		{
			Vector3 result;

			result.x = a.m00 * b.x + a.m10 * b.y + a.m20 * b.z + a.m30;
			result.y = a.m01 * b.x + a.m11 * b.y + a.m21 * b.z + a.m31;
			result.z = a.m02 * b.x + a.m12 * b.y + a.m22 * b.z + a.m32;

			return result;
		}

		public static Quaternion operator *(Matrix a, Quaternion b)
		{
			Matrix bMat = CreateRotation(b);
			Matrix result = a * bMat;

			float sx = MathF.Sqrt(result.m00 * result.m00 + result.m01 * result.m01 + result.m02 * result.m02);
			float sy = MathF.Sqrt(result.m10 * result.m10 + result.m11 * result.m11 + result.m12 * result.m12);
			float sz = MathF.Sqrt(result.m20 * result.m20 + result.m21 * result.m21 + result.m22 * result.m22);

			float c00 = result.m00 / sx;
			float c11 = result.m11 / sy;
			float c22 = result.m22 / sz;

			float qw = MathF.Sqrt(MathF.Max(0.0f, 1.0f + c00 + c11 + c22)) / 2.0f;
			float qx = MathF.Sqrt(MathF.Max(0.0f, 1.0f + c00 - c11 - c22)) / 2.0f;
			float qy = MathF.Sqrt(MathF.Max(0.0f, 1.0f - c00 + c11 - c22)) / 2.0f;
			float qz = MathF.Sqrt(MathF.Max(0.0f, 1.0f - c00 - c11 + c22)) / 2.0f;

			qx = MathF.CopySign(qx, result.m12 - result.m21);
			qy = MathF.CopySign(qy, result.m20 - result.m02);
			qz = MathF.CopySign(qz, result.m01 - result.m10);

			return new Quaternion(qx, qy, qz, qw).normalized;
		}

		public static Matrix CreateTranslation(float x, float y, float z, float w)
		{
			Matrix matrix = Identity;
			matrix.m30 = x;
			matrix.m31 = y;
			matrix.m32 = z;
			matrix.m33 = w;
			return matrix;
		}

		public static Matrix CreateTranslation(Vector4 v)
		{
			Matrix matrix = Identity;
			matrix.m30 = v.x;
			matrix.m31 = v.y;
			matrix.m32 = v.z;
			matrix.m33 = v.w;
			return matrix;
		}

		public static Matrix CreateTranslation(float x, float y, float z)
		{
			Matrix matrix = Identity;
			matrix.m30 = x;
			matrix.m31 = y;
			matrix.m32 = z;
			return matrix;
		}

		public static Matrix CreateTranslation(Vector3 v)
		{
			Matrix matrix = Identity;
			matrix.m30 = v.x;
			matrix.m31 = v.y;
			matrix.m32 = v.z;
			return matrix;
		}

		public static Matrix CreateRotation(Quaternion q)
		{
			Matrix matrix;

			matrix.m00 = 1.0f - 2.0f * q.y * q.y - 2.0f * q.z * q.z;
			matrix.m01 = 2.0f * q.x * q.y + 2.0f * q.z * q.w;
			matrix.m02 = 2.0f * q.x * q.z - 2.0f * q.y * q.w;
			matrix.m03 = 0.0f;

			matrix.m10 = 2.0f * q.x * q.y - 2.0f * q.z * q.w;
			matrix.m11 = 1.0f - 2.0f * q.x * q.x - 2.0f * q.z * q.z;
			matrix.m12 = 2.0f * q.y * q.z + 2.0f * q.x * q.w;
			matrix.m13 = 0.0f;

			matrix.m20 = 2.0f * q.x * q.z + 2.0f * q.y * q.w;
			matrix.m21 = 2.0f * q.y * q.z - 2.0f * q.x * q.w;
			matrix.m22 = 1.0f - 2.0f * q.x * q.x - 2.0f * q.y * q.y;
			matrix.m23 = 0.0f;

			matrix.m30 = 0.0f;
			matrix.m31 = 0.0f;
			matrix.m32 = 0.0f;
			matrix.m33 = 1.0f;

			return matrix;
		}

		public static Matrix CreateRotation(Vector3 axis, float radians)
		{
			float half = radians * 0.5f;
			float s = MathF.Sin(half);
			float x = axis.x * s;
			float y = axis.y * s;
			float z = axis.z * s;
			float w = MathF.Cos(half);

			Matrix matrix;

			matrix.m00 = 1.0f - 2.0f * y * y - 2.0f * z * z;
			matrix.m01 = 2.0f * x * y + 2.0f * z * w;
			matrix.m02 = 2.0f * x * z - 2.0f * y * w;
			matrix.m03 = 0.0f;

			matrix.m10 = 2.0f * x * y - 2.0f * z * w;
			matrix.m11 = 1.0f - 2.0f * x * x - 2.0f * z * z;
			matrix.m12 = 2.0f * y * z + 2.0f * x * w;
			matrix.m13 = 0.0f;

			matrix.m20 = 2.0f * x * z + 2.0f * y * w;
			matrix.m21 = 2.0f * y * z - 2.0f * x * w;
			matrix.m22 = 1.0f - 2.0f * x * x - 2.0f * y * y;
			matrix.m23 = 0.0f;

			matrix.m30 = 0.0f;
			matrix.m31 = 0.0f;
			matrix.m32 = 0.0f;
			matrix.m33 = 1.0f;

			return matrix;
		}

		public static Matrix CreateScale(Vector3 v)
		{
			Matrix matrix = Identity;
			matrix.m00 = v.x;
			matrix.m11 = v.y;
			matrix.m22 = v.z;
			matrix.m33 = 1.0f;
			return matrix;
		}

		public static Matrix CreateScale(float sx, float sy, float sz)
		{
			Matrix matrix = Identity;
			matrix.m00 = sx;
			matrix.m11 = sy;
			matrix.m22 = sz;
			matrix.m33 = 1.0f;
			return matrix;
		}

		public static Matrix CreateScale(float s)
		{
			Matrix matrix = Identity;
			matrix.m00 = s;
			matrix.m11 = s;
			matrix.m22 = s;
			matrix.m33 = 1.0f;
			return matrix;
		}

		public static Matrix CreateTransform(Vector3 position, Quaternion rotation, Vector3 scale)
		{
			return CreateTranslation(position) * CreateRotation(rotation) * CreateScale(scale);
		}

		public static Matrix CreateTransform(Vector3 position, Quaternion rotation)
		{
			return CreateTranslation(position) * CreateRotation(rotation);
		}

		public static Matrix CreatePerspective(float fov, float aspect, float near, float far)
		{
			Matrix matrix = Identity;

			float y = 1.0f / MathF.Tan(0.5f * fov);
			float x = y / aspect;
			float l = far - near;

			matrix.m00 = x;
			matrix.m11 = y;
			matrix.m22 = (far + near) / -l;
			matrix.m23 = -1.0f;
			matrix.m32 = -2.0f * near * far / l;
			matrix.m33 = 0.0f;

			return matrix;
		}

		public static Matrix CreateOrthographic(float left, float right, float bottom, float top, float near, float far)
		{
			Matrix matrix = Identity;

			float width = right - left;
			float height = top - bottom;
			float depth = far - near;

			matrix.m00 = 2.0f / width;
			matrix.m11 = 2.0f / height;
			matrix.m22 = -1.0f / depth; // Convert to 0.0->1.0 instead of -1.0->1.0 (because of DirectX I guess?)

			matrix.m30 = -(right + left) / width;
			matrix.m31 = -(top + bottom) / height;
			matrix.m32 = (far + near) / depth + 0.5f;

			return matrix;
		}

		public static Matrix CreateOrthographic(float width, float height, float near, float far)
		{
			return CreateOrthographic(-0.5f * width, 0.5f * width, -0.5f * height, 0.5f * height, near, far);
		}
	}
}
