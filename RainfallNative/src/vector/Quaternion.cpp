#include "Quaternion.h"

#include "Math.h"

#include <math.h>


Quaternion::Quaternion()
	: x(0.0f), y(0.0f), z(0.0f), w(1.0f)
{
}

Quaternion::Quaternion(float x, float y, float z, float w)
	: x(x), y(y), z(z), w(w)
{
}

Quaternion::Quaternion(const Vector3& xyz, float w)
	: x(xyz.x), y(xyz.y), z(xyz.z), w(w)
{
}

void Quaternion::normalize()
{
	if (fabsf(x * x + y * y + z * z + w * w - 1.0f) < 0.001f)
		return;
	if (x * x + y * y + z * z + w * w == 0.0f)
		return;

	float l = 1.0f / this->length();
	x *= l;
	y *= l;
	z *= l;
	w *= l;
}

float Quaternion::length() const
{
	return sqrtf(this->x * this->x + this->y * this->y + this->z * this->z + this->w * this->w);
}

Quaternion Quaternion::normalized() const
{
	if (fabsf(x * x + y * y + z * z + w * w - 1.0f) < 0.00000001f)
		return *this;
	if (x * x + y * y + z * z + w * w == 0.0f)
		return *this;

	float l = 1.0f / this->length();
	float x = this->x * l;
	float y = this->y * l;
	float z = this->z * l;
	float w = this->w * l;
	return Quaternion(x, y, z, w);
}

Quaternion Quaternion::conjugated() const
{
	return Quaternion(-x, -y, -z, w);
}

Vector4 Quaternion::toAxisAngle() const
{
	float angle = 2.0f * acosf(w);
	float s = 1.0f / sqrtf(1.0f - w * w);
	if (isinf(s))
	{
		return Vector4(1.0f, 0.0f, 0.0f, 0.0f);
	}
	if (s < 0.001f)
	{
		return Vector4(1.0f, 0.0f, 0.0f, angle);
	}
	else
	{
		return Vector4(x * s, y * s, z * s, angle);
	}
}

float Quaternion::getAngle() const
{
	return 2.0f * acosf(w);
}

Vector3 Quaternion::forward() const
{
	return *this * Vector3::Forward;
}

Vector3 Quaternion::back() const
{
	return *this * Vector3::Back;
}

Vector3 Quaternion::left() const
{
	return *this * Vector3::Left;
}

Vector3 Quaternion::right() const
{
	return *this * Vector3::Right;
}

Vector3 Quaternion::down() const
{
	return *this * Vector3::Down;
}

Vector3 Quaternion::up() const
{
	return *this * Vector3::Up;
}

Quaternion Quaternion::FromAxisAngle(Vector3 axis, float angle)
{
	float half = angle * 0.5f;
	float s = sinf(half);
	float x = axis.x * s;
	float y = axis.y * s;
	float z = axis.z * s;
	float w = cosf(half);

	return Quaternion(x, y, z, w);
}

Quaternion Quaternion::LookAt(const Vector3& eye, const Vector3& at)
{
	Vector3 forward = (at - eye).normalized();
	Vector3 right = cross(forward, Vector3::Up).normalized();
	Vector3 up = cross(right, forward);

	Matrix rotation = Matrix::Identity;
	rotation.columns[0].xyz = right;
	rotation.columns[1].xyz = up;
	rotation.columns[2].xyz = -forward;

	return rotation.rotation();

	/*
	float d = dot(Vector3::Forward, forward);

	if (fabsf(d - -1.0f) < 0.000001f)
		return Quaternion(0.0f, 1.0f, 0.0f, PI);
	if (fabsf(d - 1.0f) < 0.000001f)
		return Quaternion::Identity;

	float angle = acosf(d);
	Vector3 axis = cross(Vector3::Forward, forward).normalized();
	Quaternion q = FromAxisAngle(axis, angle).normalized();

	return q;
	*/
}

Quaternion Quaternion::FromEulers(Vector3 eulers)
{
	float c1 = cosf(eulers.y / 2.0f);
	float s1 = sinf(eulers.y / 2.0f);
	float c2 = cosf(eulers.z / 2.0f);
	float s2 = sinf(eulers.z / 2.0f);
	float c3 = cosf(eulers.x / 2.0f);
	float s3 = sinf(eulers.x / 2.0f);
	float c1c2 = c1 * c2;
	float s1s2 = s1 * s2;
	float x = c1c2 * s3 + s1s2 * c3;
	float y = s1 * c2 * c3 + c1 * s2 * s3;
	float z = c1 * s2 * c3 - s1 * c2 * s3;
	float w = c1c2 * c3 - s1s2 * s3;

	return Quaternion(x, y, z, w);
}

const Quaternion Quaternion::Identity = Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

Quaternion operator*(const Quaternion& a, const Quaternion& b)
{
	float w = a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z;
	float x = a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y;
	float y = a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x;
	float z = a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w;
	return Quaternion(x, y, z, w);
}

Quaternion operator*(const Quaternion& a, const float& b)
{
	return Quaternion(a.x * b, a.y * b, a.z * b, a.w * b);
}

Quaternion operator*(const float& a, const Quaternion& b)
{
	return Quaternion(a * b.x, a * b.y, a * b.z, a * b.w);
}

Quaternion operator+(const Quaternion& a, const Quaternion& b)
{
	return Quaternion(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
}

Vector3 operator*(const Quaternion& a, const Vector3& b)
{
	Quaternion a1 = a.normalized();
	Quaternion a2 = a1.conjugated();

	Quaternion q;
	q.w = -a1.x * b.x - a1.y * b.y - a1.z * b.z;
	q.x = +a1.w * b.x + a1.y * b.z - a1.z * b.y;
	q.y = +a1.w * b.y - a1.x * b.z + a1.z * b.x;
	q.z = +a1.w * b.z + a1.x * b.y - a1.y * b.x;

	Quaternion q2;
	q2.w = q.w * a2.w - q.x * a2.x - q.y * a2.y - q.z * a2.z;
	q2.x = q.w * a2.x + q.x * a2.w + q.y * a2.z - q.z * a2.y;
	q2.y = q.w * a2.y - q.x * a2.z + q.y * a2.w + q.z * a2.x;
	q2.z = q.w * a2.z + q.x * a2.y - q.y * a2.x + q.z * a2.w;

	return { q2.x, q2.y, q2.z };
}

bool operator==(const Quaternion& a, const Quaternion& b)
{
	return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
}

Quaternion slerp(const Quaternion& left, Quaternion right, float t)
{
	float cosHalfTheta = left.w * right.w + left.x * right.x + left.y * right.y + left.z * right.z;
	if (fabsf(cosHalfTheta) >= 1.0f)
		return left;
	if (cosHalfTheta < 0.0f)
	{
		right = right * -1.0f;
		cosHalfTheta = -cosHalfTheta;
	}

	float halfTheta = acosf(cosHalfTheta);
	float sinHalfTheta = sqrtf(1.0f - cosHalfTheta * cosHalfTheta);
	if (fabsf(sinHalfTheta) < 0.001f)
		return 0.5f * left + 0.5f * right;

	float ratioA = sinf((1.0f - t) * halfTheta) / sinHalfTheta;
	float ratioB = sinf(t * halfTheta) / sinHalfTheta;

	float w = left.w * ratioA + right.w * ratioB;
	float x = left.x * ratioA + right.x * ratioB;
	float y = left.y * ratioA + right.y * ratioB;
	float z = left.z * ratioA + right.z * ratioB;

	return Quaternion(x, y, z, w);
}
