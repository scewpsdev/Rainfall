#pragma once

#include "vector/Vector.h"
#include "vector/Quaternion.h"

#include <bgfx/bgfx.h>


namespace nv::cloth
{
	class Cloth;
	class Fabric;
}

struct Cloth
{
	nv::cloth::Cloth* cloth;
	nv::cloth::Fabric* fabric;
	struct MeshData* mesh;

	Vector3 position;
	Quaternion rotation;
	bgfx::DynamicVertexBufferHandle animatedPosition;
	bgfx::DynamicVertexBufferHandle animatedNormalTangent;
};


void Physics_ClothInit();
void Physics_ClothTerminate();
void Physics_ClothUpdate(float delta);
