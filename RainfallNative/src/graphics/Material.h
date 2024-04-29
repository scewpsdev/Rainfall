#pragma once

#include "Shader.h"

#include "vector/Vector.h"

#include <bgfx/bgfx.h>


struct Material
{
	Shader* shader;
	Vector4 materialData[4];
	bgfx::TextureHandle textures[5];
};
