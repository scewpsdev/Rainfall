#pragma once

#include "Hash.h"
#include "vector/Math.h"

#include <stdint.h>
#include <string.h>
#include <math.h>


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

	void nextBytes(uint8_t* bytes, int size)
	{
		int numInts = (size + 3) / 4;
		for (int i = 0; i < numInts; i++)
		{
			uint32_t i32 = next();
			memcpy(&bytes[i * 4], &i32, min(4, size - numInts * 4));
		}
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

	Vector3 randomDirection(Vector3 direction, float randomness, bool uniform)
	{
		float minCosTheta = 1 - 2 * randomness;

		float cosTheta = uniform ? mix(minCosTheta, 1.0f, nextFloat()) : cosf(mix(0.0f, acosf(minCosTheta), nextFloat()));
		float sinTheta = sqrtf(1 - cosTheta * cosTheta);
		float phi = 2 * PI * nextFloat();

		Vector3 localDirection = Vector3(cosf(phi) * sinTheta, sinf(phi) * sinTheta, cosTheta);
		Vector3 right = fabsf(direction.z) < 0.999f ? Vector3(0, 0, 1) : Vector3(1, 0, 0);
		Vector3 tangent = cross(right, direction).normalized();
		Vector3 bitangent = cross(direction, tangent);

		return tangent * localDirection.x + bitangent * localDirection.y + direction * localDirection.z;
	}

	Vector3 randomPointOnSphere()
	{
		float z = 1 - 2 * nextFloat();
		float phi = 2 * PI * nextFloat();

		float r = sqrtf(max(0.0f, 1.0f - z * z));

		Vector3 direction = Vector3(
			r * cosf(phi),
			r * sinf(phi),
			z
		);

		return direction;

		//float x = RandomGaussian(random);
		//float y = RandomGaussian(random);
		//float z = RandomGaussian(random);
		//Vector3 p = Vector3(x, y, z);
		//return p.normalized();
	}
};
