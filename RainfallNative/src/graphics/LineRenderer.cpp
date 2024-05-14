#include "LineRenderer.h"

#include "Rainfall.h"
#include "Application.h"
#include "Shader.h"
#include "Graphics.h"

#include <bx/allocator.h>


struct PosColorVertex
{
	float x, y, z;
	Vector4 color;


	static bgfx::VertexLayout layout;

	static void init()
	{
		static bool initialized = false;
		if (!initialized)
		{
			initialized = true;
			layout.begin().add(bgfx::Attrib::Position, 3, bgfx::AttribType::Float).add(bgfx::Attrib::Color0, 4, bgfx::AttribType::Float).end();
		}
	}
};

bgfx::VertexLayout PosColorVertex::layout;


LineRenderer::LineRenderer()
{
	PosColorVertex::init();
}

void LineRenderer::begin(int numDrawCommands)
{
	vertexPtr = nullptr;
	indexPtr = nullptr;
	i = 0;

	vertexCount = numDrawCommands * 2;
	indexCount = numDrawCommands * 2;

	if (vertexCount > 0 && bgfx::getAvailTransientVertexBuffer(vertexCount, PosColorVertex::layout) == vertexCount)
	{
		bgfx::allocTransientBuffers(&vertexBuffer, PosColorVertex::layout, vertexCount, &indexBuffer, indexCount);

		vertexPtr = vertexBuffer.data;
		indexPtr = indexBuffer.data;
	}
	else
	{
		vertexCount = 0;
	}
}

void LineRenderer::end(bgfx::ViewId pass, bgfx::ProgramHandle shader)
{
	if (vertexCount > 0)
	{
		bgfx::setVertexBuffer(0, &vertexBuffer);
		bgfx::setIndexBuffer(&indexBuffer, 0, indexCount);
		Graphics_SetPrimitiveType(PrimitiveType::Lines);

		bgfx::submit(pass, shader);
	}
}

void LineRenderer::processDrawCommand(const LineDrawCommand& cmd)
{
	if (!vertexPtr || !indexPtr)
		return;

	Vector3 vertex0 = cmd.vertex0;
	Vector3 vertex1 = cmd.vertex1;
	Vector4 color = cmd.color;

	PosColorVertex* vertex = (PosColorVertex*)vertexPtr;
	uint16_t* index = (uint16_t*)indexPtr;

	vertex->x = vertex0.x;
	vertex->y = vertex0.y;
	vertex->z = vertex0.z;
	vertex->color = color;
	vertex++;

	vertex->x = vertex1.x;
	vertex->y = vertex1.y;
	vertex->z = vertex1.z;
	vertex->color = color;
	vertex++;

	*(index++) = i * 2 + 0;
	*(index++) = i * 2 + 1;

	vertexPtr = (uint8_t*)vertex;
	indexPtr = (uint8_t*)index;
	i++;
}

RFAPI LineRenderer* LineRenderer_Create()
{
	return BX_NEW(Application_GetAllocator(), LineRenderer);
}

RFAPI void LineRenderer_Destroy(LineRenderer* renderer)
{
	BX_DELETE(Application_GetAllocator(), renderer);
}

RFAPI void LineRenderer_Begin(LineRenderer* renderer, int numDrawCommands)
{
	renderer->begin(numDrawCommands);
}

RFAPI void LineRenderer_End(LineRenderer* renderer, int pass, Shader* shader)
{
	renderer->end((bgfx::ViewId)pass, shader->program);
}

RFAPI void LineRenderer_Draw(LineRenderer* renderer, Vector3 vertex0, Vector3 vertex1, Vector4 color)
{
	renderer->processDrawCommand({ vertex0, vertex1, color });
}
