#include "Shader.h"

#include "Application.h"
#include "Resource.h"
#include "Console.h"
#include "Graphics.h"

#include "Hash.h"

#include <string.h>


bgfx::UniformHandle Shader::getUniform(const char* name, bgfx::UniformType::Enum type, int16_t num)
{
	auto it = uniforms.find(hash(name) + num);
	if (it == uniforms.end())
	{
		bgfx::UniformHandle uniform = bgfx::createUniform(name, type, num);
		uniforms.insert(std::make_pair(hash(name) + num, uniform));
		return uniform;
	}
	return it->second;
}

