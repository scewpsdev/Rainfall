#include "Geometry.h"

#include "Application.h"

#include <bx/allocator.h>


static TextureData* CreateTextureData(bgfx::TextureHandle handle)
{
	TextureData* texture = BX_NEW(Application_GetAllocator(), TextureData);
	texture->handle = handle;
	return texture;
}

RFAPI MaterialData Material_Create(uint32_t color, float metallicFactor, float roughnessFactor, const Vector3& emissiveColor, float emissiveStrength, uint16_t diffuse, uint16_t normal, uint16_t roughness, uint16_t metallic, uint16_t emissive)
{
	MaterialData material;

	material.color = color;
	material.metallicFactor = metallicFactor;
	material.roughnessFactor = roughnessFactor;
	material.emissiveColor = emissiveColor;
	material.emissiveStrength = emissiveStrength;

	material.diffuse = diffuse != bgfx::kInvalidHandle ? CreateTextureData(bgfx::TextureHandle{ diffuse }) : nullptr;
	material.normal = normal != bgfx::kInvalidHandle ? CreateTextureData(bgfx::TextureHandle{ normal }) : nullptr;
	material.roughness = roughness != bgfx::kInvalidHandle ? CreateTextureData(bgfx::TextureHandle{ roughness }) : nullptr;
	material.metallic = metallic != bgfx::kInvalidHandle ? CreateTextureData(bgfx::TextureHandle{ metallic }) : nullptr;
	material.emissive = emissive != bgfx::kInvalidHandle ? CreateTextureData(bgfx::TextureHandle{ emissive }) : nullptr;

	return material;
}

static void DestroyTextureData(TextureData* texture)
{
	BX_FREE(Application_GetAllocator(), texture);
}

RFAPI void Material_Destroy(MaterialData* material)
{
	if (material->diffuse)
	{
		DestroyTextureData(material->diffuse);
		material->diffuse = nullptr;
	}
	if (material->normal)
	{
		DestroyTextureData(material->normal);
		material->normal = nullptr;
	}
	if (material->roughness)
	{
		DestroyTextureData(material->roughness);
		material->roughness = nullptr;
	}
	if (material->metallic)
	{
		DestroyTextureData(material->metallic);
		material->metallic = nullptr;
	}
	if (material->emissive)
	{
		DestroyTextureData(material->emissive);
		material->emissive = nullptr;
	}
}
