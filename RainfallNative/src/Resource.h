#pragma once

#include "Rainfall.h"

#include <bgfx/bgfx.h>


namespace bx
{
	struct FileReaderI;
}

namespace SoLoud
{
	class Wav;
}

struct Shader;
struct SceneData;

struct ShaderResource
{
	Shader* handle;
	uint32_t hash;
	int refCount = 0;
};

struct TextureResource
{
	bgfx::TextureHandle handle;
	bgfx::TextureInfo info;
	const bgfx::Memory* memory;
	uint32_t hash;
	int refCount = 0;
};

struct SceneResource
{
	SceneData* handle;
	uint32_t hash;
	int refCount = 0;
};

struct SoundResource
{
	SoLoud::Wav* handle;
	float length;
	uint32_t hash;
	int refCount = 0;
};

struct MiscResource
{
	char* data;
	int size;
	uint32_t hash;
	int refCount = 0;
};


const bgfx::Memory* ReadFileBinary(bx::FileReaderI* reader, const char* path);

RFAPI TextureResource* Resource_GetTexture(const char* path, uint64_t flags, bool cubemap);
RFAPI SceneResource* Resource_GetScene(const char* path, uint64_t textureFlags);
