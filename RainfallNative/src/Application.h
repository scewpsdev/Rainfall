#pragma once

#include "Rainfall.h"

#include <stdint.h>


namespace bx
{
	struct AllocatorI;
	struct FileReaderI;
}


bx::AllocatorI* Application_GetAllocator();
bx::FileReaderI* Application_GetFileReader();

RFAPI int64_t Application_GetTimestamp();
RFAPI int64_t Application_GetCurrentTime();
RFAPI int64_t Application_GetFrameTime();
RFAPI int Application_GetFPS();
RFAPI float Application_GetMS();
RFAPI int64_t Application_GetMemoryUsage();
RFAPI int Application_GetNumAllocations();

RFAPI bool Application_IsMouseLocked();

inline float Application_GetDelta()
{
	return Application_GetFrameTime() / 1e9f;
}
