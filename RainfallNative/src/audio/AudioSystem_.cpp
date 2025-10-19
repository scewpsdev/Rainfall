

#include "Rainfall.h"
#include "Console.h"
#include "Sound.h"

#include "Reverb.h"
#include "ReverbPresets.h"

#include "vector/Vector.h"
#include "vector/Quaternion.h"

#include <openal/alc.h>
#include <openal/al.h>
#include <openal/efx.h>


namespace AudioSystem
{
	static ALCdevice* device;
	static ALCcontext* context;

	static ALuint effectSlot;
	static ALuint reverbEffect;

	static LPALGENAUXILIARYEFFECTSLOTS alGenAuxiliaryEffectSlots;
	static LPALDELETEAUXILIARYEFFECTSLOTS alDeleteAuxiliaryEffectSlots;
	static LPALISAUXILIARYEFFECTSLOT alIsAuxiliaryEffectSlot;
	static LPALAUXILIARYEFFECTSLOTI alAuxiliaryEffectSloti;
	static LPALAUXILIARYEFFECTSLOTIV alAuxiliaryEffectSlotiv;
	static LPALAUXILIARYEFFECTSLOTF alAuxiliaryEffectSlotf;
	static LPALAUXILIARYEFFECTSLOTFV alAuxiliaryEffectSlotfv;
	static LPALGETAUXILIARYEFFECTSLOTI alGetAuxiliaryEffectSloti;
	static LPALGETAUXILIARYEFFECTSLOTIV alGetAuxiliaryEffectSlotiv;
	static LPALGETAUXILIARYEFFECTSLOTF alGetAuxiliaryEffectSlotf;
	static LPALGETAUXILIARYEFFECTSLOTFV alGetAuxiliaryEffectSlotfv;

	LPALGENEFFECTS alGenEffects;
	LPALDELETEEFFECTS alDeleteEffects;
	LPALISEFFECT alIsEffect;
	LPALEFFECTI alEffecti;
	LPALEFFECTIV alEffectiv;
	LPALEFFECTF alEffectf;
	LPALEFFECTFV alEffectfv;
	LPALGETEFFECTI alGetEffecti;
	LPALGETEFFECTIV alGetEffectiv;
	LPALGETEFFECTF alGetEffectf;
	LPALGETEFFECTFV alGetEffectfv;


	void CheckOpenALError()
	{
		ALenum error = alGetError();
		if (error != AL_NO_ERROR)
		{
			__debugbreak();
		}
	}

	RFAPI void Audio_Init()
	{
		const ALCchar* devicename = alcGetString(NULL, ALC_DEVICE_SPECIFIER);
		device = alcOpenDevice(devicename);
		if (!device)
		{
			Console_Error("Failed to create audio device %s", devicename);
			return;
		}
		context = alcCreateContext(device, NULL);
		if (!context)
		{
			Console_Error("Failed to create audio context");
			return;
		}
		alcMakeContextCurrent(context);

		if (!alcIsExtensionPresent(device, ALC_EXT_EFX_NAME))
		{
			Console_Warn("EFX not supported");
			return;
		}

#define LOAD_PROC(T, x)  ((x) = (T)alGetProcAddress(#x))
		LOAD_PROC(LPALGENEFFECTS, alGenEffects);
		LOAD_PROC(LPALDELETEEFFECTS, alDeleteEffects);
		LOAD_PROC(LPALISEFFECT, alIsEffect);
		LOAD_PROC(LPALEFFECTI, alEffecti);
		LOAD_PROC(LPALEFFECTIV, alEffectiv);
		LOAD_PROC(LPALEFFECTF, alEffectf);
		LOAD_PROC(LPALEFFECTFV, alEffectfv);
		LOAD_PROC(LPALGETEFFECTI, alGetEffecti);
		LOAD_PROC(LPALGETEFFECTIV, alGetEffectiv);
		LOAD_PROC(LPALGETEFFECTF, alGetEffectf);
		LOAD_PROC(LPALGETEFFECTFV, alGetEffectfv);

		LOAD_PROC(LPALGENAUXILIARYEFFECTSLOTS, alGenAuxiliaryEffectSlots);
		LOAD_PROC(LPALDELETEAUXILIARYEFFECTSLOTS, alDeleteAuxiliaryEffectSlots);
		LOAD_PROC(LPALISAUXILIARYEFFECTSLOT, alIsAuxiliaryEffectSlot);
		LOAD_PROC(LPALAUXILIARYEFFECTSLOTI, alAuxiliaryEffectSloti);
		LOAD_PROC(LPALAUXILIARYEFFECTSLOTIV, alAuxiliaryEffectSlotiv);
		LOAD_PROC(LPALAUXILIARYEFFECTSLOTF, alAuxiliaryEffectSlotf);
		LOAD_PROC(LPALAUXILIARYEFFECTSLOTFV, alAuxiliaryEffectSlotfv);
		LOAD_PROC(LPALGETAUXILIARYEFFECTSLOTI, alGetAuxiliaryEffectSloti);
		LOAD_PROC(LPALGETAUXILIARYEFFECTSLOTIV, alGetAuxiliaryEffectSlotiv);
		LOAD_PROC(LPALGETAUXILIARYEFFECTSLOTF, alGetAuxiliaryEffectSlotf);
		LOAD_PROC(LPALGETAUXILIARYEFFECTSLOTFV, alGetAuxiliaryEffectSlotfv);
#undef LOAD_PROC

		alGenAuxiliaryEffectSlots(1, &effectSlot);
		alAuxiliaryEffectSloti(effectSlot, AL_EFFECTSLOT_AUXILIARY_SEND_AUTO, AL_TRUE);

		ReverbEffect preset = EFX_REVERB_PRESET_GENERIC;
		preset.flLateReverbGain = 1.2589f * 0.4f;
		reverbEffect = CreateReverbEffect(&preset);

		CheckOpenALError();
	}

