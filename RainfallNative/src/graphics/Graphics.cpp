#include "Graphics.h"

#include "Rainfall.h"
#include "Application.h"
#include "Resource.h"
#include "Console.h"

#include "Model.h"
#include "Shader.h"
#include "ModelReader.h"

#include "vector/Matrix.h"

#include <bgfx/bgfx.h>
#include <bx/allocator.h>
#include <bx/os.h>
#include <bimg/decode.h>

#include <string.h>
#include <stdint.h>
#include <stdio.h>


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
	BGFX_STATE_BLEND_FUNC(BGFX_STATE_BLEND_ZERO, BGFX_STATE_BLEND_SRC_COLOR),
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

RFAPI const bgfx::Memory* Graphics_CreateVideoMemoryRef(int size, const void* data, void(releaseCallback)(void* _ptr, void* _userData))
{
	return bgfx::makeRef(data, size, releaseCallback);
}

RFAPI uint16_t Graphics_CreateVertexBuffer(const bgfx::Memory* memory, const VertexElement* layoutElements, int layoutElementsCount, BufferFlags flags)
{
	bgfx::VertexLayout layout = CreateVertexLayout(layoutElements, layoutElementsCount);
	bgfx::VertexBufferHandle handle = bgfx::createVertexBuffer(memory, layout, flags);
	return handle.idx;
}

RFAPI void Graphics_DestroyVertexBuffer(VertexBuffer buffer)
{
	bgfx::destroy(bgfx::VertexBufferHandle{ buffer });
}

RFAPI uint16_t Graphics_CreateDynamicVertexBuffer(const VertexElement* layoutElements, int layoutElementsCount, int vertexCount, uint16_t flags)
{
	bgfx::VertexLayout layout = CreateVertexLayout(layoutElements, layoutElementsCount);
	bgfx::DynamicVertexBufferHandle handle = bgfx::createDynamicVertexBuffer(vertexCount, layout, flags);
	return handle.idx;
}

RFAPI uint16_t Graphics_CreateDynamicVertexBufferFromMemory(const bgfx::Memory* memory, const VertexElement* layoutElements, int layoutElementsCount, uint16_t flags)
{
	bgfx::VertexLayout layout = CreateVertexLayout(layoutElements, layoutElementsCount);
	bgfx::DynamicVertexBufferHandle handle = bgfx::createDynamicVertexBuffer(memory, layout, flags);
	return handle.idx;
}

RFAPI void Graphics_DestroyDynamicVertexBuffer(uint16_t buffer)
{
	bgfx::destroy(bgfx::DynamicVertexBufferHandle{ buffer });
}

