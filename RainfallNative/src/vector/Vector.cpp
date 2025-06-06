#include "Vector.h"

#include "Math.h"

#include <math.h>


Vector2::Vector2()
	: x(0.0f), y(0.0f)
{
}

Vector2::Vector2(float f)
	: x(f), y(f)
{
}

Vector2::Vector2(float x, float y)
	: x(x), y(y)
{
}

float Vector2::lengthSquared() const
{
	return x * x + y * y;
}

float Vector2::length() const
{
	return sqrtf(x * x + y * y);
}

Vector2 Vector2::normalized() const
{
	return *this / length();
}

float Vector2::angle() const
{
	return atan2f(y, x);
}

Vector2 Vector2::rotate(float angle) const
{
	float currentAngle = this->angle();
	float length = this->length();
	float newAngle = currentAngle + angle;
	return Vector2(length * cosf(newAngle), length * sinf(newAngle));
}

Vector2 Vector2::operator-() const
{
	return Vector2(-x, -y);
}

const Vector2 Vector2::Zero = Vector2(0.0f, 0.0f);
const Vector2 Vector2::One = Vector2(1.0f, 1.0f);
const Vector2 Vector2::Left = Vector2(-1.0f, 0.0f);
const Vector2 Vector2::Right = Vector2(1.0f, 0.0f);
const Vector2 Vector2::Down = Vector2(0.0f, -1.0f);
const Vector2 Vector2::Up = Vector2(0.0f, 1.0f);
const Vector2 Vector2::AxisX = Vector2(1.0f, 0.0f);
const Vector2 Vector2::AxisY = Vector2(0.0f, 1.0f);

Vector3::Vector3()
	: x(0.0f), y(0.0f), z(0.0f)
{
}

Vector3::Vector3(float xyz)
	: x(xyz), y(xyz), z(xyz)
{
}

Vector3::Vector3(float x, float y, float z)
	: x(x), y(y), z(z)
{
}

Vector3::Vector3(const Vector2& xy, float z)
	: x(xy.x), y(xy.y), z(z)
{
}

Vector3::Vector3(float x, const Vector2& yz)
	: x(x), y(yz.x), z(yz.y)
{
}

float Vector3::lengthSquared() const
{
	return x * x + y * y + z * z;
}

float Vector3::length() const
{
	return sqrtf(x * x + y * y + z * z);
}

Vector3 Vector3::normalized() const
{
	if (fabsf(this->lengthSquared() - 1.0f) > 0.001f && this->lengthSquared() != 0.0f)
		return *this / length();
	else
		return *this;
}

Vector3 Vector3::operator-() const
{
	return Vector3(-x, -y, -z);
}

Vector3& Vector3::operator+=(const Vector3& v)
{
	x += v.x;
	y += v.y;
	z += v.z;
	return *this;
}

Vector3& Vector3::operator-=(const Vector3& v)
{
	x -= v.x;
	y -= v.y;
	z -= v.z;
	return *this;
}

Vector3& Vector3::operator*=(const Vector3& v)
{
	x *= v.x;
	y *= v.y;
	z *= v.z;
	return *this;
}

Vector3& Vector3::operator/=(const Vector3& v)
{
	x /= v.x;
	y /= v.y;
	z /= v.z;
	return *this;
}

void Vector3::OrthoNormalize(const Vector3& normal, Vector3& tangent)
{
	Vector3 norm = normal;
	Vector3 tan = tangent.normalized();
	tangent = tan - (norm * dot(norm, tan));
	tangent = tangent.normalized();
}

const Vector3 Vector3::Zero = Vector3(0.0f, 0.0f, 0.0f);
const Vector3 Vector3::One = Vector3(1.0f, 1.0f, 1.0f);
const Vector3 Vector3::Left = Vector3(-1.0f, 0.0f, 0.0f);
const Vector3 Vector3::Right = Vector3(1.0f, 0.0f, 0.0f);
const Vector3 Vector3::Down = Vector3(0.0f, -1.0f, 0.0f);
const Vector3 Vector3::Up = Vector3(0.0f, 1.0f, 0.0f);
const Vector3 Vector3::Forward = Vector3(0.0f, 0.0f, -1.0f);
const Vector3 Vector3::Back = Vector3(0.0f, 0.0f, 1.0f);
const Vector3 Vector3::AxisX = Vector3(1.0f, 0.0f, 0.0f);
const Vector3 Vector3::AxisY = Vector3(0.0f, 1.0f, 0.0f);
const Vector3 Vector3::AxisZ = Vector3(0.0f, 0.0f, 1.0f);

