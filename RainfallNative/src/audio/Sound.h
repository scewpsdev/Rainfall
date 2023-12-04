#pragma once

#include <stdint.h>


struct Sound
{
	uint32_t handle;

	void* data;
	int size;
	int channels;
	int bps;
	int sampleRate;


	Sound(void* data, int size, int channels, int sampleRate, int bps);
};
