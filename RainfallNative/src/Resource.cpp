#include "Resource.h"

#include "Rainfall.h"
#include "Application.h"
#include "Console.h"

#include "graphics/Shader.h"
#include "graphics/Model.h"
#include "graphics/ModelReader.h"
#include "graphics/Graphics.h"
#include "graphics/Font.h"
#include "graphics/Material.h"

#include "Hash.h"

#include <stdio.h>
#include <string.h>
#include <map>
#include <filesystem>

#include <bimg/decode.h>
#include <stb_truetype.h>

#include <bx/bx.h>
#include <bx/file.h>
//#include <stb_vorbis.h>

#include <soloud.h>
#include <soloud_wav.h>

#include <zlib.h>


namespace fs = std::filesystem;


struct ResourceHeaderElement
{
	int pathLen;
	char* path;
	int offset;
	int size;
	int decompressedSize;
};

struct ResourcePackage
{
	// Header
	int numResources;
	ResourceHeaderElement* headerElements;
	std::map<uint32_t, int> pathIdx;
	int dataBlockOffset;

	bx::FileReader* reader;
};

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

static List<ResourcePackage> packages;


void ResourceInit()
{
}

void ResourceTerminate()
{
	if (!loadedShaders.empty() || !loadedTextures.empty() || !loadedScenes.empty() || !loadedSounds.empty() && !loadedMiscs.empty())
	{
		Console_Warn("Leaked resources!");
		for (auto pair : loadedShaders)
		{
			Console_Warn("%s, %s: %d refs", pair.second->vertex ? pair.second->vertex : "-", pair.second->fragment ? pair.second->fragment : "-", pair.second->refCount);
		}
		for (auto pair : loadedTextures)
		{
			Console_Warn("%s: %d refs", pair.second->path ? pair.second->path : "-", pair.second->refCount);
		}
		for (auto pair : loadedScenes)
		{
			Console_Warn("%s: %d refs", pair.second->path ? pair.second->path : "-", pair.second->refCount);
		}
		for (auto pair : loadedSounds)
		{
			Console_Warn("%s: %d refs", pair.second->path ? pair.second->path : "-", pair.second->refCount);
		}
		for (auto pair : loadedMiscs)
		{
			Console_Warn("%s: %d refs", pair.second->path ? pair.second->path : "-", pair.second->refCount);
		}
	}
}

static uint32_t hashPath(const char* _path)
{
	std::string s = _path;
	std::transform(s.begin(), s.end(), s.begin(), [](unsigned char c) { return c == '\\' ? '/' : c; });

	size_t idx = std::string::npos;
	while ((idx = s.find(std::string("../"))) != std::string::npos)
	{
		std::string first = s.substr(0, idx - 1);
		size_t prevSlash = first.find_last_of('/');
		if (prevSlash != std::string::npos)
		{
			s = first.substr(0, prevSlash + 1) + s.substr(idx + 3);
		}
		else
		{
			break;
		}
	}

	const char* path = s.c_str();
	return hash(path);
}

ResourcePackage* GetPackageForFile(const char* path, int* outIdx)
{
	uint32_t h = hashPath(path);
	for (int i = 0; i < packages.size; i++)
	{
		ResourcePackage* package = &packages[i];
		auto it = package->pathIdx.find(h);
		if (it != package->pathIdx.end())
		{
			*outIdx = it->second;
			return package;
		}
	}
	return nullptr;
}

const bgfx::Memory* ReadPackageFile(ResourcePackage* package, int idx)
{
	ResourceHeaderElement* element = &package->headerElements[idx];
	bx::seek(package->reader, package->dataBlockOffset + element->offset, bx::Whence::Begin);

	int size = element->size;
	if (element->decompressedSize != size)
	{
		// decompress

		char* data = (char*)BX_ALLOC(Application_GetAllocator(), size);
		bx::read(package->reader, data, size, bx::ErrorAssert{});

		const bgfx::Memory* memory = bgfx::alloc(element->decompressedSize);
		uLongf decompressedSize = memory->size;
		int result = uncompress((Bytef*)memory->data, &decompressedSize, (const Bytef*)data, size);
		if (result != Z_OK)
		{
			Console_Error("Decompression failed (%d): %s", result, element->path);
			//BX_FREE(Application_GetAllocator(), decompressedData);
			return nullptr;
		}

		BX_FREE(Application_GetAllocator(), data);

		return memory;
	}
	else
	{
		const bgfx::Memory* memory = bgfx::alloc(size);
		bx::read(package->reader, memory->data, size, bx::ErrorAssert{});
		return memory;
	}
}

