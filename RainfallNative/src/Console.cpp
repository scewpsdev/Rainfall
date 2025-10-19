#include "Console.h"

#include <stdio.h>


#define CSI "\x1b["
#define COLOR(c) CSI c "m"

#define RESET "0"
#define RESET_COLOR "27"
#define RESET_FG "39"
#define RESET_BG "49"
#define BLACK "30"
#define RED "31"
#define GREEN "32"
#define YELLOW "33"
#define BLUE "34"
#define MAGENTA "35"
#define CYAN "36"
#define WHITE "37"


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

	fputs(COLOR(RED), stderr);

	va_list args;
	va_start(args, &format);
	vsprintf(buffer, format, args);
	fprintf(stderr, "\n");
	va_end(args);

	fputs(COLOR(RED), stderr);

	errorCallback(buffer);

	fputs(COLOR(RESET), stderr);
}

RFAPI void Console_ErrorStr(const char* str)
{
	Console_Error("%s", str);
}

void Console_Warn(const char* format, ...)
{
	fputs(COLOR(YELLOW), stderr);

	va_list args;
	va_start(args, &format);
	vfprintf(stderr, format, args);
	fprintf(stderr, "\n");
	va_end(args);

	fputs(COLOR(RESET), stderr);
}

RFAPI void Console_WarnStr(const char* str)
{
	Console_Warn("%s", str);
}