RFAPI void Graphics_UpdateDynamicVertexBuffer(uint16_t buffer, int startVertex, const bgfx::Memory* memory)
{
	bgfx::update(bgfx::DynamicVertexBufferHandle{ buffer }, startVertex, memory);
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

RFAPI IndexBuffer Graphics_CreateIndexBuffer(const bgfx::Memory* memory, uint16_t flags)
{
	bgfx::IndexBufferHandle handle = bgfx::createIndexBuffer(memory, flags);
	return handle.idx;
}

RFAPI void Graphics_DestroyIndexBuffer(IndexBuffer buffer)
{
	bgfx::destroy(bgfx::IndexBufferHandle{ buffer });
}

RFAPI DynamicIndexBuffer Graphics_CreateDynamicIndexBuffer(int indexCount, uint16_t flags)
{
	bgfx::DynamicIndexBufferHandle handle = bgfx::createDynamicIndexBuffer(indexCount, flags);
	return handle.idx;
}

RFAPI void Graphics_DestroyDynamicIndexBuffer(DynamicIndexBuffer buffer)
{
	bgfx::destroy(bgfx::DynamicIndexBufferHandle{ buffer });
}

RFAPI bool Graphics_CreateTransientIndexBuffer(int indexCount, bool index32, bgfx::TransientIndexBuffer* buffer)
{
	if (bgfx::getAvailTransientIndexBuffer(indexCount, index32) == indexCount)
	{
		bgfx::allocTransientIndexBuffer(buffer, indexCount, index32);
		return true;
	}
	return false;
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

RFAPI IndirectBuffer Graphics_CreateIndirectBuffer(int count)
{
	bgfx::IndirectBufferHandle handle = bgfx::createIndirectBuffer(count);
	return handle.idx;
}

RFAPI void Graphics_DestroyIndirectBuffer(IndirectBuffer buffer)
{
	bgfx::destroy(bgfx::IndirectBufferHandle{ buffer });
}

RFAPI Texture Graphics_CreateTextureMutable(int width, int height, bgfx::TextureFormat::Enum format, uint64_t flags, bgfx::TextureInfo* info)
{
	bgfx::TextureHandle handle = bgfx::createTexture2D(width, height, false, 1, format, flags);
	bgfx::calcTextureSize(*info, width, height, 0, false, false, 1, format);
	return handle.idx;
}

RFAPI Texture Graphics_CreateTextureMutableEx(int width, int height, bgfx::TextureFormat::Enum format, bool hasMips, int numLayers, uint64_t flags, bgfx::TextureInfo* info)
{
	bgfx::TextureHandle handle = bgfx::createTexture2D(width, height, hasMips, numLayers, format, flags);
	bgfx::calcTextureSize(*info, width, height, 0, false, hasMips, numLayers, format);
	return handle.idx;
}

RFAPI Texture Graphics_CreateTextureMutableR(bgfx::BackbufferRatio::Enum ratio, bool hasMips, bgfx::TextureFormat::Enum format, uint64_t flags, bgfx::TextureInfo* info)
{
	bgfx::TextureHandle handle = bgfx::createTexture2D(ratio, hasMips, 1, format, flags);
	bgfx::calcTextureSize(*info, info->width, info->height, info->depth, false, hasMips, 1, format);
	return handle.idx;
}

RFAPI void Graphics_SetTextureData(Texture texture, int x, int y, int width, int height, const bgfx::Memory* memory)
{
	bgfx::TextureHandle handle = { texture };
	bgfx::updateTexture2D(handle, 0, 0, x, y, width, height, memory);
}

RFAPI Texture Graphics_CreateTextureImmutable(int width, int height, bgfx::TextureFormat::Enum format, uint64_t flags, const bgfx::Memory* memory, bgfx::TextureInfo* info)
{
	bgfx::TextureHandle handle = bgfx::createTexture2D(width, height, false, 1, format, flags, memory);
	bgfx::calcTextureSize(*info, width, height, 0, false, false, 1, format);
	return handle.idx;
}

RFAPI Texture Graphics_CreateTextureFromMemory(const bgfx::Memory* memory, uint64_t flags, bgfx::TextureInfo* info)
{
	bgfx::TextureHandle handle = bgfx::createTexture(memory, flags, 0, info);
	return handle.idx;
}

RFAPI Texture Graphics_CreateTexture3DImmutable(int width, int height, int depth, bgfx::TextureFormat::Enum format, uint64_t flags, const bgfx::Memory* memory, bgfx::TextureInfo* info)
{
	bgfx::TextureHandle handle = bgfx::createTexture3D(width, height, depth, false, format, flags, memory);
	bgfx::calcTextureSize(*info, width, height, depth, false, false, 1, format);
	return handle.idx;
}

RFAPI Texture Graphics_CreateTexture3DMutable(int width, int height, int depth, bool hasMips, bgfx::TextureFormat::Enum format, uint64_t flags, bgfx::TextureInfo* info)
{
	bgfx::TextureHandle handle = bgfx::createTexture3D(width, height, depth, hasMips, format, flags);
	bgfx::calcTextureSize(*info, width, height, depth, false, hasMips, 1, format);
	return handle.idx;
}

RFAPI void Graphics_SetTexture3DData(Texture texture, int mip, int x, int y, int z, int width, int height, int depth, const bgfx::Memory* memory)
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

RFAPI Texture Graphics_CreateCubemapFromMemory(const bgfx::Memory* memory, uint64_t flags, bgfx::TextureInfo* info)
{
	bimg::ImageContainer* image = bimg::imageParse(Application_GetAllocator(), memory->data, memory->size);
	bgfx::calcTextureSize(*info, image->m_width, image->m_height, image->m_depth, image->m_cubeMap, image->m_numMips > 1, image->m_numLayers, (bgfx::TextureFormat::Enum)image->m_format);

	const bgfx::Memory* textureMemory = bgfx::makeRef(image->m_data, image->m_size, ImageReleaseCallback, image);

	bgfx::TextureHandle handle = bgfx::createTextureCube(image->m_width, image->m_numMips > 1, image->m_numLayers, (bgfx::TextureFormat::Enum)image->m_format, 0, textureMemory);
	return handle.idx;
}

RFAPI Texture Graphics_CreateCubemap(int size, bgfx::TextureFormat::Enum format, uint64_t flags, bgfx::TextureInfo* info)
{
	bgfx::TextureHandle handle = bgfx::createTextureCube(size, false, 1, format, flags, nullptr);
	if (info)
		bgfx::calcTextureSize(*info, size, size, 1, true, false, 1, format);
	return handle.idx;
}

RFAPI void Graphics_DestroyTexture(Texture texture)
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

RFAPI void Graphics_DestroyShader(Shader* shader)
{
	bgfx::destroy(shader->program);
	for (auto pair : shader->uniforms)
	{
		bgfx::destroy(pair.second);
	}
	BX_FREE(Application_GetAllocator(), shader);
}

RFAPI Uniform Graphics_ShaderGetUniform(Shader* shader, const char* name, bgfx::UniformType::Enum type, int num)
{
	bgfx::UniformHandle handle = shader->getUniform(name, type, num);
	return handle.idx;
}

RFAPI RenderTarget Graphics_CreateRenderTarget(int numAttachments, const RenderTargetAttachment* attachmentInfo, bgfx::TextureHandle* textures, bgfx::TextureInfo* textureInfos)
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
				return BGFX_INVALID_HANDLE;
			}

			if (attachmentInfo[i].ratio < bgfx::BackbufferRatio::Count)
				texture = bgfx::createTexture2D(attachmentInfo[i].ratio, false, 1, attachmentInfo[i].format, attachmentInfo[i].flags);
			else
				texture = bgfx::createTexture2D(attachmentInfo[i].width, attachmentInfo[i].height, false, 1, attachmentInfo[i].format, attachmentInfo[i].flags);

			textures[i] = texture;
			textureInfos[i] = bgfx::TextureInfo{ attachmentInfo[i].format, 0, (uint16_t)attachmentInfo[i].width, (uint16_t)attachmentInfo[i].height, 0, 1, 1, 0, false };
		}

		attachments[i].init(texture, bgfx::Access::Write, BGFX_CUBE_MAP_POSITIVE_X + attachmentInfo[i].textureLayer, attachmentInfo[i].isCubemap ? 1 : 1, 0, attachmentInfo[i].generateMipmaps ? BGFX_RESOLVE_AUTO_GEN_MIPS : BGFX_RESOLVE_NONE);
	}

	bgfx::FrameBufferHandle handle = bgfx::createFrameBuffer(numAttachments, attachments, attachmentInfo[0].texture.idx == bgfx::kInvalidHandle);
	if (!bgfx::isValid(handle))
	{
		Console_Error("Failed to validate framebuffer %i", (int)handle.idx);
		return BGFX_INVALID_HANDLE;
	}

	return handle.idx;
}

