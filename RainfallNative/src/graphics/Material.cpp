#include "Material.h"

#include "Application.h"
#include "Geometry.h"

#include <bx/allocator.h>


extern Shader* defaultShader;


static TextureData* CreateTextureData(bgfx::TextureHandle handle)
{
	TextureData* texture = BX_NEW(Application_GetAllocator(), TextureData);
	texture->handle = handle;
	return texture;
}

RFAPI Material* Material_GetDefault()
{
	static Material material =
	{
		defaultShader,
		{
			Vector4(0, 0, 0, 0),
			Vector4(1, 1, 1, 0),
			Vector4(1, 0, 0, 0),
			Vector4(0, 0, 0, 0),
		},
		{
			BGFX_INVALID_HANDLE,
			BGFX_INVALID_HANDLE,
			BGFX_INVALID_HANDLE,
			BGFX_INVALID_HANDLE,
			BGFX_INVALID_HANDLE
		}
	};
	return &material;
}

RFAPI Material* Material_Create(uint32_t color, float metallicFactor, float roughnessFactor, const Vector3& emissiveColor, float emissiveStrength, uint16_t diffuse, uint16_t normal, uint16_t roughness, uint16_t metallic, uint16_t emissive)
{
	Material* material = BX_NEW(Application_GetAllocator(), Material) {};

	float r = ((color & 0x00FF0000) >> 16) / 255.0f;
	float g = ((color & 0x0000FF00) >> 8) / 255.0f;
	float b = ((color & 0x000000FF) >> 0) / 255.0f;
	float a = ((color & 0xFF000000) >> 24) / 255.0f;
	material->materialData[0] = Vector4(r, g, b, a);

	material->materialData[1].r = roughnessFactor;
	material->materialData[1].g = metallicFactor;
	material->materialData[2].rgb = emissiveColor;
	material->materialData[2].a = emissiveStrength;

	material->textures[0] = diffuse != bgfx::kInvalidHandle ? bgfx::TextureHandle{ diffuse } : bgfx::TextureHandle BGFX_INVALID_HANDLE;
	material->textures[1] = normal != bgfx::kInvalidHandle ? bgfx::TextureHandle{ normal } : bgfx::TextureHandle BGFX_INVALID_HANDLE;
	material->textures[2] = roughness != bgfx::kInvalidHandle ? bgfx::TextureHandle{ roughness } : bgfx::TextureHandle BGFX_INVALID_HANDLE;
	material->textures[3] = metallic != bgfx::kInvalidHandle ? bgfx::TextureHandle{ metallic } : bgfx::TextureHandle BGFX_INVALID_HANDLE;
	material->textures[4] = emissive != bgfx::kInvalidHandle ? bgfx::TextureHandle{ emissive } : bgfx::TextureHandle BGFX_INVALID_HANDLE;

	return material;
}

RFAPI void Material_Destroy(Material* material)
{
	for (int i = 0; i < 5; i++)
	{
		if (material->textures[i].idx != bgfx::kInvalidHandle)
			bgfx::destroy(material->textures[i]);
	}
	BX_FREE(Application_GetAllocator(), material);
}
