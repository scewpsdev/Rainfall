#pragma once

#include "Rainfall.h"

#include <unordered_map>

#include <bgfx/bgfx.h>


struct Shader
{
	bgfx::ProgramHandle program;

	std::unordered_map<uint32_t, bgfx::UniformHandle> uniforms;

	bgfx::UniformHandle getUniform(const char* name, bgfx::UniformType::Enum type, int16_t num = 1);
};


Shader* Shader_Create(const char* vertexPath, const char* fragmentPath);
Shader* Shader_CreateCompute(const char* computePath);
