using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public enum TextureFormat
	{
		BC1,          //!< DXT1 R5G6B5A1
		BC2,          //!< DXT3 R5G6B5A4
		BC3,          //!< DXT5 R5G6B5A8
		BC4,          //!< LATC1/ATI1 R8
		BC5,          //!< LATC2/ATI2 RG8
		BC6H,         //!< BC6H RGB16F
		BC7,          //!< BC7 RGB 4-7 bits per color channel, 0-8 bits alpha
		ETC1,         //!< ETC1 RGB8
		ETC2,         //!< ETC2 RGB8
		ETC2A,        //!< ETC2 RGBA8
		ETC2A1,       //!< ETC2 RGB8A1
		PTC12,        //!< PVRTC1 RGB 2BPP
		PTC14,        //!< PVRTC1 RGB 4BPP
		PTC12A,       //!< PVRTC1 RGBA 2BPP
		PTC14A,       //!< PVRTC1 RGBA 4BPP
		PTC22,        //!< PVRTC2 RGBA 2BPP
		PTC24,        //!< PVRTC2 RGBA 4BPP
		ATC,          //!< ATC RGB 4BPP
		ATCE,         //!< ATCE RGBA 8 BPP explicit alpha
		ATCI,         //!< ATCI RGBA 8 BPP interpolated alpha
		ASTC4x4,      //!< ASTC 4x4 8.0 BPP
		ASTC5x4,      //!< ASTC 5x4 6.40 BPP
		ASTC5x5,      //!< ASTC 5x5 5.12 BPP
		ASTC6x5,      //!< ASTC 6x5 4.27 BPP
		ASTC6x6,      //!< ASTC 6x6 3.56 BPP
		ASTC8x5,      //!< ASTC 8x5 3.20 BPP
		ASTC8x6,      //!< ASTC 8x6 2.67 BPP
		ASTC8x8,      //!< ASTC 8x8 2.00 BPP
		ASTC10x5,     //!< ASTC 10x5 2.56 BPP
		ASTC10x6,     //!< ASTC 10x6 2.13 BPP
		ASTC10x8,     //!< ASTC 10x8 1.60 BPP
		ASTC10x10,    //!< ASTC 10x10 1.28 BPP
		ASTC12x10,    //!< ASTC 12x10 1.07 BPP
		ASTC12x12,    //!< ASTC 12x12 0.89 BPP

		Unknown,      // Compressed formats above.

		R1,
		A8,
		R8,
		R8I,
		R8U,
		R8S,
		R16,
		R16I,
		R16U,
		R16F,
		R16S,
		R32I,
		R32U,
		R32F,
		RG8,
		RG8I,
		RG8U,
		RG8S,
		RG16,
		RG16I,
		RG16U,
		RG16F,
		RG16S,
		RG32I,
		RG32U,
		RG32F,
		RGB8,
		RGB8I,
		RGB8U,
		RGB8S,
		RGB9E5F,
		BGRA8,
		RGBA8,
		RGBA8I,
		RGBA8U,
		RGBA8S,
		RGBA16,
		RGBA16I,
		RGBA16U,
		RGBA16F,
		RGBA16S,
		RGBA32I,
		RGBA32U,
		RGBA32F,
		B5G6R5,
		R5G6B5,
		BGRA4,
		RGBA4,
		BGR5A1,
		RGB5A1,
		RGB10A2,
		RG11B10F,

		UnknownDepth, // Depth formats below.

		D16,
		D24,
		D24S8,
		D32,
		D16F,
		D24F,
		D32F,
		D0S8,

		Count
	}

	public enum TextureFlags : ulong
	{
		None = 0x0000000000000000,
		MSAASample = 0x0000000800000000, //!< Texture will be used for MSAA sampling.
		RenderTarget = 0x0000001000000000, //!< Render target no MSAA.
		ComputeWrite = 0x0000100000000000, //!< Texture will be used for compute write.
		SRGB = 0x0000200000000000, //!< Sample texture as sRGB.
		BlitDst = 0x0000400000000000, //!< Texture will be used as blit destination.
		ReadBack = 0x0000800000000000, //!< Texture will be used for read back from GPU.

		RenderTargetMSAAX2 = 0x0000002000000000, //!< Render target MSAAx2 mode.
		RenderTargetMSAAX4 = 0x0000003000000000, //!< Render target MSAAx4 mode.
		RenderTargetMSAAX8 = 0x0000004000000000, //!< Render target MSAAx8 mode.
		RenderTargetMSAAX16 = 0x0000005000000000, //!< Render target MSAAx16 mode.
		RenderTargetWriteOnly = 0x0000008000000000, //!< Render target will be used for writing
	}

	public enum SamplerFlags : uint
	{
		UMirror = 0x00000001, //!< Wrap U mode: Mirror
		UClamp = 0x00000002, //!< Wrap U mode: Clamp
		UBorder = 0x00000003, //!< Wrap U mode: Border

		VMirror = 0x00000004, //!< Wrap V mode: Mirror
		VClamp = 0x00000008, //!< Wrap V mode: Clamp
		VBorder = 0x0000000c, //!< Wrap V mode: Border

		WMirror = 0x00000010, //!< Wrap W mode: Mirror
		WClamp = 0x00000020, //!< Wrap W mode: Clamp
		WBorder = 0x00000030, //!< Wrap W mode: Border

		Mirror = UMirror | VMirror | WMirror,
		Clamp = UClamp | VClamp | WClamp,
		Border = UBorder | VBorder | WBorder,

		MinPoint = 0x00000040, //!< Min sampling mode: Point
		MinAnisotropic = 0x00000080, //!< Min sampling mode: Anisotropic

		MagPoint = 0x00000100, //!< Mag sampling mode: Point
		MagAnisotropic = 0x00000200, //!< Mag sampling mode: Anisotropic

		MipPoint = 0x00000400, //!< Mip sampling mode: Point

		Point = MinPoint | MagPoint | MipPoint,
		Anisotropic = MinAnisotropic | MagAnisotropic,

		CompareLess = 0x00010000, //!< Compare when sampling depth texture: less.
		CompareLEqual = 0x00020000, //!< Compare when sampling depth texture: less or equal.
		CompareEqual = 0x00030000, //!< Compare when sampling depth texture: equal.
		CompareGEqual = 0x00040000, //!< Compare when sampling depth texture: greater or equal.
		CompareGreater = 0x00050000, //!< Compare when sampling depth texture: greater.
		CompareNotEqual = 0x00060000, //!< Compare when sampling depth texture: not equal.
		CompareNever = 0x00070000, //!< Compare when sampling depth texture: never.
		CompareAlways = 0x00080000, //!< Compare when sampling depth texture: always.

		SampleStencil = 0x00100000,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct TextureInfo
	{
		public TextureFormat format;

		uint _storageSize;
		ushort _width;
		ushort _height;
		ushort _depth;
		ushort _numLayers;
		byte _numMips;
		byte _bitsPerPixel;
		byte _cubeMap;

		public int storageSize { get => (int)_storageSize; }
		public int width { get => _width; }
		public int height { get => _height; }
		public int depth { get => _depth; }
		public int numLayers { get => _numLayers; }
		public int numMips { get => _numMips; }
		public int bitsPerPixel { get => _bitsPerPixel; }
		public bool cubeMap { get => _cubeMap != 0; }
	}

	public class Texture
	{
		public ushort handle;

		public readonly TextureInfo info;


		internal Texture(ushort handle, TextureInfo info)
		{
			this.handle = handle;
			this.info = info;
		}

		public int width { get => info.width; }
		public int height { get => info.height; }
		public int depth { get => info.depth; }
		public Vector3i size { get => new Vector3i(info.width, info.height, info.depth); }
	}
}