char* ReadPackageFile(ResourcePackage* package, int idx, int* outSize)
{
	ResourceHeaderElement* element = &package->headerElements[idx];
	bx::seek(package->reader, package->dataBlockOffset + element->offset, bx::Whence::Begin);

	int size = element->size;
	char* data = (char*)BX_ALLOC(Application_GetAllocator(), size);
	bx::read(package->reader, data, size, bx::ErrorAssert{});

	if (element->decompressedSize != size)
	{
		// decompress
		uLongf decompressedSize = element->decompressedSize;
		char* decompressedData = (char*)BX_ALLOC(Application_GetAllocator(), decompressedSize);
		int result = uncompress((Bytef*)decompressedData, &decompressedSize, (const Bytef*)data, size);
		if (result != Z_OK)
		{
			Console_Error("Decompression failed (%d): %s", result, element->path);
			BX_FREE(Application_GetAllocator(), decompressedData);
			return nullptr;
		}

		BX_FREE(Application_GetAllocator(), data);
		data = decompressedData;
		size = decompressedSize;
	}

	*outSize = size;
	return data;
}

const bgfx::Memory* ReadFileBinary(const char* path)
{
	int packageResIdx;
	if (ResourcePackage* package = GetPackageForFile(path, &packageResIdx))
	{
		return ReadPackageFile(package, packageResIdx);
	}
	else
	{
		char compiledPath[256];
		sprintf(compiledPath, "%s.bin", path);

		bx::FileReader reader;
		if (bx::open(&reader, compiledPath))
		{
			uint32_t size = (uint32_t)bx::getSize(&reader);

			const bgfx::Memory* memory = bgfx::alloc(size + 1);
			bx::read(&reader, memory->data, size, bx::ErrorAssert{});
			bx::close(&reader);
			memory->data[memory->size - 1] = '\0';

			return memory;
		}
	}

	return nullptr;
}

RFAPI void Resource_LoadPackageHeader(const char* path)
{
	bx::Error err;
	bx::FileReader* reader = BX_NEW(Application_GetAllocator(), bx::FileReader);
	if (bx::open(reader, path))
	{
		ResourcePackage& package = packages.add();
		package.reader = reader;

		bx::read(reader, package.numResources, &err);
		package.headerElements = (ResourceHeaderElement*)BX_ALLOC(Application_GetAllocator(), package.numResources * sizeof(ResourceHeaderElement));

		for (int i = 0; i < package.numResources; i++)
		{
			bx::read(reader, package.headerElements[i].pathLen, &err);
			package.headerElements[i].path = (char*)BX_ALLOC(Application_GetAllocator(), package.headerElements[i].pathLen + 1);
			bx::read(reader, package.headerElements[i].path, package.headerElements[i].pathLen, &err);
			package.headerElements[i].path[package.headerElements[i].pathLen] = 0;
			bx::read(reader, package.headerElements[i].offset, &err);
			bx::read(reader, package.headerElements[i].size, &err);
			bx::read(reader, package.headerElements[i].decompressedSize, &err);

			uint32_t h = hashPath(package.headerElements[i].path);
			package.pathIdx.emplace(h, i);
		}

		package.dataBlockOffset = (int)reader->seek();
	}
}

Shader* ReadShader(const char* vertexPath, const char* fragmentPath)
{
	const bgfx::Memory* vertexMemory = ReadFileBinary(vertexPath);
	const bgfx::Memory* fragmentMemory = ReadFileBinary(fragmentPath);

	if (!vertexMemory)
		Console_Error("Failed to read vertex shader '%s'", vertexPath);
	if (!fragmentMemory)
		Console_Error("Failed to read fragment shader '%s'", fragmentPath);

	if (vertexMemory && fragmentMemory)
		return Graphics_CreateShader(vertexMemory, fragmentMemory);

	return nullptr;
}

Shader* ReadShaderCompute(const char* computePath)
{
	const bgfx::Memory* computeMemory = ReadFileBinary(computePath);

	if (!computeMemory)
		Console_Error("Failed to read compute shader '%s'", computePath);

	if (computeMemory)
		return Graphics_CreateShaderCompute(computeMemory);

	return nullptr;
}

