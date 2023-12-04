#include "AudioListener.h"

#include "AudioSystem.h"


void AudioListener::init()
{
	AudioSystem::RegisterAudioListener(this);
}

void AudioListener::destroy()
{
	AudioSystem::RemoveAudioListener(this);
}
