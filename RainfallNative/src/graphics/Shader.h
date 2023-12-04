#pragma once

#include <bgfx/bgfx.h>
#include <tinystl/allocator.h>
#include <tinystl/unordered_map.h>

#include <tinystl/string.h>


struct Shader
{
	bgfx::ProgramHandle program;

	tinystl::unordered_map<uint32_t, bgfx::UniformHandle> uniforms;


	bgfx::UniformHandle getUniform(const char* name, bgfx::UniformType::Enum type, int16_t num = 1);
};