bgfx::TextureHandle ReadTexture(const char* path, uint64_t flags, bgfx::TextureInfo* info, char** outData, int* outSize)
{
	if (const bgfx::Memory* memory = ReadFileBinary(path))
	{
		if (outData && outSize)
		{
			*outData = (char*)BX_ALLOC(Application_GetAllocator(), memory->size);
			*outSize = memory->size;
			memcpy(*outData, memory->data, memory->size);
		}

		return { Graphics_CreateTextureFromMemory(memory, flags, info) };
	}

	Console_Error("Failed to read texture '%s'", path);
	return BGFX_INVALID_HANDLE;
}

static bgfx::TextureHandle ReadCubemap(const char* path, int64_t flags, bgfx::TextureInfo* info, char** outData, int* outSize)
{
	if (const bgfx::Memory* memory = ReadFileBinary(path))
	{
		if (outData && outSize)
		{
			*outData = (char*)BX_ALLOC(Application_GetAllocator(), memory->size);
			*outSize = memory->size;
			memcpy(*outData, memory->data, memory->size);
		}

		return { Graphics_CreateCubemapFromMemory(memory, flags, info) };
	}

	Console_Error("Failed to read cubemap '%s'", path);
	return BGFX_INVALID_HANDLE;
}

SceneData* ReadScene(const char* path, uint64_t textureFlags)
{
	int packageResIdx;
	if (ResourcePackage* package = GetPackageForFile(path, &packageResIdx))
	{
		int size;
		char* data = ReadPackageFile(package, packageResIdx, &size);

		bx::MemoryReader reader(data, size);

		SceneData* scene = BX_NEW(Application_GetAllocator(), SceneData);
		ReadSceneData(&reader, *scene);

		InitializeScene(*scene, path, textureFlags);
		CreateSceneMaterials(scene);

		BX_FREE(Application_GetAllocator(), data);

		return scene;
	}
	else
	{
		char compiledPath[256];
		sprintf(compiledPath, "%s.bin", path);

		bx::FileReader reader;
		if (bx::open(&reader, compiledPath))
		{
			SceneData* scene = BX_NEW(Application_GetAllocator(), SceneData);
			ReadSceneData(&reader, *scene);
			bx::close(&reader);

			InitializeScene(*scene, path, textureFlags);
			CreateSceneMaterials(scene);

			return scene;
		}
	}

	Console_Error("Failed to read model '%s'", path);
	return nullptr;
}

static SoLoud::Wav* ReadSound(const char* path, float* outLength)
{
	int packageResIdx;
	if (ResourcePackage* package = GetPackageForFile(path, &packageResIdx))
	{
		int size;
		char* data = ReadPackageFile(package, packageResIdx, &size);

		SoLoud::Wav* wav = BX_NEW(Application_GetAllocator(), SoLoud::Wav);
		if (!wav->loadMem((uint8_t*)data, size, true, true))
		{
			*outLength = (float)wav->getLength();
			return wav;
		}
	}
	else
	{
		char compiledPath[256];
		sprintf(compiledPath, "%s.bin", path);

		SoLoud::Wav* wav = BX_NEW(Application_GetAllocator(), SoLoud::Wav);
		if (!wav->load(compiledPath))
		{
			*outLength = (float)wav->getLength();
			return wav;
		}
	}

	Console_Error("Failed to read sound file '%s'", path);
	return nullptr;
}

static char* ReadBinary(const char* path, int* outSize)
{
	int packageResIdx;
	if (ResourcePackage* package = GetPackageForFile(path, &packageResIdx))
	{
		return ReadPackageFile(package, packageResIdx, outSize);
	}
	else
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

	if (Shader* shader = ReadShader(vertex, fragment))
	{
		ShaderResource* resource = BX_NEW(Application_GetAllocator(), ShaderResource);
		resource->handle = shader;
		resource->hash = h;
		resource->vertex = _strdup(vertex);
		resource->fragment = _strdup(fragment);
		resource->refCount = 1;
		loadedShaders.emplace(h, resource);
		return resource;
	}

	return nullptr;
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

	if (Shader* shader = ReadShaderCompute(compute))
	{
		ShaderResource* resource = BX_NEW(Application_GetAllocator(), ShaderResource);
		resource->handle = shader;
		resource->hash = h;
		resource->vertex = _strdup(compute);
		resource->fragment = nullptr;
		resource->refCount = 1;
		loadedShaders.emplace(h, resource);
		return resource;
	}

	return nullptr;
}