Vector4::Vector4()
	: x(0.0f), y(0.0f), z(0.0f), w(0.0f)
{
}

Vector4::Vector4(float f)
	: x(f), y(f), z(f), w(f)
{
}

Vector4::Vector4(float x, float y, float z, float w)
	: x(x), y(y), z(z), w(w)
{
}

Vector4::Vector4(const Vector3& xyz, float w)
	: x(xyz.x), y(xyz.y), z(xyz.z), w(w)
{
}

Vector4::Vector4(float x, const Vector3& yzw)
	: x(x), y(yzw.x), z(yzw.y), w(yzw.z)
{
}

float& Vector4::operator[](int index)
{
	return elements[index];
}

const float& Vector4::operator[](int index) const
{
	return elements[index];
}

Vector4& Vector4::operator+=(const Vector4& v)
{
	x += v.x;
	y += v.y;
	z += v.z;
	w += v.w;
	return *this;
}

Vector4& Vector4::operator-=(const Vector4& v)
{
	x -= v.x;
	y -= v.y;
	z -= v.z;
	w -= v.w;
	return *this;
}

Vector4& Vector4::operator*=(const Vector4& v)
{
	x *= v.x;
	y *= v.y;
	z *= v.z;
	w *= v.w;
	return *this;
}

Vector4& Vector4::operator/=(const Vector4& v)
{
	x /= v.x;
	y /= v.y;
	z /= v.z;
	w /= v.w;
	return *this;
}

Vector4& Vector4::operator+=(float f)
{
	x += f;
	y += f;
	z += f;
	w += f;
	return *this;
}

Vector4& Vector4::operator-=(float f)
{
	x -= f;
	y -= f;
	z -= f;
	w -= f;
	return *this;
}

Vector4& Vector4::operator*=(float f)
{
	x *= f;
	y *= f;
	z *= f;
	w *= f;
	return *this;
}

Vector4& Vector4::operator/=(float f)
{
	x /= f;
	y /= f;
	z /= f;
	w /= f;
	return *this;
}

const Vector4 Vector4::Zero = Vector4(0.0f, 0.0f, 0.0f, 0.0f);
const Vector4 Vector4::One = Vector4(1.0f, 1.0f, 1.0f, 1.0f);

Vector2i::Vector2i()
	: x(0), y(0)
{
}

Vector2i::Vector2i(int i)
	: x(i), y(i)
{
}

Vector2i::Vector2i(int x, int y)
	: x(x), y(y)
{
}

Vector3i::Vector3i()
	: x(0), y(0), z(0)
{
}

Vector3i::Vector3i(int i)
	: x(i), y(i), z(i)
{
}

Vector3i::Vector3i(int x, int y, int z)
	: x(x), y(y), z(z)
{
}

Vector3i Vector3i::operator-() const
{
	return Vector3i(-x, -y, -z);
}

Vector4i::Vector4i()
	: x(0), y(0), z(0), w(0)
{
}

Vector4i::Vector4i(int x, int y, int z, int w)
	: x(x), y(y), z(z), w(w)
{
}

Vector4i::Vector4i(const Vector3i& xyz, int w)
	: x(xyz.x), y(xyz.y), z(xyz.z), w(w)
{
}

Color::Color()
	: r(0), g(0), b(0), a(0)
{
}

Color::Color(uint8_t r, uint8_t g, uint8_t b, uint8_t a)
	: r(r), g(g), b(b), a(a)
{
}

Color::Color(uint32_t hex)
	: r((hex & 0x00ff0000) >> 16), g((hex & 0x0000ff00) >> 8), b(hex & 0x000000ff), a((hex & 0xff000000) >> 24)
{
}

Vector4 Color::toVector() const
{
	return Vector4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
}

Vector2 operator+(Vector2 a, Vector2 b)
{
	return Vector2(a.x + b.x, a.y + b.y);
}

