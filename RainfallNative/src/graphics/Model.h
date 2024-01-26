#pragma once

#include "Application.h"
#include "Geometry.h"
#include "Shader.h"
#include "Animation.h"

#include "vector/Matrix.h"

#include <bgfx/bgfx.h>


struct Model : SceneData
{
	SceneData* lod0;
	float maxDistance = FLT_MAX;


	Model(SceneData* scene);

	void drawMesh(int id, bgfx::ViewId view, Shader* shader, const Matrix& transform);
	void drawMeshAnimated(int id, bgfx::ViewId view, Shader* animatedShader, AnimationState* animationState, const Matrix& transform);
	void draw(bgfx::ViewId view, Shader* shader, Shader* animatedShader, AnimationState* animationState, const Matrix& transform);

	AnimationData* getAnimation(const char* name) const;
};


void InitializeScene(SceneData& scene, const char* scenePath, uint64_t textureFlags);

RFAPI Model* Model_Create(int numVertices, PositionNormalTangent* vertices, Vector2* uvs, int numIndices, int* indices, MaterialData* material);
RFAPI void Model_ConfigureLODs(Model* model, float maxDistance);
