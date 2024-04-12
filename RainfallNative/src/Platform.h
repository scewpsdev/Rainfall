#pragma once

#include "Rainfall.h"

#include <stdint.h>


RFAPI void Platform_Init();
RFAPI void Platform_Terminate();

RFAPI int64_t Application_GetTimestamp();
RFAPI void Application_SleepFor(int millis);
RFAPI void Application_SleepForAccurate(int nanos);
