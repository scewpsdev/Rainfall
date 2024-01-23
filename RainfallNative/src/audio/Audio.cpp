#include "Rainfall.h"
#include "Console.h"

#include "vector/Vector.h"
#include "vector/Quaternion.h"

#include <soloud.h>
#include <soloud_wav.h>
#include <soloud_freeverbfilter.h>


using namespace SoLoud;

static Soloud soloud;
static Bus reverbBus;
static handle reverbBusSource;

static FreeverbFilter reverb;


void Audio_Init()
{
	soloud.init();
	printf("Audio Backend: %s\n", soloud.getBackendString());

	reverb.setParams(0.0f, 0.5f, 0.5f, 1.0f);
	reverbBus.setFilter(1, &reverb);
}

void Audio_Shutdown()
{
	soloud.deinit();
}

void Audio_Update()
{
	soloud.update3dAudio();
}

RFAPI void Audio_ListenerUpdateTransform(const Vector3& position, const Vector3& forward, const Vector3& up)
{
	soloud.set3dListenerParameters(position.x, position.y, position.z, forward.x, forward.y, forward.z, up.x, up.y, up.z);
}

RFAPI uint32_t Audio_PlayBackground(AudioSource* sound, float gain, float pitch, bool looping, float fadein)
{
	handle source = soloud.playBackground(*sound, gain, true);
	soloud.setRelativePlaySpeed(source, pitch);
	soloud.setLooping(source, looping);
	if (fadein > 0)
	{
		soloud.setVolume(source, 0.0f);
		soloud.fadeVolume(source, gain, fadein);
	}
	soloud.setPause(source, false);
	return source;
}

RFAPI uint32_t Audio_SourcePlay(AudioSource* sound, const Vector3& position, float gain, float pitch, float rolloff)
{
	handle source = soloud.play3d(*sound, position.x, position.y, position.z, 0.0f, 0.0f, 0.0f, gain, true);
	soloud.setRelativePlaySpeed(source, pitch);
	soloud.set3dSourceAttenuation(source, SoLoud::AudioSource::INVERSE_DISTANCE, rolloff);
	soloud.set3dSourceMinMaxDistance(source, 1, 50);
	soloud.setPause(source, false);

	if (reverbBusSource != 0)
	{
		handle reverbSource = reverbBus.play3d(*sound, position.x, position.y, position.z, 0.0f, 0.0f, 0.0f, gain, true);
		soloud.setRelativePlaySpeed(reverbSource, pitch);
		soloud.set3dSourceAttenuation(reverbSource, SoLoud::AudioSource::INVERSE_DISTANCE, rolloff);
		soloud.set3dSourceMinMaxDistance(reverbSource, 1, 50);
		soloud.setPause(reverbSource, false);
	}

	return source;
}

RFAPI void Audio_SourceStop(uint32_t source)
{
	soloud.stop(source);
}

RFAPI void Audio_SourcePause(uint32_t source)
{
	soloud.setPause(source, true);
}

RFAPI void Audio_SourceResume(uint32_t source)
{
	soloud.setPause(source, false);
}

RFAPI void Audio_SourceRewind(uint32_t source)
{
	soloud.seek(source, 0.0);
}

RFAPI void Audio_SourceFadeout(uint32_t source, float time)
{
	soloud.fadeVolume(source, 0.0f, time);
	soloud.scheduleStop(source, time);
}

RFAPI void Audio_SourceSetPosition(uint32_t source, const Vector3& position)
{
	soloud.set3dSourcePosition(source, position.x, position.y, position.z);
}

RFAPI void Audio_SourceSetGain(uint32_t source, float gain)
{
	soloud.setVolume(source, gain);
}

RFAPI void Audio_SourceSetPitch(uint32_t source, float pitch)
{
	soloud.setRelativePlaySpeed(source, pitch);
}

RFAPI void Audio_SourceSetLooping(uint32_t source, bool looping)
{
	soloud.setLooping(source, looping);
}

RFAPI void Audio_SetEffectNone()
{
	soloud.stop(reverbBusSource);
	reverbBusSource = 0;
}

RFAPI void Audio_SetEffectReverb()
{
	reverbBusSource = soloud.play(reverbBus);
}
