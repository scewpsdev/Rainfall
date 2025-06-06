#pragma once

#include <stdint.h>


struct Vector2
{
	float x, y;


	Vector2();
	Vector2(float f);
	Vector2(float x, float y);

	float lengthSquared() const;
	float length() const;
	Vector2 normalized() const;

	float angle() const;
	Vector2 rotate(float angle) const;

	Vector2 operator-() const;

	static const Vector2 Zero;
	static const Vector2 One;
	static const Vector2 Left;
	static const Vector2 Right;
	static const Vector2 Down;
	static const Vector2 Up;
	static const Vector2 AxisX;
	static const Vector2 AxisY;
};

struct Vector3
{
	union {
		struct {
			float x, y, z;
		};
		struct {
			Vector2 xy;
			float z;
		};
		struct {
			float x;
			Vector2 yz;
		};
	};


	Vector3();
	Vector3(float xyz);
	Vector3(float x, float y, float z);
	Vector3(const Vector2& xy, float z);
	Vector3(float x, const Vector2& yz);

	float lengthSquared() const;
	float length() const;
	Vector3 normalized() const;

	Vector3 operator-() const;

	Vector3& operator+=(const Vector3& v);
	Vector3& operator-=(const Vector3& v);
	Vector3& operator*=(const Vector3& v);
	Vector3& operator/=(const Vector3& v);

	static const Vector3 Zero;
	static const Vector3 One;
	static const Vector3 Left;
	static const Vector3 Right;
	static const Vector3 Down;
	static const Vector3 Up;
	static const Vector3 Forward;
	static const Vector3 Back;
	static const Vector3 AxisX;
	static const Vector3 AxisY;
	static const Vector3 AxisZ;

	static void OrthoNormalize(const Vector3& normal, Vector3& tangent);
};

struct Vector4
{
	union
	{
		struct {
			float x, y, z, w;
		};
		struct {
			Vector3 xyz;
			float w;
		};
		struct {
			Vector2 xy;
			Vector2 zw;
		};
		struct {
			float x;
			Vector3 yzw;
		};
		struct {
			float x;
			Vector2 yz;
			float w;
		};
		struct {
			float r, g, b, a;
		};
		struct {
			Vector3 rgb;
			float a;
		};
		struct {
			Vector2 rg;
			Vector2 ba;
		};
		struct {
			float r;
			Vector3 gba;
		};
		struct {
			float r;
			Vector2 gb;
			float a;
		};
		float elements[4];
	};


	Vector4();
	Vector4(float f);
	Vector4(float x, float y, float z, float w);
	Vector4(const Vector3& xyz, float w);
	Vector4(float x, const Vector3& yzw);

	float& operator[](int index);
	const float& operator[](int index) const;

	Vector4& operator+=(const Vector4& v);
	Vector4& operator-=(const Vector4& v);
	Vector4& operator*=(const Vector4& v);
	Vector4& operator/=(const Vector4& v);

	Vector4& operator+=(float f);
	Vector4& operator-=(float f);
	Vector4& operator*=(float f);
	Vector4& operator/=(float f);

	static const Vector4 Zero;
	static const Vector4 One;
};

struct Vector2i
{
	int x, y;


	Vector2i();
	Vector2i(int i);
	Vector2i(int x, int y);
};

struct Vector3i
{
	union {
		struct {
			int x, y, z;
		};
		struct {
			Vector2i xy;
			int z;
		};
		struct {
			int x;
			Vector2i yz;
		};
	};


	Vector3i();
	Vector3i(int i);
	Vector3i(int x, int y, int z);

	Vector3i operator-() const;
};

struct Vector4i
{
	union {
		struct {
			int x, y, z, w;
		};
		struct {
			Vector3i xyz;
			int w;
		};
		struct {
			Vector2i xy;
			Vector2 zw;
		};
		struct {
			int x;
			Vector3i yzw;
		};
		struct {
			int x;
			Vector2i yz;
			int w;
		};
	};


