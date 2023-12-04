using Rainfall.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public enum VertexAttribute : int
	{
		Position,  //!< a_position
		Normal,    //!< a_normal
		Tangent,   //!< a_tangent
		Bitangent, //!< a_bitangent
		Color0,    //!< a_color0
		Color1,    //!< a_color1
		Color2,    //!< a_color2
		Color3,    //!< a_color3
		Indices,   //!< a_indices
		Weight,    //!< a_weight
		TexCoord0, //!< a_texcoord0
		TexCoord1, //!< a_texcoord1
		TexCoord2, //!< a_texcoord2
		TexCoord3, //!< a_texcoord3
		TexCoord4, //!< a_texcoord4
		TexCoord5, //!< a_texcoord5
		TexCoord6, //!< a_texcoord6
		TexCoord7, //!< a_texcoord7

		Count
	}

	public enum VertexAttributeType : int
	{
		Byte4,
		Half,
		Single,
		Vector2,
		Vector3,
		Vector4,

		Count
	}

	public struct VertexElement
	{
		public VertexAttribute attribute;
		public VertexAttributeType type;
		public bool normalized;

		public VertexElement(VertexAttribute attribute, VertexAttributeType type, bool normalized)
		{
			this.attribute = attribute;
			this.type = type;
			this.normalized = normalized;
		}
	}

	public class VertexBuffer
	{
		public readonly ushort handle;

		internal VertexBuffer(ushort handle)
		{
			this.handle = handle;
		}
	}

	public class DynamicVertexBuffer
	{
		internal ushort handle;

		internal DynamicVertexBuffer(ushort handle)
		{
			this.handle = handle;
		}
	}

	public class TransientVertexBuffer
	{
		internal TransientVertexBufferData data;

		internal TransientVertexBuffer(TransientVertexBufferData data)
		{
			this.data = data;
		}
	}
}