Vector2 operator-(Vector2  a, Vector2  b)
{
	return Vector2(a.x - b.x, a.y - b.y);
}

Vector2 operator*(Vector2 a, Vector2 b)
{
	return Vector2(a.x * b.x, a.y * b.y);
}

Vector2 operator/(Vector2 a, Vector2 b)
{
	return Vector2(a.x / b.x, a.y / b.y);
}

Vector2 operator+(Vector2 a, float b)
{
	return Vector2(a.x + b, a.y + b);
}

Vector2 operator-(Vector2 a, float b)
{
	return Vector2(a.x - b, a.y - b);
}

Vector2 operator*(Vector2 a, float b)
{
	return Vector2(a.x * b, a.y * b);
}

Vector2 operator/(Vector2 a, float b)
{
	return Vector2(a.x / b, a.y / b);
}

Vector2 operator+(float a, Vector2 b)
{
	return Vector2(a + b.x, a + b.y);
}

Vector2 operator-(float a, Vector2 b)
{
	return Vector2(a - b.x, a - b.y);
}

Vector2 operator*(float a, Vector2 b)
{
	return Vector2(a * b.x, a * b.y);
}

Vector2 operator/(float a, Vector2 b)
{
	return Vector2(a / b.x, a / b.y);
}

Vector2 operator+(Vector2i a, Vector2 b)
{
	return Vector2(a.x + b.x, a.y + b.y);
}

Vector2 operator-(Vector2i a, Vector2 b)
{
	return Vector2(a.x - b.x, a.y - b.y);
}

Vector2 operator*(Vector2i a, Vector2 b)
{
	return Vector2(a.x * b.x, a.y * b.y);
}

Vector2 operator/(Vector2i a, Vector2 b)
{
	return Vector2(a.x / b.x, a.y / b.y);
}

Vector2& operator+=(Vector2& a, const Vector2& b)
{
	a.x += b.x;
	a.y += b.y;
	return a;
}

Vector2& operator-=(Vector2& a, const Vector2& b)
{
	a.x -= b.x;
	a.y -= b.y;
	return a;
}

bool operator==(const Vector2& a, const Vector2& b)
{
	return a.x == b.x && a.y == b.y;
}

Vector3 operator+(Vector3 a, Vector3 b)
{
	return Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
}

Vector3 operator-(Vector3 a, Vector3 b)
{
	return Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
}

Vector3 operator*(Vector3 a, Vector3 b)
{
	return Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
}

Vector3 operator/(Vector3 a, Vector3 b)
{
	return Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
}

Vector3 operator+(Vector3 a, float b)
{
	return Vector3(a.x + b, a.y + b, a.z + b);
}

Vector3 operator-(Vector3 a, float b)
{
	return Vector3(a.x - b, a.y - b, a.z - b);
}

Vector3 operator*(Vector3 a, float b)
{
	return Vector3(a.x * b, a.y * b, a.z * b);
}

Vector3 operator/(Vector3 a, float b)
{
	return Vector3(a.x / b, a.y / b, a.z / b);
}

Vector3 operator+(float a, Vector3 b)
{
	return Vector3(a + b.x, a + b.y, a + b.z);
}

Vector3 operator-(float a, Vector3 b)
{
	return Vector3(a - b.x, a - b.y, a - b.z);
}

Vector3 operator*(float a, Vector3 b)
{
	return Vector3(a * b.x, a * b.y, a * b.z);
}

Vector3 operator/(float a, Vector3 b)
{
	return Vector3(a / b.x, a / b.y, a / b.z);
}

bool operator==(const Vector3& a, const Vector3& b)
{
	return a.x == b.x && a.y == b.y && a.z == b.z;
}

Vector3& operator*=(Vector3& a, float b)
{
	a.x *= b;
	a.y *= b;
	a.z *= b;
	return a;
}

Vector4 operator+(Vector4 a, Vector4 b)
{
	return Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
}

Vector4 operator-(Vector4 a, Vector4 b)
{
	return Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
}

Vector4 operator*(Vector4 a, Vector4 b)
{
	return Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
}

