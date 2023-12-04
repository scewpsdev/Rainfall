#pragma once

#include "Geometry.h"

#include <bx/readerwriter.h>


void WriteSceneData(bx::FileWriterI* writer, const SceneData& scene, const char* out);
