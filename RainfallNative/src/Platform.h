#pragma once

#include "Rainfall.h"

#include <stdint.h>


RFAPI void Platform_Init();
RFAPI void Platform_Terminate();

RFAPI int64_t Platform_GetTimestamp();
RFAPI void Platform_SleepFor(int millis);
RFAPI void Platform_SleepForAccurate(int nanos);