Vector4 operator/(Vector4 a, Vector4 b)
{
	return Vector4(a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
}

Vector4 operator+(Vector4 a, float b)
{
	return Vector4(a.x + b, a.y + b, a.z + b, a.w + b);
}

Vector4 operator-(Vector4 a, float b)
{
	return Vector4(a.x - b, a.y - b, a.z - b, a.w - b);
}

Vector4 operator*(Vector4 a, float b)
{
	return Vector4(a.x * b, a.y * b, a.z * b, a.w * b);
}

Vector4 operator/(Vector4 a, float b)
{
	return Vector4(a.x / b, a.y / b, a.z / b, a.w / b);
}

Vector4 operator+(float a, Vector4 b)
{
	return Vector4(a + b.x, a + b.y, a + b.z, a + b.w);
}

Vector4 operator-(float a, Vector4 b)
{
	return Vector4(a - b.x, a - b.y, a - b.z, a - b.w);
}

Vector4 operator*(float a, Vector4 b)
{
	return Vector4(a * b.x, a * b.y, a * b.z, a * b.w);
}

Vector4 operator/(float a, Vector4 b)
{
	return Vector4(a / b.x, a / b.y, a / b.z, a / b.w);
}

bool operator==(const Vector4& a, const Vector4& b)
{
	return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
}

Vector4& operator*=(Vector4& a, float b)
{
	a.x *= b;
	a.y *= b;
	a.z *= b;
	a.w *= b;
	return a;
}

Vector3i operator+(Vector3i a, Vector3i b)
{
	return Vector3i(a.x + b.x, a.y + b.y, a.z + b.z);
}

Vector3i operator-(Vector3i a, Vector3i b)
{
	return Vector3i(a.x - b.x, a.y - b.y, a.z - b.z);
}

Vector3i operator*(Vector3i a, Vector3i b)
{
	return Vector3i(a.x * b.x, a.y * b.y, a.z * b.z);
}

Vector3i operator/(Vector3i a, Vector3i b)
{
	return Vector3i(a.x / b.x, a.y / b.y, a.z / b.z);
}

Vector3 operator*(Vector3i a, float b)
{
	return Vector3(a.x * b, a.y * b, a.z * b);
}

Vector3 operator/(Vector3i a, float b)
{
	return Vector3(a.x / b, a.y / b, a.z / b);
}

bool operator==(const Vector3i& a, const Vector3i& b)
{
	return a.x == b.x && a.y == b.y && a.z == b.z;
}

float dot(const Vector3& a, const Vector3& b)
{
	return a.x * b.x + a.y * b.y + a.z * b.z;
}

Vector3 cross(const Vector3& a, const Vector3& b)
{
	float x = a.y * b.z - a.z * b.y;
	float y = a.z * b.x - a.x * b.z;
	float z = a.x * b.y - a.y * b.x;
	return Vector3(x, y, z);
}

Vector3i abs(const Vector3i& v)
{
	return Vector3i(abs(v.x), abs(v.y), abs(v.z));
}

Vector3 abs(const Vector3& v)
{
	return Vector3(fabsf(v.x), fabsf(v.y), fabsf(v.z));
}

Vector2 abs(const Vector2& v)
{
	return Vector2(fabsf(v.x), fabsf(v.y));
}

Vector2 min(const Vector2& a, const Vector2& b)
{
	return Vector2(fminf(a.x, b.x), fminf(a.y, b.y));
}

Vector2 max(const Vector2& a, const Vector2& b)
{
	return Vector2(fmaxf(a.x, b.x), fmaxf(a.y, b.y));
}

Vector3 min(const Vector3& a, const Vector3& b)
{
	return Vector3(fminf(a.x, b.x), fminf(a.y, b.y), fminf(a.z, b.z));
}

Vector3 max(const Vector3& a, const Vector3& b)
{
	return Vector3(fmaxf(a.x, b.x), fmaxf(a.y, b.y), fmaxf(a.z, b.z));
}

Vector2i sign(const Vector2& v)
{
	return Vector2i(fsign(v.x), fsign(v.y));
}

Vector2 mix(const Vector2& a, const Vector2& b, float t)
{
	return Vector2(
		a.x + (b.x - a.x) * t,
		a.y + (b.y - a.y) * t
	);
}

Vector3 mix(const Vector3& a, const Vector3& b, float t)
{
	return Vector3(
		a.x + (b.x - a.x) * t,
		a.y + (b.y - a.y) * t,
		a.z + (b.z - a.z) * t
	);
}
