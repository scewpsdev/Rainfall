#include "Shader.h"

#include "Hash.h"

#include <string.h>


bgfx::UniformHandle Shader::getUniform(const char* name, bgfx::UniformType::Enum type, int16_t num)
{
	auto it = uniforms.find(hash(name));
	if (it == uniforms.end())
	{
		bgfx::UniformHandle uniform = bgfx::createUniform(name, type, num);
		uniforms.insert(std::make_pair(hash(name), uniform));
		return uniform;
	}
	return it->second;
}
