#pragma once

#include "Rainfall.h"

#include "Shader.h"
#include "SpriteBatch.h"

#include <bgfx/bgfx.h>

#include <stdint.h>
#include <stb_truetype.h>


#define DEFAULT_FONT_ATLAS_WIDTH 1024
#define DEFAULT_FONT_ATLAS_HEIGHT 1024


struct FontInfo : stbtt_fontinfo {};
struct CharacterData : stbtt_bakedchar {};

struct FontData
{
	int bufferLen;
	uint8_t* bytes;
	FontInfo* info;
};

struct Font
{
	float size;
	FontData* data;

	CharacterData* characters = nullptr;
	int charOffset = -1;

	uint8_t* pixels;
	int atlasWidth, atlasHeight;

	bgfx::TextureHandle texture = BGFX_INVALID_HANDLE;


	Font(FontData* data, float size, bool antialiased, int atlasWidth = DEFAULT_FONT_ATLAS_WIDTH, int atlasHeight = DEFAULT_FONT_ATLAS_HEIGHT, int charOffset = ' ');

	int measureText(const char* str, int offset, int count);

	void drawText(bgfx::ViewId view, int x, int y, float z, float textScale, const char* text, int offset, int count, uint32_t color, SpriteBatch* batch);
};
