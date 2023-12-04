#include "Console.h"

#include <stdio.h>
#include <stdarg.h>


static void(*errorCallback)(const char* msg);


void Console_SetErrorCallback(void(*callback)(const char* msg))
{
	errorCallback = callback;
}

void Console_Error(const char* format, ...)
{
	/*
	va_list args;
	va_start(args, &format);
	vfprintf(stderr, format, args);
	fprintf(stderr, "\n");
	va_end(args);
	*/

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
