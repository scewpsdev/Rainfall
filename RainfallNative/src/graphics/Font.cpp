#include "Font.h"

#include "Application.h"
#include "Resource.h"

#include <stb_truetype.h>
#include <bgfx/bgfx.h>
#include <bx/allocator.h>

#include <string.h>
#include <math.h>

#define STB_TRUETYPE_IMPLEMENTATION
#include <stb_truetype.h>


RFAPI FontData* FontData_Create(char* data, int size)
{
	FontData* fontData = BX_NEW(Application_GetAllocator(), FontData);

	fontData->bufferLen = size;
	fontData->bytes = (uint8_t*)data;

	if (!stbtt_InitFont(&fontData->info, (uint8_t*)data, 0))
		return {};

	return fontData;
}

RFAPI void FontData_Destroy(FontData* fontData)
{
	BX_FREE(Application_GetAllocator(), fontData);
}

RFAPI Font* Resource_CreateFontFromData(FontData* data, float size, bool antialiased)
{
	Font* font = BX_NEW(Application_GetAllocator(), Font)(data, size, antialiased);
	return font;
}

RFAPI int Resource_FontMeasureText(Font* font, const char* text, int offset, int count)
{
	return font->measureText(text, offset, count);
}


Font::Font(FontData* data, float size, bool antialiased, int atlasWidth, int atlasHeight, int charOffset)
	: data(data), size(size), atlasWidth(atlasWidth), atlasHeight(atlasHeight), charOffset(charOffset)
{
	pixels = (uint8_t*)BX_ALLOC(Application_GetAllocator(), atlasWidth * atlasHeight);

	int numChars = 255 - charOffset;
	characters = (stbtt_bakedchar*)BX_ALLOC(Application_GetAllocator(), sizeof(stbtt_bakedchar) * numChars);
	stbtt_BakeFontBitmap(data->bytes, 0, size, pixels, atlasWidth, atlasHeight, charOffset, numChars, characters);

	texture = bgfx::createTexture2D(atlasWidth, atlasHeight, false, 1, bgfx::TextureFormat::R8, antialiased ? 0 : BGFX_SAMPLER_POINT, bgfx::makeRef(pixels, atlasWidth * atlasHeight));
}

int Font::measureText(const char* text, int offset, int count)
{
	float textScale = 1.0f;
	float scale = stbtt_ScaleForPixelHeight(&data->info, size * textScale);

	int ascent, descent, lineGap;
	stbtt_GetFontVMetrics(&data->info, &ascent, &descent, &lineGap);
	ascent = (int)roundf(ascent * scale);

	float advance = 0;
	for (int i = offset; i < offset + count; i++)
	{
		int advanceWidth, leftSideBearing;
		stbtt_GetCodepointHMetrics(&data->info, text[i], &advanceWidth, &leftSideBearing);

		int kerning = stbtt_GetCodepointKernAdvance(&data->info, text[i], text[i + 1]);
		if (i == offset + count - 1)
			kerning = 0;
		advance += roundf((advanceWidth - kerning + leftSideBearing) * scale);
	}

	return (int)advance;
}

void Font::drawText(bgfx::ViewId view, int x, int y, float z, float textScale, int viewportHeight, const char* text, int offset, int count, uint32_t color, SpriteBatch* batch)
{
	float scale = stbtt_ScaleForPixelHeight(&data->info, this->size * textScale);

	int ascent, descent, lineGap;
	stbtt_GetFontVMetrics(&data->info, &ascent, &descent, &lineGap);
	ascent = (int)roundf(ascent * scale);

	float advance = 0;
	for (int i = offset; i < offset + count; i++)
	{
		if (text[i] == '\n')
		{
			y += (int)ceilf(this->size * textScale);
			advance = 0;
			continue;
		}

		int advanceWidth, leftSideBearing;
		stbtt_GetCodepointHMetrics(&data->info, text[i], &advanceWidth, &leftSideBearing);

		//int x0, x1, y0, y1;
		//stbtt_GetCodepointBitmapBox(this->data->info, text[i], scale, scale, &x0, &y0, &x1, &y1);

		float xpos = 0.0f, ypos = 0.0f;
		int charIndex = text[i] - this->charOffset;
		stbtt_aligned_quad quad;
		stbtt_GetBakedQuad((const stbtt_bakedchar*)this->characters, this->atlasWidth, this->atlasHeight, charIndex, &xpos, &ypos, &quad, 1);

		quad.s0 += 0.00001f;
		quad.t0 += 0.00001f;
		quad.s1 -= 0.00001f;
		quad.t1 -= 0.00001f;

		float width = (quad.x1 - quad.x0) * textScale;
		float height = (quad.y1 - quad.y0) * textScale;

		float xx = x + advance + leftSideBearing * scale;
		float yy = (float)(viewportHeight - y - (ascent + quad.y1 * textScale) - 1);

		uint8_t r = (color & 0xFF0000) >> 16;
		uint8_t g = (color & 0xFF00) >> 8;
		uint8_t b = (color & 0xFF);
		uint8_t a = (color & 0xFF000000) >> 24;

		uint16_t texture = this->texture.idx;
		uint32_t flags = UINT32_MAX;
		Vector3 vertex0 = Vector3(xx, yy, z);
		Vector3 vertex1 = Vector3(xx + width, yy, z);
		Vector3 vertex2 = Vector3(xx + width, yy + height, z);
		Vector3 vertex3 = Vector3(xx, yy + height, z);
		Vector3 normal0 = Vector3(0.0f);
		Vector3 normal1 = Vector3(0.0f);
		Vector3 normal2 = Vector3(0.0f);
		Vector3 normal3 = Vector3(0.0f);
		Vector2 uv0 = Vector2(quad.s0, quad.t1);
		Vector2 uv1 = Vector2(quad.s1, quad.t1);
		Vector2 uv2 = Vector2(quad.s1, quad.t0);
		Vector2 uv3 = Vector2(quad.s0, quad.t0);
		Vector4 color = Vector4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);

		batch->processDrawCommand(
			vertex0.x,
			vertex0.y,
			vertex0.z,
			vertex1.x,
			vertex1.y,
			vertex1.z,
			vertex2.x,
			vertex2.y,
			vertex2.z,
			vertex3.x,
			vertex3.y,
			vertex3.z,
			normal0.x,
			normal0.y,
			normal0.z,
			normal1.x,
			normal1.y,
			normal1.z,
			normal2.x,
			normal2.y,
			normal2.z,
			normal3.x,
			normal3.y,
			normal3.z,
			1.0f,
			uv0.x,
			uv0.y,
			uv1.x,
			uv1.y,
			uv2.x,
			uv2.y,
			uv3.x,
			uv3.y,
			color.x,
			color.y,
			color.z,
			color.w,
			1.0f,
			texture, flags);

		int kerning = stbtt_GetCodepointKernAdvance(&data->info, text[i], text[i + 1]);
		advance += roundf((advanceWidth - kerning + leftSideBearing) * scale);
	}
}
