#pragma once

#include "Rainfall.h"
#include "Font.h"

#include "vector/Matrix.h"

#include "Shader.h"

#include <bgfx/bgfx.h>


enum class VertexAttribute : int
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
};

enum class VertexAttributeType : int
{
	Byte4,
	Half,
	Single,
	Vector2,
	Vector3,
	Vector4,

	Count
};

struct VertexElement
{
	VertexAttribute attribute;
	VertexAttributeType type;
	bool normalized;

	VertexElement(VertexAttribute attribute, VertexAttributeType type, bool normalized = false)
		: attribute(attribute), type(type), normalized(normalized)
	{
	}
};

enum BufferFlags : uint16_t
{
	None = 0x0000,
	ComputeRead = 0x0100,
	ComputeWrite = 0x0200,
	DrawIndirect = 0x0400,
	AllowResize = 0x0800,
	Index32 = 0x1000,
};

enum class BlendState
{
	Default,
	Alpha,
	Additive,

	Count
};

enum class DepthTest
{
	None,
	Less,
	LEqual,
	Equal,
	GEqual,
	Greater,
	NotEqual,
	Never,
	Always,

	Count
};

enum class CullState
{
	None,
	ClockWise,
	CounterClockWise,

	Count
};

enum class PrimitiveType
{
	Triangle,
	TriangleStrip,
	Lines,
	LineStrip,
	Points,

	Count
};

struct RenderTargetAttachment
{
	int width = 0, height = 0;
	bgfx::BackbufferRatio::Enum ratio = bgfx::BackbufferRatio::Count;
	bgfx::TextureFormat::Enum format = bgfx::TextureFormat::Count;
	uint64_t flags = 0;

	bgfx::TextureHandle texture = BGFX_INVALID_HANDLE;
	int textureLayer = 0;
	bool isCubemap = false;

	bool generateMipmaps = false;


	RenderTargetAttachment(bgfx::BackbufferRatio::Enum ratio = bgfx::BackbufferRatio::Equal, bgfx::TextureFormat::Enum format = bgfx::TextureFormat::RGBA8, uint64_t flags = 0, bool generateMipmaps = false)
		: ratio(ratio), format(format), flags(flags), generateMipmaps(generateMipmaps)
	{
	}

	RenderTargetAttachment(int width, int height, bgfx::TextureFormat::Enum format = bgfx::TextureFormat::RGBA8, uint64_t flags = 0, bool generateMipmaps = false)
		: width(width), height(height), format(format), flags(flags), generateMipmaps(generateMipmaps)
	{
	}

	RenderTargetAttachment(bgfx::TextureHandle texture, bgfx::TextureInfo* info, bool generateMipmaps = false)
		: width(info->width), height(info->height), format(info->format), texture(texture), generateMipmaps(generateMipmaps)
	{
	}

	RenderTargetAttachment(bgfx::TextureHandle cubemap, bgfx::TextureInfo* info, int idx, bool generateMipmaps = false)
		: width(info->width), height(info->height), format(info->format), texture(cubemap), textureLayer(idx), isCubemap(true), generateMipmaps(generateMipmaps)
	{
	}
};


RFAPI void* Graphics_AllocateNativeMemory(int size);
RFAPI void Graphics_FreeNativeMemory(void* ptr);
RFAPI const void* Graphics_AllocateVideoMemory(int size, const bgfx::Memory** outDataPtr);
RFAPI const bgfx::Memory* Graphics_CreateVideoMemoryRef(int size, const void* data, void(releaseCallback)(void* _ptr, void* _userData));