RFAPI bool Resource_FreeShader(ShaderResource* resource)
{
	auto it = loadedShaders.find(resource->hash);
	BX_ASSUME(it != loadedShaders.end());

	resource->refCount--;
	if (resource->refCount == 0)
	{
		Graphics_DestroyShader(resource->handle);
		if (resource->vertex)
			free(resource->vertex);
		if (resource->fragment)
			free(resource->fragment);
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

RFAPI TextureResource* Resource_GetTexture(const char* path, uint64_t flags, bool cubemap, bool keepCPUData)
{
	uint32_t h = hash(path);
	auto it = loadedTextures.find(h);
	if (it != loadedTextures.end())
	{
		it->second->refCount++;
		return it->second;
	}

	char* data = nullptr;
	int size = 0;

	bgfx::TextureInfo info;
	bgfx::TextureHandle handle = cubemap ? ReadCubemap(path, flags, &info, keepCPUData ? &data : nullptr, &size) : ReadTexture(path, flags, &info, keepCPUData ? &data : nullptr, &size);
	if (handle.idx != bgfx::kInvalidHandle)
	{
		TextureResource* resource = BX_NEW(Application_GetAllocator(), TextureResource);
		resource->handle = handle;
		resource->info = info;
		resource->data = data;
		resource->size = size;
		resource->hash = h;
		resource->path = _strdup(path);
		resource->refCount = 1;
		loadedTextures.emplace(h, resource);
		return resource;
	}

	return nullptr;
}

RFAPI bool Resource_FreeTexture(TextureResource* resource)
{
	auto it = loadedTextures.find(resource->hash);
	BX_ASSUME(it != loadedTextures.end());

	resource->refCount--;
	if (resource->refCount == 0)
	{
		Graphics_DestroyTexture(resource->handle.idx);
		if (resource->path)
			free(resource->path);
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
	if (resource->data)
	{
		if (bimg::ImageContainer* image = bimg::imageParse(Application_GetAllocator(), resource->data, resource->size))
		{
			imageData->image = image;
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

RFAPI void Resource_TextureFreeCPUData(TextureResource* resource)
{
	if (resource->data)
	{
		BX_FREE(Application_GetAllocator(), resource->data);
		resource->data = nullptr;
	}
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

	if (SceneData* scene = ReadScene(path, textureFlags))
	{
		SceneResource* resource = BX_NEW(Application_GetAllocator(), SceneResource);
		resource->handle = scene;
		resource->hash = h;
		resource->path = _strdup(path);
		resource->refCount = 1;
		loadedScenes.emplace(h, resource);
		return resource;
	}

	return nullptr;
}

RFAPI bool Resource_FreeScene(SceneResource* resource)
{
	auto it = loadedScenes.find(resource->hash);
	BX_ASSUME(it != loadedScenes.end());

	resource->refCount--;
	if (resource->refCount == 0)
	{
		Model_Destroy(resource->handle);
		if (resource->path)
			free(resource->path);
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

	float length;
	if (SoLoud::Wav* sound = ReadSound(path, &length))
	{
		SoundResource* resource = BX_NEW(Application_GetAllocator(), SoundResource);
		resource->handle = sound;
		resource->length = length;
		resource->hash = h;
		resource->path = _strdup(path);
		resource->refCount = 1;
		loadedSounds.emplace(h, resource);
		return resource;
	}

	return nullptr;
}

RFAPI bool Resource_FreeSound(SoundResource* resource)
{
	auto it = loadedSounds.find(resource->hash);
	BX_ASSUME(it != loadedSounds.end());

	resource->refCount--;
	if (resource->refCount == 0)
	{
		BX_FREE(Application_GetAllocator(), resource->handle);
		if (resource->path)
			free(resource->path);
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

	int size = 0;
	if (char* data = ReadBinary(path, &size))
	{
		MiscResource* resource = BX_NEW(Application_GetAllocator(), MiscResource);
		resource->data = data;
		resource->size = size;
		resource->hash = h;
		resource->path = _strdup(path);
		resource->refCount = 1;
		loadedMiscs.emplace(h, resource);
		return resource;
	}

	return nullptr;
}

RFAPI bool Resource_FreeMisc(MiscResource* resource)
{
	auto it = loadedMiscs.find(resource->hash);
	BX_ASSUME(it != loadedMiscs.end());

	resource->refCount--;
	if (resource->refCount == 0)
	{
		BX_FREE(Application_GetAllocator(), resource->data);
		if (resource->path)
			free(resource->path);
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
