#pragma once

#include "Vector.h"


struct Quaternion
{
	float x, y, z, w;


	Quaternion();
	Quaternion(float x, float y, float z, float w);
	Quaternion(const Vector3& xyz, float w);

	void normalize();

	float length() const;
	Quaternion normalized() const;
	Quaternion conjugated() const;
	Vector4 toAxisAngle() const;
	float getAngle() const;

	Vector3 forward() const;
	Vector3 back() const;
	Vector3 left() const;
	Vector3 right() const;
	Vector3 down() const;
	Vector3 up() const;


	static Quaternion FromAxisAngle(Vector3 axis, float angle);
	static Quaternion LookAt(const Vector3& eye, const Vector3& at);
	static Quaternion FromEulers(Vector3 eulers);

	static const Quaternion Identity;
};


Quaternion operator*(const Quaternion& a, const Quaternion& b);
Quaternion operator*(const Quaternion& a, const float& b);
Quaternion operator*(const float& a, const Quaternion& b);

Quaternion operator+(const Quaternion& a, const Quaternion& b);

Vector3 operator*(const Quaternion& a, const Vector3& b);

bool operator==(const Quaternion& a, const Quaternion& b);

Quaternion slerp(const Quaternion& left, Quaternion right, float t);
