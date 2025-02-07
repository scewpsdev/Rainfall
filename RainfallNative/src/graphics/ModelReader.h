#pragma once


namespace bx
{
	struct FileReaderI;
}

struct SceneData;


bool ReadSceneData(bx::FileReaderI* reader, const char* path, SceneData& scene);
