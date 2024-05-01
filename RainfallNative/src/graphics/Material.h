#pragma once

#include "Shader.h"
#include "Geometry.h"

#include "vector/Vector.h"

#include <bgfx/bgfx.h>


struct Material
{
	Shader* shader;
	Vector4 materialData[4];
	bgfx::TextureHandle textures[5];
};


RFAPI Material* Material_Create(uint32_t color, float metallicFactor, float roughnessFactor, const Vector3& emissiveColor, float emissiveStrength, uint16_t diffuse, uint16_t normal, uint16_t roughness, uint16_t metallic, uint16_t emissive);
RFAPI Material* Material_GetForData(MaterialData* materialData);
Material* Material_GetDefault();
