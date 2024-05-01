#include "Math.h"

#include <math.h>
#include <float.h>


int ipow(int base, int exp)
{
	int result = 1;
	for (;;)
	{
		if (exp & 1)
			result *= base;
		exp >>= 1;
		if (!exp)
			break;
		base *= base;
	}

	return result;
}

int fsign(float f)
{
	return f < 0.0f ? -1 : f > 0.0f ? 1 : 0;
}

float radians(float degrees)
{
	return degrees / 180.0f * PI;
}

float degrees(float radians)
{
	return radians / PI * 180.0f;
}

float clamp(float f, float min, float max)
{
	return fminf(fmaxf(f, min), max);
}

int min(int a, int b)
{
	return a < b ? a : b;
}

int max(int a, int b)
{
	return a > b ? a : b;
}

static float nextGaussian = FLT_MAX;
static float RandomGaussian(Random& random)
{
	if (nextGaussian == FLT_MAX)
	{
		float u1 = random.nextFloat();
		float u2 = random.nextFloat();
		float r = sqrtf(-2 * logf(u1));
		float t = 2 * PI * u2;
		float x = r * cosf(t);
		float y = r * sinf(t);
		nextGaussian = y;
		return x;
	}
	else
	{
		float r = nextGaussian;
		nextGaussian = FLT_MAX;
		return r;
	}
}

Vector3 RandomPointOnSphere(Random& random)
{
	float x = RandomGaussian(random);
	float y = RandomGaussian(random);
	float z = RandomGaussian(random);
	Vector3 p = Vector3(x, y, z);
	return p.normalized();
}

Vector2i WorldToScreenSpace(const Vector3& p, const Matrix& vp, int displayWidth, int displayHeight)
{
	Vector4 clipSpacePosition = vp * Vector4(p, 1.0f);
	Vector3 ndcSpacePosition = clipSpacePosition.xyz / clipSpacePosition.w;
	if (ndcSpacePosition.z >= -1.0f && ndcSpacePosition.z <= 1.0f)
	{
		Vector2 windowSpacePosition = ndcSpacePosition.xy * 0.5f + 0.5f;
		Vector2i pixelPosition = Vector2i(
			(int)(windowSpacePosition.x * displayWidth + 0.5f),
			displayHeight - (int)(windowSpacePosition.y * displayHeight + 0.5f)
		);
		return pixelPosition;
	}
	else
	{
		return Vector2i(-1, -1);
	}
}
