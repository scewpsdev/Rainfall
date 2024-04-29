#pragma once

#include "Rainfall.h"

#include "graphics/Model.h"
#include "graphics/Material.h"

#include "vector/Matrix.h"


RFAPI void Renderer3D_Init(int width, int height);
RFAPI void Renderer3D_Resize(int width, int height);
RFAPI void Renderer3D_Terminate();
RFAPI void Renderer3D_SetCamera(Vector3 position, Quaternion rotation, Matrix proj);
RFAPI void Renderer3D_DrawModel(SceneData* scene, Matrix transform, Material* material);
RFAPI void Renderer3D_Begin();
RFAPI void Renderer3D_End();
