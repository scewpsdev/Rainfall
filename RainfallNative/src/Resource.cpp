#include "Resource.h"

#include "Rainfall.h"
#include "Application.h"
#include "Console.h"

#include "graphics/Shader.h"
#include "graphics/Model.h"
#include "graphics/ModelReader.h"
#include "graphics/Graphics.h"
#include "graphics/Font.h"

#include <stdio.h>
#include <string.h>

#include <bx/file.h>
#include <bimg/decode.h>
#include <stb_truetype.h>
//#include <stb_vorbis.h>

#include <soloud.h>
#include <soloud_wav.h>


struct ImageData
{
	void* data;
	uint32_t size;

	bimg::TextureFormat::Enum format;
	uint32_t width, height;
};


const bgfx::Memory* ReadFileBinary(bx::FileReaderI* reader, const char* path)
{
	if (bx::open(reader, path))
	{
		uint32_t size = (uint32_t)bx::getSize(reader);
		const bgfx::Memory* memory = bgfx::alloc(size + 1);
		bx::read(reader, memory->data, size, bx::ErrorAssert{});
		bx::close(reader);
		memory->data[memory->size - 1] = '\0';
		return memory;
	}

	return nullptr;
}


RFAPI Shader* Resource_CreateShader(const char* vertexPath, const char* fragmentPath)
{
	//printf("Reading shaders '%s', '%s'\n", vertexPath, fragmentPath);

	const bgfx::Memory* vertexMemory = ReadFileBinary(Application_GetFileReader(), vertexPath);
	const bgfx::Memory* fragmentMemory = ReadFileBinary(Application_GetFileReader(), fragmentPath);

	if (!vertexMemory)
		Console_Error("Failed to read vertex shader '%s'", vertexPath);
	if (!fragmentMemory)
		Console_Error("Failed to read fragment shader '%s'", fragmentPath);

	if (vertexMemory && fragmentMemory)
		return Graphics_CreateShader(vertexMemory, fragmentMemory);

	return nullptr;
}

RFAPI Shader* Resource_CreateShaderCompute(const char* computePath)
{
	const bgfx::Memory* computeMemory = ReadFileBinary(Application_GetFileReader(), computePath);

	if (!computeMemory)
		Console_Error("Failed to read compute shader '%s'", computePath);

	if (computeMemory)
		return Graphics_CreateShaderCompute(computeMemory);

	return nullptr;
}

RFAPI bimg::ImageContainer* Resource_ReadImageFromFile(const char* path, bgfx::TextureInfo* info)
{
	if (const bgfx::Memory* memory = ReadFileBinary(Application_GetFileReader(), path))
	{
		bx::Error err;
		if (bimg::ImageContainer* image = bimg::imageParseKtx(Application_GetAllocator(), memory->data, memory->size, &err))
		{
			bgfx::calcTextureSize(*info, image->m_width, image->m_height, image->m_depth, image->m_cubeMap, image->m_numMips, image->m_numLayers, (bgfx::TextureFormat::Enum)image->m_format);
			return image;
		}
	}
	return nullptr;
}

RFAPI void* Resource_ImageGetData(bimg::ImageContainer* image)
{
	return image->m_data;
}

RFAPI void Resource_FreeImage(bimg::ImageContainer* image)
{
	bimg::imageFree(image);
}

RFAPI uint16_t Resource_CreateTexture2DFromFile(const char* path, uint64_t flags, bgfx::TextureInfo* info)
{
	//printf("Reading texture '%s'\n", path);

	if (const bgfx::Memory* memory = ReadFileBinary(Application_GetFileReader(), path))
	{
		return Graphics_CreateTextureFromMemory(memory, flags, info);
	}

	Console_Error("Failed to read texture '%s'", path);
	return bgfx::kInvalidHandle;
}

