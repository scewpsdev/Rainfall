

#include "Rainfall.h"
#include "Application.h"
#include "Resource.h"
#include "Console.h"

#include "Model.h"
#include "Shader.h"
#include "ModelReader.h"
#include "Font.h"

#include "vector/Matrix.h"

#include <bgfx/bgfx.h>
#include <bx/allocator.h>
#include <bimg/decode.h>

#include <string.h>
#include <stdint.h>
#include <stdio.h>


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
};

struct RenderTargetAttachment
{
	int width, height;
	bgfx::BackbufferRatio::Enum ratio;
	bgfx::TextureFormat::Enum format;
	uint64_t flags;

	bgfx::TextureHandle texture;
	int textureLayer;
	bool isCubemap;

	bool generateMipmaps;
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


static int vertexAttributeTypeCounts[(int)VertexAttributeType::Count] = { 4, 1, 1, 2, 3, 4 };
static bgfx::AttribType::Enum vertexAttributeTypesBgfx[(int)VertexAttributeType::Count] = {
	bgfx::AttribType::Uint8,
	bgfx::AttribType::Half,
	bgfx::AttribType::Float,
	bgfx::AttribType::Float,
	bgfx::AttribType::Float,
	bgfx::AttribType::Float,
};

static uint64_t blendStateValues[(int)BlendState::Count] = {
	0,
	BGFX_STATE_BLEND_ALPHA,
	BGFX_STATE_BLEND_FUNC_SEPARATE(BGFX_STATE_BLEND_SRC_ALPHA, BGFX_STATE_BLEND_ONE, BGFX_STATE_BLEND_SRC_ALPHA, BGFX_STATE_BLEND_INV_SRC_ALPHA),
};

static uint64_t depthTestValues[(int)DepthTest::Count] = {
	0,
	BGFX_STATE_DEPTH_TEST_LESS,
	BGFX_STATE_DEPTH_TEST_LEQUAL,
	BGFX_STATE_DEPTH_TEST_EQUAL,
	BGFX_STATE_DEPTH_TEST_GEQUAL,
	BGFX_STATE_DEPTH_TEST_GREATER,
	BGFX_STATE_DEPTH_TEST_NOTEQUAL,
	BGFX_STATE_DEPTH_TEST_NEVER,
	BGFX_STATE_DEPTH_TEST_ALWAYS,
};

static uint64_t cullStateValues[(int)CullState::Count] = {
	0,
	BGFX_STATE_CULL_CW,
	BGFX_STATE_CULL_CCW,
};

static uint64_t primitiveTypeValues[(int)PrimitiveType::Count] = {
	0,
	BGFX_STATE_PT_TRISTRIP,
	BGFX_STATE_PT_LINES,
	BGFX_STATE_PT_LINESTRIP,
	BGFX_STATE_PT_POINTS,
};


static uint64_t state = 0;
static uint16_t currentRenderTarget = bgfx::kInvalidHandle;


static bgfx::VertexLayout CreateVertexLayout(const VertexElement* layoutElements, int layoutElementsCount)
{
	bgfx::VertexLayout layout;
	layout.begin();
	for (int i = 0; i < layoutElementsCount; i++)
	{
		layout.add(
			(bgfx::Attrib::Enum)layoutElements[i].attribute,
			vertexAttributeTypeCounts[(int)layoutElements[i].type],
			vertexAttributeTypesBgfx[(int)layoutElements[i].type],
			layoutElements[i].normalized
		);
	}
	layout.end();
	return layout;
}

RFAPI void* Graphics_AllocateNativeMemory(int size)
{
	return bx::alloc(Application_GetAllocator(), size);
}

RFAPI void Graphics_FreeNativeMemory(void* ptr)
{
	bx::free(Application_GetAllocator(), ptr);
}

RFAPI const void* Graphics_AllocateVideoMemory(int size, const bgfx::Memory** outDataPtr)
{
	const bgfx::Memory* memory = bgfx::alloc(size);
	*outDataPtr = memory;
	return memory->data;
}

RFAPI void Graphics_CreateVideoMemoryRef(int size, const void* data, void(releaseCallback)(void* _ptr, void* _userData), const bgfx::Memory** outDataPtr)
{
	const bgfx::Memory* memory = bgfx::makeRef(data, size, releaseCallback);
	*outDataPtr = memory;
}

RFAPI uint16_t Graphics_CreateVertexBuffer(const bgfx::Memory* memory, const VertexElement* layoutElements, int layoutElementsCount, uint16_t flags)
{
	bgfx::VertexLayout layout = CreateVertexLayout(layoutElements, layoutElementsCount);
	bgfx::VertexBufferHandle handle = bgfx::createVertexBuffer(memory, layout, flags);
	return handle.idx;
}

RFAPI void Graphics_DestroyVertexBuffer(uint16_t buffer)
{
	bgfx::destroy(bgfx::VertexBufferHandle{ buffer });
}

RFAPI uint16_t Graphics_CreateDynamicVertexBuffer(const VertexElement* layoutElements, int layoutElementsCount, int vertexCount, uint16_t flags)
{
	bgfx::VertexLayout layout = CreateVertexLayout(layoutElements, layoutElementsCount);
	bgfx::DynamicVertexBufferHandle handle = bgfx::createDynamicVertexBuffer(vertexCount, layout, flags);
	return handle.idx;
}

RFAPI void Graphics_DestroyDynamicVertexBuffer(uint16_t buffer)
{
	bgfx::destroy(bgfx::DynamicVertexBufferHandle{ buffer });
}

RFAPI bool Graphics_CreateTransientVertexBuffer(const VertexElement* layoutElements, int layoutElementsCount, int vertexCount, bgfx::TransientVertexBuffer* buffer)
{
	bgfx::VertexLayout layout = CreateVertexLayout(layoutElements, layoutElementsCount);
	if (bgfx::getAvailTransientVertexBuffer(vertexCount, layout) == vertexCount)
	{
		bgfx::allocTransientVertexBuffer(buffer, vertexCount, layout);
		return true;
	}
	return false;
}

RFAPI uint16_t Graphics_CreateIndexBuffer(const bgfx::Memory* memory, uint16_t flags)
{
	bgfx::IndexBufferHandle handle = bgfx::createIndexBuffer(memory, flags);
	return handle.idx;
}

RFAPI void Graphics_DestroyIndexBuffer(uint16_t buffer)
{
	bgfx::destroy(bgfx::IndexBufferHandle{ buffer });
}

RFAPI uint16_t Graphics_CreateDynamicIndexBuffer(int indexCount, uint16_t flags)
{
	bgfx::DynamicIndexBufferHandle handle = bgfx::createDynamicIndexBuffer(indexCount, flags);
	return handle.idx;
}

RFAPI void Graphics_DestroyDynamicIndexBuffer(uint16_t buffer)
{
	bgfx::destroy(bgfx::DynamicIndexBufferHandle{ buffer });
}

RFAPI bgfx::TransientIndexBuffer Graphics_CreateTransientIndexBuffer(int indexCount, bool index32)
{
	if (bgfx::getAvailTransientIndexBuffer(indexCount, index32) == indexCount)
	{
		bgfx::TransientIndexBuffer buffer;
		bgfx::allocTransientIndexBuffer(&buffer, indexCount, index32);
		return buffer;
	}
	return {};
}

RFAPI bool Graphics_CreateInstanceBuffer(int count, int stride, bgfx::InstanceDataBuffer* buffer)
{
	if (bgfx::getAvailInstanceDataBuffer(count, stride) == count)
	{
		bgfx::allocInstanceDataBuffer(buffer, count, stride);
		return true;
	}
	return false;
}

RFAPI uint16_t Graphics_CreateTextureMutable(int width, int height, bgfx::TextureFormat::Enum format, uint64_t flags, bgfx::TextureInfo* info)
{
	bgfx::TextureHandle handle = bgfx::createTexture2D(width, height, false, 1, format, flags);
	bgfx::calcTextureSize(*info, width, height, 0, false, false, 1, format);
	return handle.idx;
}

RFAPI uint16_t Graphics_CreateTextureMutableR(bgfx::BackbufferRatio::Enum ratio, bool hasMips, bgfx::TextureFormat::Enum format, uint64_t flags, bgfx::TextureInfo* info)
{
	bgfx::TextureHandle handle = bgfx::createTexture2D(ratio, hasMips, 1, format, flags);
	bgfx::calcTextureSize(*info, info->width, info->height, info->depth, false, hasMips, 1, format);
	return handle.idx;
}

RFAPI void Graphics_SetTextureData(uint16_t texture, int x, int y, int width, int height, const bgfx::Memory* memory)
{
	bgfx::TextureHandle handle = { texture };
	bgfx::updateTexture2D(handle, 0, 0, x, y, width, height, memory);
}

RFAPI uint16_t Graphics_CreateTextureImmutable(int width, int height, bgfx::TextureFormat::Enum format, uint64_t flags, const bgfx::Memory* memory, bgfx::TextureInfo* info)
{
	bgfx::TextureHandle handle = bgfx::createTexture2D(width, height, false, 1, format, flags, memory);
	bgfx::calcTextureSize(*info, width, height, 0, false, false, 1, format);
	return handle.idx;
}

RFAPI uint16_t Graphics_CreateTextureFromMemory(const bgfx::Memory* memory, uint64_t flags, bgfx::TextureInfo* info)
{
	bgfx::TextureHandle handle = bgfx::createTexture(memory, flags, 0, info);
	return handle.idx;
}

RFAPI uint16_t Graphics_CreateTexture3DImmutable(int width, int height, int depth, bgfx::TextureFormat::Enum format, uint64_t flags, const bgfx::Memory* memory, bgfx::TextureInfo* info)
{
	bgfx::TextureHandle handle = bgfx::createTexture3D(width, height, depth, false, format, flags, memory);
	bgfx::calcTextureSize(*info, width, height, depth, false, false, 1, format);
	return handle.idx;
}

RFAPI uint16_t Graphics_CreateTexture3DMutable(int width, int height, int depth, bool hasMips, bgfx::TextureFormat::Enum format, uint64_t flags, bgfx::TextureInfo* info)
{
	bgfx::TextureHandle handle = bgfx::createTexture3D(width, height, depth, hasMips, format, flags);
	bgfx::calcTextureSize(*info, width, height, depth, false, hasMips, 1, format);
	return handle.idx;
}

RFAPI void Graphics_SetTexture3DData(uint16_t texture, int mip, int x, int y, int z, int width, int height, int depth, const bgfx::Memory* memory)
{
	bgfx::TextureHandle handle = { texture };
	bgfx::updateTexture3D(handle, mip, x, y, z, width, height, depth, memory);
}

static void ImageReleaseCallback(void* ptr, void* userData)
{
	BX_UNUSED(ptr);
	bimg::ImageContainer* image = (bimg::ImageContainer*)userData;
	bimg::imageFree(image);
}

RFAPI uint16_t Graphics_CreateCubemapFromMemory(const bgfx::Memory* memory, uint64_t flags, bgfx::TextureInfo* info)
{
	bimg::ImageContainer* image = bimg::imageParse(Application_GetAllocator(), memory->data, memory->size);
	bgfx::calcTextureSize(*info, image->m_width, image->m_height, image->m_depth, image->m_cubeMap, image->m_numMips > 1, image->m_numLayers, (bgfx::TextureFormat::Enum)image->m_format);

	const bgfx::Memory* textureMemory = bgfx::makeRef(image->m_data, image->m_size, ImageReleaseCallback, image);

	bgfx::TextureHandle handle = bgfx::createTextureCube(image->m_width, image->m_numMips > 1, image->m_numLayers, (bgfx::TextureFormat::Enum)image->m_format, 0, textureMemory);
	return handle.idx;
}

RFAPI uint16_t Graphics_CreateCubemap(int size, bgfx::TextureFormat::Enum format, uint64_t flags, bgfx::TextureInfo* info)
{
	bgfx::TextureHandle handle = bgfx::createTextureCube(size, true, 1, format, flags, nullptr);
	bgfx::calcTextureSize(*info, size, size, 1, true, true, 1, format);
	return handle.idx;
}

RFAPI void Graphics_DestroyTexture(uint16_t texture)
{
	bgfx::destroy(bgfx::TextureHandle{ texture });
}

RFAPI Shader* Graphics_CreateShader(const bgfx::Memory* vertexMemory, const bgfx::Memory* fragmentMemory)
{
	bgfx::ShaderHandle vertex = bgfx::createShader(vertexMemory);
	bgfx::ShaderHandle fragment = bgfx::createShader(fragmentMemory);
	bgfx::ProgramHandle program = bgfx::createProgram(vertex, fragment, true);

	Shader* shader = BX_NEW(Application_GetAllocator(), Shader);
	shader->program = program;

	return shader;
}

RFAPI Shader* Graphics_CreateShaderCompute(const bgfx::Memory* computeMemory)
{
	const bgfx::Caps* caps = bgfx::getCaps();
	if (!(caps->supported & BGFX_CAPS_COMPUTE))
	{
		Console_Error("Compute shaders not supported!");
		return nullptr;
	}

	bgfx::ShaderHandle compute = bgfx::createShader(computeMemory);
	bgfx::ProgramHandle program = bgfx::createProgram(compute, true);

	Shader* shader = BX_NEW(Application_GetAllocator(), Shader);
	shader->program = program;

	return shader;
}

RFAPI uint16_t Graphics_ShaderGetUniform(Shader* shader, const char* name, bgfx::UniformType::Enum type, int num)
{
	bgfx::UniformHandle handle = shader->getUniform(name, type, num);
	return handle.idx;
}

RFAPI void Graphics_DestroyShader(Shader* shader)
{
	bgfx::destroy(shader->program);
}

RFAPI uint16_t Graphics_CreateRenderTarget(int numAttachments, const RenderTargetAttachment* attachmentInfo, bgfx::TextureInfo* textureInfos, uint16_t* textures)
{
	bgfx::Attachment attachments[8] = {};
	for (int i = 0; i < numAttachments; i++)
	{
		bgfx::TextureHandle texture = attachmentInfo[i].texture;
		if (texture.idx == bgfx::kInvalidHandle)
		{
			if (!bgfx::isTextureValid(0, false, 1, attachmentInfo[i].format, attachmentInfo[i].flags))
			{
				Console_Error("Invalid framebuffer attachment #%i format", i);
				return bgfx::kInvalidHandle;
			}

			if (attachmentInfo[i].ratio < bgfx::BackbufferRatio::Count)
				texture = bgfx::createTexture2D(attachmentInfo[i].ratio, false, 1, attachmentInfo[i].format, attachmentInfo[i].flags);
			else
				texture = bgfx::createTexture2D(attachmentInfo[i].width, attachmentInfo[i].height, false, 1, attachmentInfo[i].format, attachmentInfo[i].flags);

			textures[i] = texture.idx;
			textureInfos[i] = bgfx::TextureInfo{ attachmentInfo[i].format, 0, (uint16_t)attachmentInfo[i].width, (uint16_t)attachmentInfo[i].height, 0, 1, 1, 0, false };
		}

		attachments[i].init(texture, bgfx::Access::Write, BGFX_CUBE_MAP_POSITIVE_X + attachmentInfo[i].textureLayer, attachmentInfo[i].isCubemap ? 1 : 1, 0, attachmentInfo[i].generateMipmaps ? BGFX_RESOLVE_AUTO_GEN_MIPS : BGFX_RESOLVE_NONE);
	}

	bgfx::FrameBufferHandle handle = bgfx::createFrameBuffer(numAttachments, attachments, attachmentInfo[0].texture.idx == bgfx::kInvalidHandle);
	if (!bgfx::isValid(handle))
	{
		Console_Error("Failed to validate framebuffer %i", (int)handle.idx);
		return bgfx::kInvalidHandle;
	}

	return handle.idx;
}

RFAPI void Graphics_DestroyRenderTarget(uint16_t renderTarget)
{
	bgfx::destroy(bgfx::FrameBufferHandle{ renderTarget });
}


RFAPI void Graphics_ResetState()
{
	bgfx::discard();
	bgfx::setState(state = BGFX_STATE_DEFAULT);
}

RFAPI uint64_t Graphics_GetState()
{
	return state;
}

RFAPI void Graphics_SetBlendState(BlendState blendState)
{
	state &= 0xffffffffffffffff ^ BGFX_STATE_BLEND_MASK;
	state |= blendStateValues[(int)blendState];
	bgfx::setState(state);
}

RFAPI void Graphics_SetDepthTest(DepthTest depthTest)
{
	state &= 0xffffffffffffffff ^ BGFX_STATE_DEPTH_TEST_MASK;
	state |= depthTestValues[(int)depthTest];
	bgfx::setState(state);
}

RFAPI void Graphics_SetCullState(CullState cullState)
{
	state &= 0xffffffffffffffff ^ BGFX_STATE_CULL_MASK;
	state |= cullStateValues[(int)cullState];
	bgfx::setState(state);
}

RFAPI void Graphics_SetPrimitiveType(PrimitiveType primitiveType)
{
	state &= 0xffffffffffffffff ^ BGFX_STATE_PT_MASK;
	state |= primitiveTypeValues[(int)primitiveType];
	bgfx::setState(state);
}

RFAPI void Graphics_SetVertexBuffer(uint16_t handle)
{
	bgfx::setVertexBuffer(0, bgfx::VertexBufferHandle{ handle });
}

RFAPI void Graphics_SetDynamicVertexBuffer(uint16_t handle)
{
	bgfx::setVertexBuffer(0, bgfx::DynamicVertexBufferHandle{ handle });
}

RFAPI void Graphics_SetTransientVertexBuffer(const bgfx::TransientVertexBuffer* buffer)
{
	bgfx::setVertexBuffer(0, buffer);
}

RFAPI void Graphics_SetIndexBuffer(uint16_t handle)
{
	bgfx::setIndexBuffer(bgfx::IndexBufferHandle{ handle });
}

RFAPI void Graphics_SetDynamicIndexBuffer(uint16_t handle)
{
	bgfx::setIndexBuffer(bgfx::DynamicIndexBufferHandle{ handle });
}

RFAPI void Graphics_SetTransientIndexBuffer(const bgfx::TransientIndexBuffer* buffer)
{
	bgfx::setIndexBuffer(buffer);
}

RFAPI void Graphics_SetInstanceBuffer(const bgfx::InstanceDataBuffer* buffer)
{
	bgfx::setInstanceDataBuffer(buffer);
}

RFAPI void Graphics_SetInstanceBufferN(const bgfx::InstanceDataBuffer* buffer, int offset, int count)
{
	bgfx::setInstanceDataBuffer(buffer, offset, count);
}

RFAPI void Graphics_SetComputeBuffer(int stage, uint16_t handle, bgfx::Access::Enum access)
{
	bgfx::setBuffer(stage, bgfx::VertexBufferHandle{ handle }, access);
}

RFAPI void Graphics_SetUniform(uint16_t handle, const void* value, int num)
{
	bgfx::setUniform(bgfx::UniformHandle{ handle }, value, num);
}

RFAPI void Graphics_SetTexture(uint16_t sampler, int unit, uint16_t texture, uint32_t flags)
{
	bgfx::setTexture(unit, bgfx::UniformHandle{ sampler }, bgfx::TextureHandle{ texture }, flags);
}

RFAPI void Graphics_SetComputeTexture(int stage, uint16_t texture, int mip, bgfx::Access::Enum access)
{
	bgfx::setImage(stage, bgfx::TextureHandle{ texture }, mip, access);
}

RFAPI void Graphics_SetRenderTarget(int pass, uint16_t handle, int width, int height, bool hasRGB, bool hasDepth, uint32_t rgba, float depth)
{
	bgfx::setViewFrameBuffer((bgfx::ViewId)pass, bgfx::FrameBufferHandle{ handle });
	bgfx::setViewRect((bgfx::ViewId)pass, 0, 0, width, height);

	if (currentRenderTarget != handle)
	{
		bgfx::setViewClear((bgfx::ViewId)pass, (hasRGB ? BGFX_CLEAR_COLOR : 0) | (hasDepth ? BGFX_CLEAR_DEPTH : 0), rgba, depth, 0);
		currentRenderTarget = handle;
	}

	bgfx::touch((bgfx::ViewId)pass);
}

RFAPI void Graphics_SetRenderTargetR(int pass, uint16_t handle, bgfx::BackbufferRatio::Enum ratio, bool hasRGB, bool hasDepth, uint32_t rgba, float depth)
{
	bgfx::setViewFrameBuffer((bgfx::ViewId)pass, bgfx::FrameBufferHandle{ handle });
	bgfx::setViewRect((bgfx::ViewId)pass, 0, 0, ratio);

	if (currentRenderTarget != handle)
	{
		bgfx::setViewClear((bgfx::ViewId)pass, (hasRGB ? BGFX_CLEAR_COLOR : 0) | (hasDepth ? BGFX_CLEAR_DEPTH : 0), rgba, depth, 0);
		currentRenderTarget = handle;
	}

	bgfx::touch((bgfx::ViewId)pass);
}

RFAPI void Graphics_SetTransform(int pass, const Matrix& transform)
{
	bgfx::setTransform(transform.elements);
}

RFAPI void Graphics_SetViewTransform(int pass, const Matrix& projection, const Matrix& view)
{
	bgfx::setViewTransform((bgfx::ViewId)pass, &view.m00, &projection.m00);
}

RFAPI void Graphics_Draw(int pass, Shader* shader)
{
	bgfx::submit((bgfx::ViewId)pass, shader->program, 0, BGFX_DISCARD_ALL);
}

RFAPI void Graphics_DrawText(int pass, int x, int y, float z, float scale, const char* text, int offset, int count, Font* font, uint32_t color, SpriteBatch* batch)
{
	font->drawText((bgfx::ViewId)pass, x, y, z, scale, text, offset, count, color, batch);
}

RFAPI void Graphics_DrawDebugText(int x, int y, uint8_t color, const char* text)
{
	bgfx::dbgTextPrintf(x, y, color, "%s", text);
}

RFAPI void Graphics_GetDebugTextSize(int* outWidth, int* outHeight)
{
	*outWidth = bgfx::getStats()->textWidth;
	*outHeight = bgfx::getStats()->textHeight;
}

RFAPI void Graphics_ComputeDispatch(int pass, Shader* shader, int numX, int numY, int numZ)
{
	bgfx::dispatch((bgfx::ViewId)pass, shader->program, numX, numY, numZ, BGFX_DISCARD_ALL);
}

RFAPI void Graphics_Blit(int pass, uint16_t dst, uint16_t src)
{
	bgfx::blit((bgfx::ViewId)pass, bgfx::TextureHandle{ dst }, 0, 0, bgfx::TextureHandle{ src }, 0, 0);
}

RFAPI void Graphics_BlitEx(int pass, uint16_t dst, int dstMip, int dstX, int dstY, int dstZ, uint16_t src, int srcMip, int srcX, int srcY, int srcZ, int width, int height, int depth)
{
	bgfx::blit((bgfx::ViewId)pass, bgfx::TextureHandle{ dst }, dstMip, dstX, dstY, dstZ, bgfx::TextureHandle{ src }, srcMip, srcX, srcY, srcZ, width, height, depth);
}

RFAPI void Graphics_CompleteFrame()
{
	bgfx::frame();
}

RFAPI void Graphics_GetRenderStats(bgfx::Stats* renderStats)
{
	const bgfx::Stats* stats = bgfx::getStats();
	*renderStats = *stats;
}
