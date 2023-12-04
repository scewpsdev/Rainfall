using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public enum UniformType
	{
		Sampler, //!< Sampler.
		End,     //!< Reserved, do not use.

		Vector4,    //!< 4 floats vector.
		Matrix3,    //!< 3x3 matrix.
		Matrix4,    //!< 4x4 matrix.

		Count
	}

	public class Shader
	{
		internal IntPtr handle;

		internal Shader(IntPtr handle)
		{
			this.handle = handle;
		}

		public ushort getUniform(Span<byte> name, UniformType type, int num = 1)
		{
			unsafe
			{
				fixed (byte* data = name)
					return Native.Graphics.Graphics_ShaderGetUniform(handle, data, type, num);
			}
		}

		public ushort getUniform(string name, UniformType type, int num = 1)
		{
			return Native.Graphics.Graphics_ShaderGetUniform(handle, name, type, num);
		}
	}
}
