#pragma once

#include "vector/Matrix.h"
#include "utils/List.h"

#include <bgfx/bgfx.h>

#include <vector>


#define MAX_SPRITE_TEXTURES 16


struct Sprite;
struct Light;

/*
struct DrawCommand2D
{
	bgfx::TextureHandle texture;
	uint32_t flags;
	Vector3 vertices[4];
	Vector3 normals[4];
	Vector2 uvs[4];
	Vector4 color;
};
*/

struct DrawCall2D
{
	int offset;
	int count;
	int firstTexture;
};

struct SpriteBatch
{
	List<uint16_t> textures;
	List<uint32_t> textureFlags;
	bgfx::UniformHandle s_textures[MAX_SPRITE_TEXTURES];
	List<DrawCall2D> drawCalls;

	bgfx::TransientVertexBuffer vertexBuffer = {};
	bgfx::TransientIndexBuffer indexBuffer = {};

	int vertexCount = 0, indexCount = 0;
	uint8_t* vertexPtr = nullptr;
	uint8_t* indexPtr = nullptr;
	int i = 0;


	SpriteBatch();
	~SpriteBatch();

	void begin(int numDrawCommands);
	void end();

	int getNumDrawCalls();
	void submitDrawCall(int idx, bgfx::ViewId pass, bgfx::ProgramHandle shader);

	void processDrawCommand(float x0, float y0, float z0, float x1, float y1, float z1, float x2, float y2, float z2, float x3, float y3, float z3,
		float nx0, float ny0, float nz0, float nx1, float ny1, float nz1, float nx2, float ny2, float nz2, float nx3, float ny3, float nz3, float lightingMask,
		float u0, float v0, float u1, float v1, float u2, float v2, float u3, float v3,
		float r, float g, float b, float a, float mask,
		uint16_t texture, uint32_t textureFlags);
};