RFAPI bool Resource_ReadImageData(const char* path, ImageData* imageData)
{
	if (const bgfx::Memory* memory = ReadFileBinary(Application_GetFileReader(), path))
	{
		if (bimg::ImageContainer* image = bimg::imageParse(Application_GetAllocator(), memory->data, memory->size))
		{
			imageData->data = image->m_data;
			imageData->size = image->m_size;
			imageData->format = image->m_format;
			imageData->width = image->m_width;
			imageData->height = image->m_height;
			return true;
		}
	}
	return false;
}

RFAPI uint16_t Resource_CreateCubemapFromFile(const char* path, int64_t flags, bgfx::TextureInfo* info)
{
	//printf("Reading cubemap '%s'\n", path);

	if (const bgfx::Memory* memory = ReadFileBinary(Application_GetFileReader(), path))
	{
		return Graphics_CreateCubemapFromMemory(memory, flags, info);
	}

	Console_Error("Failed to read cubemap '%s'", path);
	return bgfx::kInvalidHandle;
}

RFAPI SceneData* Resource_CreateSceneDataFromFile(const char* path, uint64_t textureFlags)
{
	//printf("Reading model '%s'\n", path);

	SceneData* scene = BX_NEW(Application_GetAllocator(), SceneData);
	if (ReadSceneData(Application_GetFileReader(), path, *scene))
	{
		InitializeScene(*scene, path, textureFlags);
		return scene;
	}

	Console_Error("Failed to read model '%s'", path);
	BX_FREE(Application_GetAllocator(), scene);
	return nullptr;
}

RFAPI Model* Resource_CreateModelFromSceneData(SceneData* scene)
{
	Model* model = BX_NEW(Application_GetAllocator(), Model)(scene);
	return model;
}

RFAPI SceneData* Resource_ModelGetSceneData(Model* model)
{
	return model->lod0;
}

RFAPI FontData* Resource_CreateFontDataFromFile(const char* path)
{
	//printf("Reading font '%s'\n", path);

	FontData* data = BX_NEW(Application_GetAllocator(), FontData);

	if (const bgfx::Memory* memory = ReadFileBinary(Application_GetFileReader(), path))
	{
		data->bufferLen = memory->size;
		data->bytes = memory->data;

		data->info = BX_NEW(Application_GetAllocator(), FontInfo);
		if (!stbtt_InitFont(data->info, data->bytes, 0))
			return {};

		return data;
	}

	Console_Error("Failed to read font '%s'", path);
	return nullptr;
}

RFAPI Font* Resource_CreateFontFromData(FontData* data, float size, bool antialiased)
{
	Font* font = BX_NEW(Application_GetAllocator(), Font)(data, size, antialiased);
	return font;
}

RFAPI int Resource_FontMeasureText(Font* font, const char* text, int length)
{
	return font->measureText(text, length);
}

RFAPI SoLoud::Wav* Resource_CreateSoundFromFile(const char* path, float* outFloat)
{
	/*
	int error = 0;
	stb_vorbis* vorbis = stb_vorbis_open_filename(path, &error, NULL);
	if (error)
	{
		Console_Error("Failed to read sound file '%s'", path);
		return nullptr;
	}

	stb_vorbis_info info = stb_vorbis_get_info(vorbis);

	int sampleRate = info.sample_rate;
	int bps = 16;
	int samples = stb_vorbis_stream_length_in_samples(vorbis);

	int size = samples * info.channels * sizeof(short);
	short* buffer = (short*)BX_ALLOC(Application_GetAllocator(), size);

	stb_vorbis_get_samples_short_interleaved(vorbis, info.channels, buffer, samples * info.channels);

	return BX_NEW(Application_GetAllocator(), Sound)(buffer, size, info.channels, sampleRate, bps);
	*/

	SoLoud::Wav* wav = new SoLoud::Wav();
	if (wav->load(path))
	{
		Console_Error("Failed to read sound file '%s'", path);
		return nullptr;
	}

	*outFloat = (float)wav->getLength();

	return wav;
}
