#pragma once

#include "Rainfall.h"

#include <stdarg.h>


void Console_SetErrorCallback(void(*callback)(const char* msg));

void Console_LogV(const char* format, va_list args);
void Console_Log(const char* format, ...);
void Console_LogLn(const char* format, ...);
void Console_Error(const char* format, ...);
void Console_Warn(const char* format, ...);
