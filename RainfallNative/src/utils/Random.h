#pragma once

#include "Hash.h"

#include <stdint.h>


struct Random
{
	uint32_t v;


	Random()
		: v(0)
	{
	}

	Random(uint32_t seed)
		: v(hash(seed))
	{
	}

	uint32_t next()
	{
		uint32_t value = v;
		v = hash(v);
		return value;
	}

	float nextFloat()
	{
		uint32_t value = next();
		return value / (float)UINT32_MAX;
	}

	float nextFloat(float min, float max)
	{
		return min + (max - min) * nextFloat();
	}

	Vector3 nextVector3(float min, float max)
	{
		return Vector3(
			nextFloat(min, max),
			nextFloat(min, max),
			nextFloat(min, max)
		);
	}
};
