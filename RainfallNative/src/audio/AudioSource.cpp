#include "AudioSource.h"

#include "AudioSystem.h"

#include "mesa/systems/core/Time.h"


AudioSource::AudioSource()
{
}

void AudioSource::play(Sound* sound, float gain, float pitch)
{
	this->sound = sound;
	this->soundPlayed = Time::now();
	AudioSystem::PlaySound(this, sound, gain, pitch);
}

void AudioSource::stop()
{
	this->sound = nullptr;
	this->soundPlayed = 0;
	AudioSystem::StopSound(this);
}

void AudioSource::pause()
{
	AudioSystem::PauseSound(this);
}

void AudioSource::resume()
{
	AudioSystem::ResumeSound(this);
}

void AudioSource::rewind()
{
	this->soundPlayed = Time::now();
	AudioSystem::RewindSound(this);
}

void AudioSource::setAmbient(bool ambient)
{
	AudioSystem::SourceSetAmbient(this, ambient);
}

void AudioSource::setLooping(bool looping)
{
	AudioSystem::SourceSetLooping(this, looping);
}

bool AudioSource::isAmbient()
{
	return ambient;
}

float AudioSource::elapsedTime() const
{
	return sound ? (Time::now() - soundPlayed) / 1e9f : 0.0f;
}

void AudioSource::init()
{
	AudioSystem::RegisterAudioSource(this);
}

void AudioSource::update()
{
	if (sound)
	{
		if (elapsedTime() > sound->duration && !looping)
			stop();
	}
}

void AudioSource::destroy()
{
	AudioSystem::RemoveAudioSource(this);
}
