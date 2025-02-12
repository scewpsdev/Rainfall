#include "Rainfall.h"
#include "Console.h"

#include "vector/Vector.h"
#include "vector/Quaternion.h"

#include <soloud.h>
#include <soloud_wav.h>
#include <soloud_freeverbfilter.h>


using namespace SoLoud;

static Soloud soloud;
static Bus defaultBus;
static Bus reverbBus;
static handle reverbBusSource;
static bool reverbEnabled = false;

static FreeverbFilter reverb;

static float _3dVolume = 1.0f;


RFAPI void Audio_Init()
{
	soloud.init();
	printf("Audio Backend: %s\n", soloud.getBackendString());

	reverb.setParams(0.0f, 0.5f, 0.5f, 1.0f);
	reverbBus.setFilter(1, &reverb);

	// We need to play 3d sounds over a default bus,
	// otherwise sound attenuation will glitch for the first frame of playing (yikes)
	//defaultBus.set3dAttenuation(SoLoud::AudioSource::INVERSE_DISTANCE, 10);
	soloud.play(defaultBus);

	reverbBusSource = soloud.play(reverbBus);
}

RFAPI void Audio_Shutdown()
{
	soloud.stopAll();
	soloud.deinit();
}

RFAPI void Audio_Update()
{
	soloud.update3dAudio();
}

RFAPI void Audio_SetGlobalVolume(float volume)
{
	soloud.setGlobalVolume(volume);
}

RFAPI void Audio_Set3DVolume(float volume)
{
	_3dVolume = volume;
}

RFAPI void Audio_ListenerUpdateTransform(const Vector3& position, const Vector3& forward, const Vector3& up)
{
	soloud.set3dListenerParameters(position.x, position.y, position.z, forward.x, forward.y, forward.z, up.x, up.y, up.z);
}

RFAPI uint32_t Audio_SourcePlayBackground(AudioSource* sound, float gain, float pitch, bool looping, float fadein)
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

RFAPI uint32_t Audio_SourcePlayBackgroundClocked(AudioSource* sound, float deltaTime, float gain, float pitch, bool looping, float fadein)
{
	handle source = soloud.playClocked(deltaTime, *sound, gain);
	soloud.setPause(source, true);
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
	handle source = defaultBus.play3d(*sound, position.x, position.y, position.z, 0.0f, 0.0f, 0.0f, 0, true);
	soloud.setRelativePlaySpeed(source, pitch);
	soloud.set3dSourceAttenuation(source, SoLoud::AudioSource::INVERSE_DISTANCE, rolloff);
	soloud.set3dSourceMinMaxDistance(source, 1, 500);
	soloud.fadeVolume(source, gain * _3dVolume, 0.0001f);
	soloud.setPause(source, false);

	//if (reverbBusSource != 0)
	if (reverbEnabled)
	{
		handle reverbSource = reverbBus.play3d(*sound, position.x, position.y, position.z, 0.0f, 0.0f, 0.0f, 0, true);
		soloud.setRelativePlaySpeed(reverbSource, pitch);
		soloud.set3dSourceAttenuation(reverbSource, SoLoud::AudioSource::INVERSE_DISTANCE, rolloff);
		soloud.set3dSourceMinMaxDistance(reverbSource, 1, 500);
		soloud.fadeVolume(source, gain, 0.0001f);
		soloud.setPause(reverbSource, false);
	}

	return source;
}

RFAPI void Audio_SourceStop(uint32_t source)
{
	soloud.stop(source);
}

RFAPI void Audio_SourceSetPaused(uint32_t source, bool paused)
{
	soloud.setPause(source, paused);
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

RFAPI void Audio_SourceFadeoutVolume(uint32_t source, float time)
{
	soloud.fadeVolume(source, 0.0f, time);
}

RFAPI void Audio_SourceFadeinVolume(uint32_t source, float time)
{
	soloud.fadeVolume(source, 1.0f, time);
}

RFAPI void Audio_SourceFadeVolume(uint32_t source, float volume, float time)
{
	soloud.fadeVolume(source, volume, time);
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

RFAPI void Audio_SourceSetInaudibleBehavior(uint32_t source, bool mustTick, bool kill)
{
	soloud.setInaudibleBehavior(source, mustTick, kill);
}

RFAPI void Audio_SourceSetProtect(uint32_t source, bool protect)
{
	soloud.setProtectVoice(source, protect);
}

RFAPI void Audio_SoundSetSingleInstance(AudioSource* sound, bool singleInstance)
{
	sound->setSingleInstance(singleInstance);
}

RFAPI void Audio_SetEffectNone()
{
	reverbEnabled = false;
	//soloud.stop(reverbBusSource);
	//reverbBusSource = 0;
}

RFAPI void Audio_SetEffectReverb()
{
	reverbEnabled = true;
	//reverbBusSource = soloud.play(reverbBus);
}
