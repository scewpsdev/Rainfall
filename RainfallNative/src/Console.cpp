#include "Console.h"

#include <stdio.h>


static void(*errorCallback)(const char* msg);


void Console_SetErrorCallback(void(*callback)(const char* msg))
{
	errorCallback = callback;
}

void Console_LogV(const char* format, va_list args)
{
#if _DEBUG
	vfprintf(stdout, format, args);
#endif
}

void Console_Log(const char* format, ...)
{
#if _DEBUG
	va_list args;
	va_start(args, &format);
	Console_LogV(format, args);
	va_end(args);
#endif
}

void Console_LogLn(const char* format, ...)
{
#if _DEBUG
	va_list args;
	va_start(args, &format);
	Console_LogV(format, args);
	fprintf(stdout, "\n");
	va_end(args);
#endif
}

void Console_Error(const char* format, ...)
{
	static char buffer[256];

	va_list args;
	va_start(args, &format);
	vsprintf(buffer, format, args);
	fprintf(stderr, "\n");
	va_end(args);

	errorCallback(buffer);
}

void Console_Warn(const char* format, ...)
{
	va_list args;
	va_start(args, &format);
	vfprintf(stderr, format, args);
	fprintf(stderr, "\n");
	va_end(args);
}
