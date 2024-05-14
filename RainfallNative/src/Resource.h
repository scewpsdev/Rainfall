#pragma once

#include "Rainfall.h"

#include "graphics/Shader.h"
#include "graphics/Geometry.h"

#include <bgfx/bgfx.h>


namespace bx
{
	struct FileReaderI;
}


const bgfx::Memory* ReadFileBinary(bx::FileReaderI* reader, const char* path);

RFAPI uint16_t Resource_CreateTexture2DFromFile(const char* path, uint64_t flags, bgfx::TextureInfo* info);
RFAPI SceneData* Resource_CreateSceneDataFromFile(const char* path, uint64_t textureFlags);
