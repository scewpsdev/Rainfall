#pragma once

#include "Rainfall.h"

#include "vector/Vector.h"

#include <bgfx/bgfx.h>


struct Shader;
struct MaterialData;
struct SceneData;

struct Material
{
	Shader* shader;
	bool isForward;
	Vector4 materialData[4];
	bgfx::TextureHandle textures[6];
};


void CreateSceneMaterials(SceneData* scene);

RFAPI Material* Material_CreateDeferred(uint32_t color, float metallicFactor, float roughnessFactor, const Vector3& emissiveColor, float emissiveStrength, uint16_t diffuse, uint16_t normal, uint16_t roughness, uint16_t metallic, uint16_t emissive, uint16_t height);
RFAPI Material* Material_GetForData(MaterialData* materialData);
Material* Material_GetDefault();
