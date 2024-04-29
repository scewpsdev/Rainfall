#pragma once

#include "Rainfall.h"

#include <bgfx/bgfx.h>
#include <tinystl/allocator.h>
#include <tinystl/unordered_map.h>

#include <tinystl/string.h>
#include <unordered_map>


struct Shader
{
	bgfx::ProgramHandle program;

	std::unordered_map<uint32_t, bgfx::UniformHandle> uniforms;

	bgfx::UniformHandle getUniform(const char* name, bgfx::UniformType::Enum type, int16_t num = 1);
};


RFAPI Shader* Shader_Create(const char* vertexPath, const char* fragmentPath);
RFAPI Shader* Shader_CreateCompute(const char* computePath);
RFAPI void Shader_Destroy(Shader* shader);
