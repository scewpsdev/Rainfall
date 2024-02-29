#pragma once

#include "vector/Matrix.h"

#include <bgfx/bgfx.h>


struct LineDrawCommand
{
	Vector3 vertex0;
	Vector3 vertex1;
	Vector4 color;
};

struct LineRenderer
{
	bgfx::TransientVertexBuffer vertexBuffer = {};
	bgfx::TransientIndexBuffer indexBuffer = {};

	int vertexCount = 0, indexCount = 0;
	uint8_t* vertexPtr = nullptr;
	uint8_t* indexPtr = nullptr;
	int i = 0;


	LineRenderer();

	void begin(int numDrawCommands);
	void end(bgfx::ViewId pass, bgfx::ProgramHandle shader);
	void processDrawCommand(const LineDrawCommand& cmd);
};
