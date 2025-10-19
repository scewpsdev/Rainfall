#include "SpriteBatch.h"

#include "Rainfall.h"
#include "Application.h"
#include "Graphics.h"
#include "Shader.h"

#include "vector/Math.h"

#include <bx/allocator.h>


struct PosOffsetTexcoordColorVertex
{
	float x, y, z;
	float nx, ny, nz;
	float lightingMask;
	float u, v;
	float textureID;
	float mask;
	float r, g, b, a;


	static bgfx::VertexLayout layout;

	static void init()
	{
		static bool initialized = false;
		if (!initialized)
		{
			initialized = true;
			layout.begin().add(bgfx::Attrib::Position, 3, bgfx::AttribType::Float).add(bgfx::Attrib::Normal, 4, bgfx::AttribType::Float).add(bgfx::Attrib::TexCoord0, 4, bgfx::AttribType::Float).add(bgfx::Attrib::Color0, 4, bgfx::AttribType::Float).end();
		}
	}
};

bgfx::VertexLayout PosOffsetTexcoordColorVertex::layout;


SpriteBatch::SpriteBatch()
{
	PosOffsetTexcoordColorVertex::init();

	for (int i = 0; i < MAX_SPRITE_TEXTURES; i++)
	{
		char name[32] = "";
		sprintf(name, "u_texture%d", i);
		s_textures[i] = bgfx::createUniform(name, bgfx::UniformType::Sampler);
	}
}

SpriteBatch::~SpriteBatch()
{
	for (int i = 0; i < MAX_SPRITE_TEXTURES; i++)
	{
		bgfx::destroy(s_textures[i]);
	}
}

void SpriteBatch::begin(int numDrawCommands)
{
	textures.clear();
	textureFlags.clear();
	drawCalls.clear();

	vertexPtr = nullptr;
	indexPtr = nullptr;
	i = 0;

	vertexCount = numDrawCommands * 4;
	indexCount = numDrawCommands * 6;

	if (vertexCount > 0 && bgfx::getAvailTransientVertexBuffer(vertexCount, PosOffsetTexcoordColorVertex::layout) == vertexCount)
	{
		bgfx::allocTransientBuffers(&vertexBuffer, PosOffsetTexcoordColorVertex::layout, vertexCount, &indexBuffer, indexCount);

		vertexPtr = vertexBuffer.data;
		indexPtr = indexBuffer.data;
	}
	else
	{
		vertexCount = 0;
	}
}

void SpriteBatch::end()
{
	/*
	if (vertexCount > 0)
	{
		for (int i = 0; i < drawCalls.size(); i++)
		{
			bgfx::setState(state);

			bgfx::setVertexBuffer(0, &vertexBuffer);

			DrawCall2D& drawCall = drawCalls[i];
			bgfx::setIndexBuffer(&indexBuffer, drawCall.offset * 6, drawCall.count * 6);

			for (int j = drawCall.firstTexture; j < drawCall.firstTexture + MAX_SPRITE_TEXTURES; j++)
			{
				if (j == textures.size())
					break;

				bgfx::setTexture(j - drawCall.firstTexture, s_textures[j - drawCall.firstTexture], { textures[j] }, textureFlags[j]);
			}

			bgfx::submit(pass, shader);
		}
	}
	*/
}

int SpriteBatch::getNumDrawCalls()
{
	return vertexCount > 0 ? drawCalls.size : 0;
}

void SpriteBatch::submitDrawCall(int idx, bgfx::ViewId pass, bgfx::ProgramHandle shader)
{
	bgfx::setVertexBuffer(0, &vertexBuffer);

	DrawCall2D& drawCall = drawCalls[idx];
	bgfx::setIndexBuffer(&indexBuffer, drawCall.offset * 6, drawCall.count * 6);

	for (int i = 0; i < MAX_SPRITE_TEXTURES; i++)
	{
		int ii = drawCall.firstTexture + i;
		bgfx::TextureHandle texture = ii < textures.size ? bgfx::TextureHandle{ textures[ii] } : bgfx::TextureHandle BGFX_INVALID_HANDLE;
		uint32_t flags = ii < textures.size ? textureFlags[ii] : UINT32_MAX;
		bgfx::setTexture(i, s_textures[i], texture, flags);
	}

	bgfx::submit(pass, shader);
}

static int FindTextureInSameDrawCall(const List<DrawCall2D>& drawCalls, const List<uint16_t>& textures, uint16_t id)
{
	int offset = max(drawCalls.size - 1, 0) * MAX_SPRITE_TEXTURES;
	int end = min(offset + MAX_SPRITE_TEXTURES, textures.size);
	for (int i = offset; i < end; i++)
	{
		if (textures[i] == id)
			return i;
	}
	return -1;
}

