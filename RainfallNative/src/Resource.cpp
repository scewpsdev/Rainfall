#include "Resource.h"

#include "Rainfall.h"
#include "Application.h"
#include "Console.h"

#include "graphics/Shader.h"
#include "graphics/Model.h"
#include "graphics/ModelReader.h"
#include "graphics/Graphics.h"
#include "graphics/Font.h"

#include "Hash.h"

#include <stdio.h>
#include <string.h>
#include <map>

#include <bimg/decode.h>
#include <stb_truetype.h>

#include <bx/bx.h>
#include <bx/file.h>
//#include <stb_vorbis.h>

#include <soloud.h>
#include <soloud_wav.h>


struct ImageData
{
	bimg::ImageContainer* image;

	void* data;
	uint32_t size;

	bimg::TextureFormat::Enum format;
	uint32_t width, height;
};


static std::map<uint32_t, ShaderResource*> loadedShaders;
static std::map<uint32_t, TextureResource*> loadedTextures;
static std::map<uint32_t, SceneResource*> loadedScenes;
static std::map<uint32_t, SoundResource*> loadedSounds;
static std::map<uint32_t, MiscResource*> loadedMiscs;


const bgfx::Memory* ReadFileBinary(bx::FileReaderI* reader, const char* path)
{
	char compiledPath[256];
	sprintf(compiledPath, "%s.bin", path);

	if (bx::open(reader, compiledPath))
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

static bgfx::TextureHandle ReadTexture(const char* path, uint64_t flags, bgfx::TextureInfo* info, const bgfx::Memory** outMem)
{
	if (const bgfx::Memory* memory = ReadFileBinary(Application_GetFileReader(), path))
	{
		*outMem = memory;
		return { Graphics_CreateTextureFromMemory(memory, flags, info) };
	}

	Console_Error("Failed to read texture '%s'", path);
	return BGFX_INVALID_HANDLE;
}

static bgfx::TextureHandle ReadCubemap(const char* path, int64_t flags, bgfx::TextureInfo* info, const bgfx::Memory** outMem)
{
	if (const bgfx::Memory* memory = ReadFileBinary(Application_GetFileReader(), path))
	{
		*outMem = memory;
		return { Graphics_CreateCubemapFromMemory(memory, flags, info) };
	}

	Console_Error("Failed to read cubemap '%s'", path);
	return BGFX_INVALID_HANDLE;
}

static SceneData* ReadScene(const char* path, uint64_t textureFlags)
{
	SceneData* scene = BX_NEW(Application_GetAllocator(), SceneData);
	if (ReadSceneData(Application_GetFileReader(), path, *scene))
	{
		InitializeScene(*scene, path, textureFlags);
		return scene;
	}

	Console_Error("Failed to read model '%s'", path);
	bx::free(Application_GetAllocator(), scene);
	return nullptr;
}

static SoLoud::Wav* ReadSound(const char* path, float* outLength)
{
	char compiledPath[256];
	sprintf(compiledPath, "%s.bin", path);

	SoLoud::Wav* wav = BX_NEW(Application_GetAllocator(), SoLoud::Wav);
	if (wav->load(compiledPath))
	{
		Console_Error("Failed to read sound file '%s'", path);
		return nullptr;
	}

	*outLength = (float)wav->getLength();

	return wav;
}

static char* ReadBinary(const char* path, int* outSize)
{
	char compiledPath[256];
	sprintf(compiledPath, "%s.bin", path);

	bx::FileReader reader;
	if (bx::open(&reader, compiledPath))
	{
		int size = (int)bx::getSize(&reader);
		char* buffer = (char*)BX_ALLOC(Application_GetAllocator(), size + 1);
		bx::read(&reader, buffer, size, bx::ErrorAssert{});
		bx::close(&reader);
		buffer[size] = '\0';
		*outSize = size;
		return buffer;
	}
	return nullptr;
}

RFAPI ShaderResource* Resource_GetShader(const char* vertex, const char* fragment)
{
	uint32_t h = hashCombine(hash(vertex), hash(fragment));
	auto it = loadedShaders.find(h);
	if (it != loadedShaders.end())
	{
		it->second->refCount++;
		return it->second;
	}

	ShaderResource* resource = BX_NEW(Application_GetAllocator(), ShaderResource);
	resource->handle = Shader_Create(vertex, fragment);
	resource->hash = h;
	resource->refCount = 1;
	loadedShaders.emplace(h, resource);
	return resource;
}

RFAPI ShaderResource* Resource_GetShaderCompute(const char* compute)
{
	uint32_t h = hash(compute);
	auto it = loadedShaders.find(h);
	if (it != loadedShaders.end())
	{
		it->second->refCount++;
		return it->second;
	}

	ShaderResource* resource = BX_NEW(Application_GetAllocator(), ShaderResource);
	resource->handle = Shader_CreateCompute(compute);
	resource->hash = h;
	resource->refCount = 1;
	loadedShaders.emplace(h, resource);
	return resource;
}

RFAPI bool Resource_FreeShader(ShaderResource* resource)
{
	auto it = loadedShaders.find(resource->hash);
	if (it == loadedShaders.end())
		__debugbreak();

	resource->refCount--;
	if (resource->refCount == 0)
	{
		Graphics_DestroyShader(resource->handle);
		loadedShaders.erase(it);
		BX_FREE(Application_GetAllocator(), resource);
		return false;
	}

	return true;
}

RFAPI Shader* Resource_ShaderGetHandle(ShaderResource* resource)
{
	return resource->handle;
}

RFAPI TextureResource* Resource_GetTexture(const char* path, uint64_t flags, bool cubemap)
{
	uint32_t h = hash(path);
	auto it = loadedTextures.find(h);
	if (it != loadedTextures.end())
	{
		it->second->refCount++;
		return it->second;
	}

	TextureResource* resource = BX_NEW(Application_GetAllocator(), TextureResource);
	resource->handle = cubemap ? ReadCubemap(path, flags, &resource->info, &resource->memory) : ReadTexture(path, flags, &resource->info, &resource->memory);
	resource->hash = h;
	resource->refCount = 1;
	loadedTextures.emplace(h, resource);
	return resource;
}

RFAPI bool Resource_FreeTexture(TextureResource* resource)
{
	auto it = loadedTextures.find(resource->hash);
	if (it == loadedTextures.end())
		__debugbreak();

	resource->refCount--;
	if (resource->refCount == 0)
	{
		Graphics_DestroyTexture(resource->handle.idx);
		loadedTextures.erase(it);
		BX_FREE(Application_GetAllocator(), resource);
		return false;
	}

	return true;
}

RFAPI uint16_t Resource_TextureGetHandle(TextureResource* resource)
{
	return resource->handle.idx;
}

RFAPI bgfx::TextureInfo* Resource_TextureGetInfo(TextureResource* resource)
{
	return &resource->info;
}

RFAPI bool Resource_TextureGetImage(TextureResource* resource, ImageData* imageData)
{
	if (bimg::ImageContainer* image = bimg::imageParse(Application_GetAllocator(), resource->memory->data, resource->memory->size))
	{
		imageData->image = image;
		imageData->data = image->m_data;
		imageData->size = image->m_size;
		imageData->format = image->m_format;
		imageData->width = image->m_width;
		imageData->height = image->m_height;

		return true;
	}
	return false;
}

RFAPI void Resource_FreeImage(bimg::ImageContainer* image)
{
	bimg::imageFree(image);
}

RFAPI SceneResource* Resource_GetScene(const char* path, uint64_t textureFlags)
{
	uint32_t h = hash(path);
	auto it = loadedScenes.find(h);
	if (it != loadedScenes.end())
	{
		it->second->refCount++;
		return it->second;
	}

	SceneResource* resource = BX_NEW(Application_GetAllocator(), SceneResource);
	resource->handle = ReadScene(path, textureFlags);
	resource->hash = h;
	resource->refCount = 1;
	loadedScenes.emplace(h, resource);
	return resource;
}

RFAPI bool Resource_FreeScene(SceneResource* resource)
{
	auto it = loadedScenes.find(resource->hash);
	if (it == loadedScenes.end())
		__debugbreak();

	resource->refCount--;
	if (resource->refCount == 0)
	{
		Model_Destroy(resource->handle);
		loadedScenes.erase(it);
		BX_FREE(Application_GetAllocator(), resource);
		return false;
	}

	return true;
}

RFAPI SceneData* Resource_SceneGetHandle(SceneResource* resource)
{
	return resource->handle;
}

RFAPI SoundResource* Resource_GetSound(const char* path)
{
	uint32_t h = hash(path);
	auto it = loadedSounds.find(h);
	if (it != loadedSounds.end())
	{
		it->second->refCount++;
		return it->second;
	}

	SoundResource* resource = BX_NEW(Application_GetAllocator(), SoundResource);
	resource->handle = ReadSound(path, &resource->length);
	resource->hash = h;
	resource->refCount = 1;
	loadedSounds.emplace(h, resource);
	return resource;
}

RFAPI bool Resource_FreeSound(SoundResource* resource)
{
	auto it = loadedSounds.find(resource->hash);
	if (it == loadedSounds.end())
		__debugbreak();

	resource->refCount--;
	if (resource->refCount == 0)
	{
		BX_FREE(Application_GetAllocator(), resource->handle);
		loadedSounds.erase(it);
		BX_FREE(Application_GetAllocator(), resource);
		return false;
	}

	return true;
}

RFAPI SoLoud::Wav* Resource_SoundGetHandle(SoundResource* resource)
{
	return resource->handle;
}

RFAPI float Resource_SoundGetDuration(SoundResource* resource)
{
	return resource->length;
}

RFAPI MiscResource* Resource_GetMisc(const char* path)
{
	uint32_t h = hash(path);
	auto it = loadedMiscs.find(h);
	if (it != loadedMiscs.end())
	{
		it->second->refCount++;
		return it->second;
	}

	MiscResource* resource = BX_NEW(Application_GetAllocator(), MiscResource);
	resource->data = ReadBinary(path, &resource->size);
	resource->hash = h;
	resource->refCount = 1;
	loadedMiscs.emplace(h, resource);
	return resource;
}

RFAPI bool Resource_FreeMisc(MiscResource* resource)
{
	auto it = loadedMiscs.find(resource->hash);
	if (it == loadedMiscs.end())
		__debugbreak();

	resource->refCount--;
	if (resource->refCount == 0)
	{
		BX_FREE(Application_GetAllocator(), resource->data);
		loadedMiscs.erase(it);
		BX_FREE(Application_GetAllocator(), resource);
		return false;
	}

	return true;
}

RFAPI char* Resource_MiscGetData(MiscResource* resource, int* outSize)
{
	*outSize = resource->size;
	return resource->data;
}
