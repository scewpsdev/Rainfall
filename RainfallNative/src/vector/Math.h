#pragma once

#include "Vector.h"
#include "Quaternion.h"
#include "Matrix.h"

#include "utils/Random.h"


#define PI 3.14159265359f


int ipow(int base, int exp);

int fsign(float f);

float radians(float degrees);
float degrees(float radians);

float clamp(float f, float min, float max);

int min(int a, int b);
int max(int a, int b);

Vector3 RandomPointOnSphere(Random& random);

Vector2i WorldToScreenSpace(const Vector3& p, const Matrix& vp, int displayWidth, int displayHeight);
