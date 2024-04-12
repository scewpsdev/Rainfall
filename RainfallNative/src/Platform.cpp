#include "Platform.h"

#include <Windows.h>


static LARGE_INTEGER appStartTime;


RFAPI void Platform_Init()
{
	timeBeginPeriod(1);

	QueryPerformanceCounter(&appStartTime);
}

RFAPI void Platform_Terminate()
{
	timeEndPeriod(1);
}

RFAPI int64_t Application_GetTimestamp()
{
	LARGE_INTEGER t;
	QueryPerformanceCounter(&t);

	LARGE_INTEGER m_high_perf_timer_freq;
	QueryPerformanceFrequency(&m_high_perf_timer_freq);

	return (t.QuadPart - appStartTime.QuadPart) * 1000000000 / m_high_perf_timer_freq.QuadPart;
}

RFAPI void Application_SleepFor(int millis)
{
	Sleep(millis);
}

RFAPI void Application_SleepForAccurate(int nanos)
{
	// https://www.geisswerks.com/ryan/FAQS/timing.html
	// note: BE SURE YOU CALL timeBeginPeriod(1) at program startup!!!
	// note: BE SURE YOU CALL timeEndPeriod(1) at program exit!!!
	// note: that will require linking to winmm.lib
	// note: never use static initializers (like this) with Winamp plug-ins!
	LARGE_INTEGER begin;
	QueryPerformanceCounter(&begin);

	LARGE_INTEGER m_high_perf_timer_freq;
	QueryPerformanceFrequency(&m_high_perf_timer_freq);

	int ticks_to_wait = nanos * (int)m_high_perf_timer_freq.QuadPart / 1000000000;
	int done = 0;
	do
	{
		LARGE_INTEGER t;
		QueryPerformanceCounter(&t);

		int ticks_passed = (int)((__int64)t.QuadPart - (__int64)begin.QuadPart);
		int ticks_left = ticks_to_wait - ticks_passed;

		if (t.QuadPart < begin.QuadPart)    // time wrap
			done = 1;
		if (ticks_passed >= ticks_to_wait)
			done = 1;

		if (!done)
		{
			// if > 0.002s left, do Sleep(1), which will actually sleep some 
			//   steady amount, probably 1-2 ms,
			//   and do so in a nice way (cpu meter drops; laptop battery spared).
			// otherwise, do a few Sleep(0)'s, which just give up the timeslice,
			//   but don't really save cpu or battery, but do pass a tiny
			//   amount of time.
			if (ticks_left > (int)m_high_perf_timer_freq.QuadPart * 2 / 1000)
				Sleep(1);
			else
				for (int i = 0; i < 10; i++)
					Sleep(0);  // causes thread to give up its timeslice
		}
	} while (!done);
}
