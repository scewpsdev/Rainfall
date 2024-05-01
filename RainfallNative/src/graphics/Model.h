#pragma once

#include "Application.h"
#include "Geometry.h"
#include "Shader.h"
#include "Animation.h"

#include "vector/Matrix.h"

#include <bgfx/bgfx.h>


void InitializeScene(SceneData& scene, const char* scenePath, uint64_t textureFlags);

RFAPI SceneData* Model_Create(int numVertices, PositionNormalTangent* vertices, Vector2* uvs, int numIndices, int* indices);
