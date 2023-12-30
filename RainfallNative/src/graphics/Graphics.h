#pragma once

#include "Rainfall.h"

#include "Shader.h"

#include <bgfx/bgfx.h>


RFAPI Shader* Graphics_CreateShader(const bgfx::Memory* vertexMemory, const bgfx::Memory* fragmentMemory);
RFAPI Shader* Graphics_CreateShaderCompute(const bgfx::Memory* computeMemory);

RFAPI uint16_t Graphics_CreateTextureFromMemory(const bgfx::Memory* memory, uint64_t flags, bgfx::TextureInfo* info);
RFAPI uint16_t Graphics_CreateCubemapFromMemory(const bgfx::Memory* memory, uint64_t flags, bgfx::TextureInfo* info);

RFAPI uint64_t Graphics_GetState();
