#pragma once

#include "Geometry.h"

#include <bx/readerwriter.h>


bool ReadSceneData(bx::FileReaderI* reader, const char* path, SceneData& scene);