	Vector4i();
	Vector4i(int x, int y, int z, int w);
	Vector4i(const Vector3i& xyz, int w);
};

struct Color
{
	uint8_t r, g, b, a;


	Color();
	Color(uint8_t r, uint8_t g, uint8_t b, uint8_t a);
	Color(uint32_t hex);

	Vector4 toVector() const;
};


Vector2 operator+(Vector2 a, Vector2 b);
Vector2 operator-(Vector2 a, Vector2 b);
Vector2 operator*(Vector2 a, Vector2 b);
Vector2 operator/(Vector2 a, Vector2 b);

Vector2 operator+(Vector2 a, float b);
Vector2 operator-(Vector2 a, float b);
Vector2 operator*(Vector2 a, float b);
Vector2 operator/(Vector2 a, float b);

Vector2 operator+(float a, Vector2 b);
Vector2 operator-(float a, Vector2 b);
Vector2 operator*(float a, Vector2 b);
Vector2 operator/(float a, Vector2 b);

Vector2 operator+(Vector2i a, Vector2 b);
Vector2 operator-(Vector2i a, Vector2 b);
Vector2 operator*(Vector2i a, Vector2 b);
Vector2 operator/(Vector2i a, Vector2 b);


Vector2& operator+=(Vector2& a, const Vector2& b);
Vector2& operator-=(Vector2& a, const Vector2& b);

bool operator==(const Vector2& a, const Vector2& b);


Vector3 operator+(Vector3 a, Vector3 b);
Vector3 operator-(Vector3 a, Vector3 b);
Vector3 operator*(Vector3 a, Vector3 b);
Vector3 operator/(Vector3 a, Vector3 b);

Vector3 operator+(Vector3 a, float b);
Vector3 operator-(Vector3 a, float b);
Vector3 operator*(Vector3 a, float b);
Vector3 operator/(Vector3 a, float b);

Vector3 operator+(float a, Vector3 b);
Vector3 operator-(float a, Vector3 b);
Vector3 operator*(float a, Vector3 b);
Vector3 operator/(float a, Vector3 b);

bool operator==(const Vector3& a, const Vector3& b);

Vector4 operator+(Vector4 a, Vector4 b);
Vector4 operator-(Vector4 a, Vector4 b);
Vector4 operator*(Vector4 a, Vector4 b);
Vector4 operator/(Vector4 a, Vector4 b);

Vector4 operator+(Vector4 a, float b);
Vector4 operator-(Vector4 a, float b);
Vector4 operator*(Vector4 a, float b);
Vector4 operator/(Vector4 a, float b);

Vector4 operator+(float a, Vector4 b);
Vector4 operator-(float a, Vector4 b);
Vector4 operator*(float a, Vector4 b);
Vector4 operator/(float a, Vector4 b);

bool operator==(const Vector4& a, const Vector4& b);


Vector3i operator+(Vector3i a, Vector3i b);
Vector3i operator-(Vector3i a, Vector3i b);
Vector3i operator*(Vector3i a, Vector3i b);
Vector3i operator/(Vector3i a, Vector3i b);

Vector3 operator*(Vector3i a, float b);
Vector3 operator/(Vector3i a, float b);

bool operator==(const Vector3i& a, const Vector3i& b);

float dot(const Vector3& a, const Vector3& b);
Vector3 cross(const Vector3& a, const Vector3& b);

Vector3i abs(const Vector3i& v);
Vector3 abs(const Vector3& v);
Vector2 abs(const Vector2& v);

Vector2 min(const Vector2& a, const Vector2& b);
Vector2 max(const Vector2& a, const Vector2& b);
Vector3 min(const Vector3& a, const Vector3& b);
Vector3 max(const Vector3& a, const Vector3& b);

Vector2i sign(const Vector2& v);

Vector2 mix(const Vector2& a, const Vector2& b, float t);
Vector3 mix(const Vector3& a, const Vector3& b, float t);
template<typename T>
T mix(const T& a, const T& b, float t)
{
	return t * b + (1 - t) * a;
}