RFAPI void Graphics_DestroyRenderTarget(RenderTarget renderTarget)
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

RFAPI void Graphics_SetDepthWrite(bool write)
{
	state &= ~BGFX_STATE_WRITE_Z;
	if (write)
		state |= BGFX_STATE_WRITE_Z;
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

RFAPI void Graphics_SetVertexBuffer(VertexBuffer handle)
{
	bgfx::setVertexBuffer(0, bgfx::VertexBufferHandle{ handle });
}

RFAPI void Graphics_SetDynamicVertexBuffer(DynamicVertexBuffer handle)
{
	bgfx::setVertexBuffer(0, bgfx::DynamicVertexBufferHandle{ handle });
}

RFAPI void Graphics_SetTransientVertexBuffer(const bgfx::TransientVertexBuffer* buffer)
{
	bgfx::setVertexBuffer(0, buffer);
}

RFAPI void Graphics_SetIndexBuffer(IndexBuffer handle)
{
	bgfx::setIndexBuffer(bgfx::IndexBufferHandle{ handle });
}

RFAPI void Graphics_SetDynamicIndexBuffer(DynamicIndexBuffer handle)
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

RFAPI void Graphics_SetComputeBuffer(int stage, VertexBuffer handle, bgfx::Access::Enum access)
{
	bgfx::setBuffer(stage, bgfx::VertexBufferHandle{ handle }, access);
}

RFAPI void Graphics_SetUniform(Uniform handle, const void* value, int num)
{
	bgfx::setUniform(bgfx::UniformHandle{ handle }, value, num);
}

RFAPI void Graphics_SetTexture(Uniform sampler, int unit, Texture texture, uint32_t flags)
{
	bgfx::setTexture(unit, bgfx::UniformHandle{ sampler }, bgfx::TextureHandle{ texture }, flags);
}

RFAPI void Graphics_SetComputeTexture(int stage, Texture texture, int mip, bgfx::Access::Enum access)
{
	bgfx::setImage(stage, bgfx::TextureHandle{ texture }, mip, access);
}

RFAPI void Graphics_SetViewMode(int pass, int viewMode)
{
	bgfx::setViewMode((bgfx::ViewId)pass, (bgfx::ViewMode::Enum)viewMode);
}

RFAPI void Graphics_SetRenderTarget(int pass, RenderTarget handle, int width, int height)
{
	bgfx::setViewFrameBuffer((bgfx::ViewId)pass, bgfx::FrameBufferHandle{ handle });
	bgfx::setViewRect((bgfx::ViewId)pass, 0, 0, width, height);

	bgfx::touch((bgfx::ViewId)pass);
}

RFAPI void Graphics_SetRenderTargetR(int pass, RenderTarget handle, bgfx::BackbufferRatio::Enum ratio)
{
	bgfx::setViewFrameBuffer((bgfx::ViewId)pass, bgfx::FrameBufferHandle{ handle });
	bgfx::setViewRect((bgfx::ViewId)pass, 0, 0, ratio);
}

RFAPI void Graphics_ClearRenderTarget(int pass, RenderTarget handle, bool hasRGB, bool hasDepth, uint32_t rgba, float depth)
{
	bgfx::setViewClear((bgfx::ViewId)pass, (hasRGB ? BGFX_CLEAR_COLOR : 0) | (hasDepth ? BGFX_CLEAR_DEPTH : 0), rgba, depth, 0);

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

RFAPI void Graphics_DrawIndirect(int pass, Shader* shader, IndirectBuffer indirectBuffer, int start, int num)
{
	bgfx::submit((bgfx::ViewId)pass, shader->program, bgfx::IndirectBufferHandle{ indirectBuffer }, start, num);
}

RFAPI void Graphics_DrawText(int pass, int x, int y, float z, float scale, int viewportHeight, const char* text, int offset, int count, Font* font, uint32_t color, SpriteBatch* batch)
{
	font->drawText((bgfx::ViewId)pass, x, y, z, scale, viewportHeight, text, offset, count, color, batch);
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

RFAPI void Graphics_Blit(int pass, Texture dst, Texture src)
{
	bgfx::blit((bgfx::ViewId)pass, bgfx::TextureHandle{ dst }, 0, 0, bgfx::TextureHandle{ src }, 0, 0);
}

RFAPI void Graphics_BlitEx(int pass, Texture dst, int dstMip, int dstX, int dstY, int dstZ, Texture src, int srcMip, int srcX, int srcY, int srcZ, int width, int height, int depth)
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

static void MemoryString(int64_t mem, char* str)
{
	if (mem >= 1 << 30)
		sprintf(str, "%.2f GB", mem / (float)(1 << 30));
	else if (mem >= 1 << 20)
		sprintf(str, "%.2f MB", mem / (float)(1 << 20));
	else if (mem >= 1 << 10)
		sprintf(str, "%.2f KB", mem / (float)(1 << 10));
	else
		sprintf(str, "%lld B", mem);
}

static const char* MemoryString(int64_t mem)
{
	static char str[32];
	MemoryString(mem, str);
	return str;
}

RFAPI int Graphics_DrawDebugInfo(int x, int y, uint8_t color)
{
	const bgfx::Stats* stats = bgfx::getStats();

	bgfx::dbgTextPrintf(x, y++, BX_CONFIG_DEBUG ? 0xc : color
		, "%s / " BX_COMPILER_NAME
		" / " BX_CPU_NAME
		" / " BX_ARCH_NAME
		" / " BX_PLATFORM_NAME
		" / Version 1.%d"
		, bgfx::getRendererName(bgfx::getRendererType())
		, BGFX_API_VERSION
	);

	bgfx::dbgTextPrintf(x, y++, color, "%dx%d", stats->width, stats->height);
	bgfx::dbgTextPrintf(x, y++, color, "%.2f ms, %d fps", Application_GetMS(), Application_GetFPS());

	bgfx::dbgTextPrintf(x, y++, color, "CPU Frame: %.2f ms", (stats->cpuTimeEnd - stats->cpuTimeBegin) / (float)stats->cpuTimerFreq * 1000);
	bgfx::dbgTextPrintf(x, y++, color, "GPU Frame: %.2f ms", (stats->gpuTimeEnd - stats->gpuTimeBegin) / (float)stats->gpuTimerFreq * 1000);

	y++;

	bgfx::dbgTextPrintf(x, y++, color, "%d allocations", Application_GetNumAllocations());
	bgfx::dbgTextPrintf(x, y++, color, "RAM: %s", MemoryString(bx::getProcessMemoryUsed()));

	char gpuMemUsed[32];
	char gpuMemMax[32];
	MemoryString(stats->gpuMemoryUsed, gpuMemUsed);
	MemoryString(stats->gpuMemoryMax, gpuMemMax);
	bgfx::dbgTextPrintf(x, y++, color, "VRAM: %s/%s", gpuMemUsed, gpuMemMax);

	bgfx::dbgTextPrintf(x, y++, color, "VB: %d", stats->numVertexBuffers);
	bgfx::dbgTextPrintf(x, y++, color, "IB: %d", stats->numIndexBuffers);
	bgfx::dbgTextPrintf(x, y++, color, "Textures: %d, %s", stats->numTextures, MemoryString(stats->textureMemoryUsed));
	bgfx::dbgTextPrintf(x, y++, color, "RTs: %d, %s", stats->numFrameBuffers, MemoryString(stats->rtMemoryUsed));
	bgfx::dbgTextPrintf(x, y++, color, "Shaders : %d", stats->numPrograms);
	bgfx::dbgTextPrintf(x, y++, color, "Draw Calls: %d", stats->numDraw);
	bgfx::dbgTextPrintf(x, y++, color, "Triangles: %d", stats->numPrims[0]);
	bgfx::dbgTextPrintf(x, y++, color, "Computes: %d", stats->numCompute);
	bgfx::dbgTextPrintf(x, y++, color, "Blits: %d", stats->numBlit);

	return y;
}
