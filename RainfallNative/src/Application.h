#pragma once

#include "Rainfall.h"
#include "Input.h"

#include <stdint.h>


namespace bx
{
	struct AllocatorI;
	struct FileReaderI;
}


bx::AllocatorI* Application_GetAllocator();

RFAPI int64_t Application_GetTimestamp();
RFAPI int64_t Application_GetCurrentTime();
RFAPI float Application_GetFrameTime();
RFAPI int Application_GetFPS();
RFAPI float Application_GetMS();
RFAPI int64_t Application_GetMemoryUsage();
RFAPI int Application_GetNumAllocations();

RFAPI CursorMode Application_GetCursorMode();