	RFAPI void Audio_Shutdown()
	{
		alcDestroyContext(context);
		alcCloseDevice(device);
	}

	RFAPI void Audio_ListenerUpdateTransform(const Vector3& position, const Vector3& forward, const Vector3& up)
	{
		float orientation[6] = {
			forward.x, forward.y, forward.z,
			up.x, up.y, up.z
		};

		alListener3f(AL_POSITION, position.x, position.y, position.z);
		alListenerfv(AL_ORIENTATION, orientation);
	}

	RFAPI uint32_t Audio_CreateSource(const Vector3& position)
	{
		uint32_t source;
		alGenSources(1, &source);
		alSource3i(source, AL_AUXILIARY_SEND_FILTER, (ALint)effectSlot, 0, AL_FILTER_NULL);
		alSource3f(source, AL_POSITION, position.x, position.y, position.z);
		return source;
	}

	RFAPI void Audio_DestroySource(uint32_t source)
	{
		alDeleteSources(1, &source);
	}

	RFAPI void Audio_SourceUpdateTransform(uint32_t source, const Vector3& position)
	{
		alSource3f(source, AL_POSITION, position.x, position.y, position.z);
	}

	RFAPI void Audio_SourcePlaySound(uint32_t source, Sound* sound, float gain, float pitch)
	{
		alSourceStop(source);
		alSourcef(source, AL_GAIN, gain);
		alSourcef(source, AL_PITCH, pitch);
		alSourcei(source, AL_BUFFER, sound->handle);
		alSourcePlay(source);
	}

	RFAPI void Audio_SourceStop(uint32_t source)
	{
		alSourceStop(source);
	}

	RFAPI void Audio_SourcePause(uint32_t source)
	{
		alSourcePause(source);
	}

	RFAPI void Audio_SourceResume(uint32_t source)
	{
		alSourcePlay(source);
	}

	RFAPI void Audio_SourceRewind(uint32_t source)
	{
		alSourceRewind(source);
	}

	RFAPI void Audio_SourceSetGain(uint32_t source, float gain)
	{
		alSourcef(source, AL_GAIN, gain);
	}

	RFAPI void Audio_SourceSetPitch(uint32_t source, float pitch)
	{
		alSourcef(source, AL_PITCH, pitch);
	}

	RFAPI void Audio_SourceSetLooping(uint32_t source, bool looping)
	{
		alSourcei(source, AL_LOOPING, looping);
	}

	RFAPI void Audio_SourceSetAmbientMode(uint32_t source, bool ambient)
	{
		if (ambient)
		{
			alSourcei(source, AL_SOURCE_RELATIVE, AL_TRUE);
			alSource3f(source, AL_POSITION, 0.0f, 0.0f, 0.0f);

			alSource3i(source, AL_AUXILIARY_SEND_FILTER, 0, 0, 0);
		}
		else
		{
			alSourcei(source, AL_SOURCE_RELATIVE, AL_FALSE);

			alSource3i(source, AL_AUXILIARY_SEND_FILTER, (ALint)effectSlot, 0, AL_FILTER_NULL);
		}
	}

	RFAPI void Audio_SetEffectNone()
	{
		alAuxiliaryEffectSloti(effectSlot, AL_EFFECTSLOT_EFFECT, AL_EFFECT_NULL);
	}

	RFAPI void Audio_SetEffectReverb()
	{
		alAuxiliaryEffectSloti(effectSlot, AL_EFFECTSLOT_EFFECT, (ALint)reverbEffect);
	}
}