void SpriteBatch::processDrawCommand(float x0, float y0, float z0, float x1, float y1, float z1, float x2, float y2, float z2, float x3, float y3, float z3,
	float nx0, float ny0, float nz0, float nx1, float ny1, float nz1, float nx2, float ny2, float nz2, float nx3, float ny3, float nz3, float lightingMask,
	float u0, float v0, float u1, float v1, float u2, float v2, float u3, float v3,
	float r, float g, float b, float a, float mask,
	uint16_t texture, uint32_t flags)
{
	if (!vertexPtr || !indexPtr)
		return;

	int textureIdx = -1;
	if (texture != bgfx::kInvalidHandle)
	{
		int idx = FindTextureInSameDrawCall(drawCalls, textures, texture);
		if (idx != -1)
			textureIdx = idx;
		else
		{
			textureIdx = textures.size;
			textures.add(texture);
			textureFlags.add(flags);
		}
	}

	if (drawCalls.size == 0 || textureIdx >= drawCalls.size * MAX_SPRITE_TEXTURES)
	{
		DrawCall2D drawCall = {};
		drawCall.offset = drawCalls.size == 0 ? 0 : (drawCalls.back().offset + drawCalls.back().count);
		drawCall.count = 0;
		drawCall.firstTexture = drawCalls.size == 0 ? 0 : (drawCalls.back().firstTexture + MAX_SPRITE_TEXTURES);
		drawCalls.add(drawCall);
	}
	drawCalls[drawCalls.size - 1].count++;

	if (textureIdx != -1)
		textureIdx -= drawCalls.back().firstTexture;


	PosOffsetTexcoordColorVertex* vertex = (PosOffsetTexcoordColorVertex*)vertexPtr;
	uint16_t* index = (uint16_t*)indexPtr;

	vertex->x = x0;
	vertex->y = y0;
	vertex->z = z0;
	vertex->nx = nx0;
	vertex->ny = ny0;
	vertex->nz = nz0;
	vertex->lightingMask = lightingMask;
	vertex->u = u0;
	vertex->v = v0;
	vertex->textureID = (float)textureIdx;
	vertex->mask = mask;
	vertex->r = r;
	vertex->g = g;
	vertex->b = b;
	vertex->a = a;
	vertex++;

	vertex->x = x1;
	vertex->y = y1;
	vertex->z = z1;
	vertex->nx = nx1;
	vertex->ny = ny1;
	vertex->nz = nz1;
	vertex->lightingMask = lightingMask;
	vertex->u = u1;
	vertex->v = v1;
	vertex->textureID = (float)textureIdx;
	vertex->mask = mask;
	vertex->r = r;
	vertex->g = g;
	vertex->b = b;
	vertex->a = a;
	vertex++;

	vertex->x = x2;
	vertex->y = y2;
	vertex->z = z2;
	vertex->nx = nx2;
	vertex->ny = ny2;
	vertex->nz = nz2;
	vertex->lightingMask = lightingMask;
	vertex->u = u2;
	vertex->v = v2;
	vertex->textureID = (float)textureIdx;
	vertex->mask = mask;
	vertex->r = r;
	vertex->g = g;
	vertex->b = b;
	vertex->a = a;
	vertex++;

	vertex->x = x3;
	vertex->y = y3;
	vertex->z = z3;
	vertex->nx = nx3;
	vertex->ny = ny3;
	vertex->nz = nz3;
	vertex->lightingMask = lightingMask;
	vertex->u = u3;
	vertex->v = v3;
	vertex->textureID = (float)textureIdx;
	vertex->mask = mask;
	vertex->r = r;
	vertex->g = g;
	vertex->b = b;
	vertex->a = a;
	vertex++;

	*(index++) = i * 4 + 0;
	*(index++) = i * 4 + 1;
	*(index++) = i * 4 + 2;

	*(index++) = i * 4 + 2;
	*(index++) = i * 4 + 3;
	*(index++) = i * 4 + 0;

	vertexPtr = (uint8_t*)vertex;
	indexPtr = (uint8_t*)index;
	i++;
}


RFAPI SpriteBatch* SpriteBatch_Create()
{
	return BX_NEW(Application_GetAllocator(), SpriteBatch);
}

RFAPI void SpriteBatch_Destroy(SpriteBatch* batch)
{
	BX_DELETE(Application_GetAllocator(), batch);
}

RFAPI void SpriteBatch_Begin(SpriteBatch* batch, int numDrawCommands)
{
	batch->begin(numDrawCommands);
}

RFAPI void SpriteBatch_End(SpriteBatch* batch)
{
	batch->end();
}

RFAPI int SpriteBatch_GetNumDrawCalls(SpriteBatch* batch)
{
	return batch->getNumDrawCalls();
}

RFAPI void SpriteBatch_SubmitDrawCall(SpriteBatch* batch, int idx, int pass, Shader* shader)
{
	batch->submitDrawCall(idx, (bgfx::ViewId)pass, shader->program);
}

RFAPI void SpriteBatch_Draw(SpriteBatch* batch,
	float x0, float y0, float z0, float x1, float y1, float z1, float x2, float y2, float z2, float x3, float y3, float z3,
	float nx0, float ny0, float nz0, float nx1, float ny1, float nz1, float nx2, float ny2, float nz2, float nx3, float ny3, float nz3, float lightingMask,
	float u0, float v0, float u1, float v1, float u2, float v2, float u3, float v3,
	float r, float g, float b, float a, float mask,
	uint16_t texture, uint32_t textureFlags)
{
	batch->processDrawCommand(x0, y0, z0, x1, y1, z1, x2, y2, z2, x3, y3, z3,
		nx0, ny0, nz0, nx1, ny1, nz1, nx2, ny2, nz2, nx3, ny3, nz3, lightingMask,
		u0, v0, u1, v1, u2, v2, u3, v3,
		r, g, b, a, mask,
		texture, textureFlags);
}
