#pragma once

#include "mesa/MesaBase.h"

#include "Sound.h"

#include "mesa/systems/scene/Component.h"

#include "mesa/math/Vector.h"

#include <stdint.h>


struct MESA_API AudioSource : Component
{
	uint32_t handle = -1;

	Vector3 offset = Vector3(0.0f);

	Sound* sound = nullptr;
	int64_t soundPlayed = 0;


	AudioSource();

	void play(Sound* sound, float gain, float pitch);
	void stop();
	void pause();
	void resume();
	void rewind();

	void setAmbient(bool ambient);
	void setLooping(bool looping);

	bool isAmbient();
	float elapsedTime() const;

	virtual void init() override;
	virtual void update() override;
	virtual void destroy() override;

private:
	bool ambient = false;
	bool looping = false;
};
