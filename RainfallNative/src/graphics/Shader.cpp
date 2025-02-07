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

Shader* Shader_Create(const char* vertexPath, const char* fragmentPath)
{
	const bgfx::Memory* vertexMemory = ReadFileBinary(Application_GetFileReader(), vertexPath);
	const bgfx::Memory* fragmentMemory = ReadFileBinary(Application_GetFileReader(), fragmentPath);

	if (!vertexMemory)
		Console_Error("Failed to read vertex shader '%s'", vertexPath);
	if (!fragmentMemory)
		Console_Error("Failed to read fragment shader '%s'", fragmentPath);

	if (vertexMemory && fragmentMemory)
		return Graphics_CreateShader(vertexMemory, fragmentMemory);

	return nullptr;
}

Shader* Shader_CreateCompute(const char* computePath)
{
	const bgfx::Memory* computeMemory = ReadFileBinary(Application_GetFileReader(), computePath);

	if (!computeMemory)
		Console_Error("Failed to read compute shader '%s'", computePath);

	if (computeMemory)
		return Graphics_CreateShaderCompute(computeMemory);

	return nullptr;
}
