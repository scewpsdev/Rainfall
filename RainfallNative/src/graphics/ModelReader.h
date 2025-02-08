#pragma once


namespace bx
{
	struct ReaderI;
}

struct SceneData;


void ReadSceneData(bx::ReaderI* reader, SceneData& scene);
