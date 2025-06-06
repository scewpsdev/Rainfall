using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Rainfall
{
	public enum BackbufferRatio
	{
		Equal,     //!< Equal to backbuffer.
		Half,      //!< One half size of backbuffer.
		Quarter,   //!< One quarter size of backbuffer.
		Eighth,    //!< One eighth size of backbuffer.
		Sixteenth, //!< One sixteenth size of backbuffer.
		Double,    //!< Double size of backbuffer.

		Count
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct RenderTargetAttachment
	{
		public int width, height;
		public BackbufferRatio ratio;
		public TextureFormat format;
		public ulong flags;

		ushort _texture;
		int _textureLayer;
		byte _isCubemap;

		byte _generateMipmaps;


		public RenderTargetAttachment(BackbufferRatio ratio = BackbufferRatio.Equal, TextureFormat format = TextureFormat.RGBA8, ulong flags = 0, bool generateMipmaps = false)
		{
			this.width = this.height = 0;
			this.ratio = ratio;
			this.format = format;
			this.flags = flags;
			this._texture = ushort.MaxValue;
			this._generateMipmaps = generateMipmaps ? (byte)1 : (byte)0;
		}

		public RenderTargetAttachment(int width, int height, TextureFormat format = TextureFormat.RGBA8, ulong flags = 0, bool generateMipmaps = false)
		{
			this.width = width;
			this.height = height;
			this.ratio = BackbufferRatio.Count;
			this.format = format;
			this.flags = flags;
			this._texture = ushort.MaxValue;
			this._generateMipmaps = generateMipmaps ? (byte)1 : (byte)0;
		}

		public RenderTargetAttachment(Texture texture, bool generateMipmaps = false)
		{
			this.width = texture.info.width;
			this.height = texture.info.height;
			this.ratio = BackbufferRatio.Count;
			this.format = texture.info.format;
			this._texture = texture.handle;
			this._generateMipmaps = generateMipmaps ? (byte)1 : (byte)0;

			Debug.Assert(texture.info.numMips > 1 == generateMipmaps);
		}

		public RenderTargetAttachment(Cubemap cubemap, int idx, bool generateMipmaps = false)
		{
			this.width = cubemap.info.width;
			this.height = cubemap.info.height;
			this.ratio = BackbufferRatio.Count;
			this.format = cubemap.info.format;
			this._texture = cubemap.handle;
			this._textureLayer = idx;
			this._isCubemap = 1;
			this._generateMipmaps = generateMipmaps ? (byte)1 : (byte)0;

			Debug.Assert(cubemap.info.numMips > 1 == generateMipmaps);
		}

		public bool generateMipmaps
		{
			get => _generateMipmaps != 0;
			set => _generateMipmaps = value ? (byte)1 : (byte)0;
		}
	}

	public class RenderTarget
	{
		internal ushort handle;
		internal RenderTargetAttachment[] attachments;
		internal Texture[] textures;

		public bool hasRGB { get; private set; }
		public bool hasDepth { get; private set; }


		internal RenderTarget(ushort handle, RenderTargetAttachment[] attachments, Texture[] textures, bool hasRGB, bool hasDepth)
		{
			this.handle = handle;
			this.attachments = attachments;
			this.textures = textures;
			this.hasRGB = hasRGB;
			this.hasDepth = hasDepth;
		}

		public int attachmentCount
		{
			get { return textures.Length; }
		}

		public BackbufferRatio ratio
		{
			get => attachments[0].ratio;
		}

		public int width
		{
			get => attachments[0].width;
		}

		public int height
		{
			get => attachments[0].height;
		}

		public RenderTargetAttachment getAttachment(int idx)
		{
			return attachments[idx];
		}

		public Texture getAttachmentTexture(int idx)
		{
			if (idx < textures.Length)
				return textures[idx];
			return null;
		}
	}
}
