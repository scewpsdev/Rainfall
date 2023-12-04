#pragma once

#include "mesa/MesaBase.h"


struct AudioListener;
struct AudioSource;
struct Sound;

namespace AudioSystem
{
	void CheckOpenALError();

	void Init();
	void Shutdown();

	void Update();

	void PlaySound(AudioSource* source, Sound* sound, float gain, float pitch);
	void StopSound(AudioSource* source);
	void PauseSound(AudioSource* source);
	void ResumeSound(AudioSource* source);
	void RewindSound(AudioSource* source);

	void SourceSetAmbient(AudioSource* source, bool ambient);
	void SourceSetLooping(AudioSource* source, bool looping);

	void RegisterAudioListener(AudioListener* listener);
	void RemoveAudioListener(AudioListener* listener);

	void RegisterAudioSource(AudioSource* source);
	void RemoveAudioSource(AudioSource* source);

	MESA_API void SetEffectNone();
	MESA_API void SetEffectReverb();
}
