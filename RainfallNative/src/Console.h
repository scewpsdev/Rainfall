#pragma once

#include "Rainfall.h"


void Console_SetErrorCallback(void(*callback)(const char* msg));

void Console_Log(const char* format, ...);
void Console_Error(const char* format, ...);
void Console_Warn(const char* format, ...);
