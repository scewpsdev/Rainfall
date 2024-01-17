#include "Sound.h"

#include "Application.h"

#include <openal/al.h>


static ALenum GetALFormat(int channels, int bps)
{
	ALenum format = 0;
	if (channels == 1 && bps == 8) format = AL_FORMAT_MONO8;
	if (channels == 1 && bps == 16) format = AL_FORMAT_MONO16;
	if (channels == 2 && bps == 8) format = AL_FORMAT_STEREO8;
	if (channels == 2 && bps == 16) format = AL_FORMAT_STEREO16;
	return format;
}

Sound::Sound(void* data, int size, int channels, int sampleRate, int bps)
	: data(data), size(size), channels(channels), bps(bps), sampleRate(sampleRate)
{
	alGenBuffers(1, &handle);

	ALenum format = GetALFormat(channels, bps);
	alBufferData(handle, format, data, size, sampleRate);
}

RFAPI float Audio_SoundGetDuration(Sound* sound)
{
	return (float)sound->size / sound->sampleRate / (sound->bps / 8);
}
