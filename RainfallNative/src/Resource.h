#pragma once

#include <bgfx/bgfx.h>


namespace bx
{
	struct FileReaderI;
}


const bgfx::Memory* ReadFileBinary(bx::FileReaderI* reader, const char* path);