RFAPI uint16_t Graphics_CreateVertexBuffer(const bgfx::Memory* memory, const VertexElement* layoutElements, int layoutElementsCount, BufferFlags flags);
RFAPI void Graphics_DestroyVertexBuffer(uint16_t buffer);
RFAPI uint16_t Graphics_CreateDynamicVertexBuffer(const VertexElement* layoutElements, int layoutElementsCount, int vertexCount, uint16_t flags);
RFAPI void Graphics_DestroyDynamicVertexBuffer(uint16_t buffer);
RFAPI bool Graphics_CreateTransientVertexBuffer(const VertexElement* layoutElements, int layoutElementsCount, int vertexCount, bgfx::TransientVertexBuffer* buffer);
RFAPI uint16_t Graphics_CreateIndexBuffer(const bgfx::Memory* memory, uint16_t flags);
RFAPI void Graphics_DestroyIndexBuffer(uint16_t buffer);
RFAPI uint16_t Graphics_CreateDynamicIndexBuffer(int indexCount, uint16_t flags);
RFAPI void Graphics_DestroyDynamicIndexBuffer(uint16_t buffer);
RFAPI bgfx::TransientIndexBuffer Graphics_CreateTransientIndexBuffer(int indexCount, bool index32);
RFAPI bool Graphics_CreateInstanceBuffer(int count, int stride, bgfx::InstanceDataBuffer* buffer);
RFAPI uint16_t Graphics_CreateIndirectBuffer(int count);
RFAPI uint16_t Graphics_CreateTextureMutable(int width, int height, bgfx::TextureFormat::Enum format, uint64_t flags, bgfx::TextureInfo* info);
RFAPI uint16_t Graphics_CreateTextureMutableEx(int width, int height, bgfx::TextureFormat::Enum format, bool hasMips, int numLayers, uint64_t flags, bgfx::TextureInfo* info);
RFAPI uint16_t Graphics_CreateTextureMutableR(bgfx::BackbufferRatio::Enum ratio, bool hasMips, bgfx::TextureFormat::Enum format, uint64_t flags, bgfx::TextureInfo* info);
RFAPI void Graphics_SetTextureData(uint16_t texture, int x, int y, int width, int height, const bgfx::Memory* memory);
RFAPI uint16_t Graphics_CreateTextureImmutable(int width, int height, bgfx::TextureFormat::Enum format, uint64_t flags, const bgfx::Memory* memory, bgfx::TextureInfo* info);
RFAPI uint16_t Graphics_CreateTextureFromMemory(const bgfx::Memory* memory, uint64_t flags, bgfx::TextureInfo* info);
RFAPI uint16_t Graphics_CreateTexture3DImmutable(int width, int height, int depth, bgfx::TextureFormat::Enum format, uint64_t flags, const bgfx::Memory* memory, bgfx::TextureInfo* info);
RFAPI uint16_t Graphics_CreateTexture3DMutable(int width, int height, int depth, bool hasMips, bgfx::TextureFormat::Enum format, uint64_t flags, bgfx::TextureInfo* info);
RFAPI void Graphics_SetTexture3DData(uint16_t texture, int mip, int x, int y, int z, int width, int height, int depth, const bgfx::Memory* memory);
RFAPI uint16_t Graphics_CreateCubemapFromMemory(const bgfx::Memory* memory, uint64_t flags, bgfx::TextureInfo* info);
RFAPI uint16_t Graphics_CreateCubemap(int size, bgfx::TextureFormat::Enum format, uint64_t flags, bgfx::TextureInfo* info);
RFAPI void Graphics_DestroyTexture(uint16_t texture);
RFAPI Shader* Graphics_CreateShader(const bgfx::Memory* vertexMemory, const bgfx::Memory* fragmentMemory);
RFAPI Shader* Graphics_CreateShaderCompute(const bgfx::Memory* computeMemory);
RFAPI uint16_t Graphics_ShaderGetUniform(Shader* shader, const char* name, bgfx::UniformType::Enum type, int num);
RFAPI void Graphics_DestroyShader(Shader* shader);
RFAPI uint16_t Graphics_CreateRenderTarget(int numAttachments, const RenderTargetAttachment* attachmentInfo, bgfx::TextureHandle* textures, bgfx::TextureInfo* textureInfos);
RFAPI void Graphics_DestroyRenderTarget(uint16_t renderTarget);
RFAPI void Graphics_ResetState();
RFAPI uint64_t Graphics_GetState();
RFAPI void Graphics_SetBlendState(BlendState blendState);
RFAPI void Graphics_SetDepthTest(DepthTest depthTest);
RFAPI void Graphics_SetCullState(CullState cullState);
RFAPI void Graphics_SetPrimitiveType(PrimitiveType primitiveType);
RFAPI void Graphics_SetVertexBuffer(uint16_t handle);
RFAPI void Graphics_SetDynamicVertexBuffer(uint16_t handle);
RFAPI void Graphics_SetTransientVertexBuffer(const bgfx::TransientVertexBuffer* buffer);
RFAPI void Graphics_SetIndexBuffer(uint16_t handle);
RFAPI void Graphics_SetDynamicIndexBuffer(uint16_t handle);
RFAPI void Graphics_SetTransientIndexBuffer(const bgfx::TransientIndexBuffer* buffer);
RFAPI void Graphics_SetInstanceBuffer(const bgfx::InstanceDataBuffer* buffer);
RFAPI void Graphics_SetInstanceBufferN(const bgfx::InstanceDataBuffer* buffer, int offset, int count);
RFAPI void Graphics_SetComputeBuffer(int stage, uint16_t handle, bgfx::Access::Enum access);
RFAPI void Graphics_SetUniform(uint16_t handle, const void* value, int num = 1);
inline void Graphics_SetUniform(bgfx::UniformHandle handle, const void* value, int num = 1) { Graphics_SetUniform(handle.idx, value, num); }
RFAPI void Graphics_SetTexture(uint16_t sampler, int unit, uint16_t texture, uint32_t flags = UINT32_MAX);
inline void Graphics_SetTexture(bgfx::UniformHandle sampler, int unit, bgfx::TextureHandle texture, uint32_t flags = UINT32_MAX) { Graphics_SetTexture(sampler.idx, unit, texture.idx, flags); }
RFAPI void Graphics_SetComputeTexture(int stage, uint16_t texture, int mip, bgfx::Access::Enum access);
RFAPI void Graphics_SetRenderTarget(int pass, uint16_t handle, int width, int height);
inline void Graphics_SetRenderTarget(int pass, bgfx::FrameBufferHandle handle, int width, int height) { Graphics_SetRenderTarget(pass, handle.idx, width, height); }
RFAPI void Graphics_SetRenderTargetR(int pass, uint16_t handle, bgfx::BackbufferRatio::Enum ratio);
RFAPI void Graphics_ClearRenderTarget(int pass, uint16_t handle, bool hasRGB, bool hasDepth, uint32_t rgba, float depth);
RFAPI void Graphics_SetTransform(int pass, const Matrix& transform);
RFAPI void Graphics_SetViewTransform(int pass, const Matrix& projection, const Matrix& view);
RFAPI void Graphics_Draw(int pass, Shader* shader);
RFAPI void Graphics_DrawText(int pass, int x, int y, float z, float scale, const char* text, int offset, int count, Font* font, uint32_t color, SpriteBatch* batch);
RFAPI void Graphics_DrawDebugText(int x, int y, uint8_t color, const char* text);
RFAPI void Graphics_GetDebugTextSize(int* outWidth, int* outHeight);
RFAPI void Graphics_ComputeDispatch(int pass, Shader* shader, int numX, int numY, int numZ);
RFAPI void Graphics_Blit(int pass, uint16_t dst, uint16_t src);
inline void Graphics_Blit(int pass, bgfx::TextureHandle dst, bgfx::TextureHandle src) { Graphics_Blit(pass, dst.idx, src.idx); }
RFAPI void Graphics_BlitEx(int pass, uint16_t dst, int dstMip, int dstX, int dstY, int dstZ, uint16_t src, int srcMip, int srcX, int srcY, int srcZ, int width, int height, int depth);
RFAPI void Graphics_CompleteFrame();
RFAPI void Graphics_GetRenderStats(bgfx::Stats* renderStats);
RFAPI int Graphics_DrawDebugInfo(int x, int y, uint8_t color);
