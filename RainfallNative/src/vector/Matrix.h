#pragma once

#include "Vector.h"
#include "Quaternion.h"


struct Matrix
{
	union
	{
		struct
		{
			float m00, m01, m02, m03;
			float m10, m11, m12, m13;
			float m20, m21, m22, m23;
			float m30, m31, m32, m33;
		};
		float matrix[4][4];
		float elements[16];
		Vector4 columns[4];
	};

	Matrix();
	Matrix(float diagonal);
	Matrix(const Vector4& col0, const Vector4& col1, const Vector4& col2, const Vector4& col3);
	Matrix(const float elements[16]);

	Vector3 translation() const;
	Vector3 scale() const;
	Quaternion rotation() const;

	void decompose(Vector3& translation, Quaternion& rotation, Vector3& scale) const;

	float determinant() const;
	Matrix inverted() const;

	Vector4& operator[](int column);
	const Vector4& operator[](int column) const;


	static Matrix Translate(const Vector4& v);
	static Matrix Translate(const Vector3& v);
	static Matrix Rotate(const Quaternion& q);
	static Matrix Scale(const Vector3& v);
	static Matrix Transform(const Vector3& position, const Quaternion& rotation, const Vector3& scale);

	static Matrix Perspective(float fovy, float aspect, float near, float far);
	static Matrix Orthographic(float left, float right, float bottom, float top, float near, float far);

	static const Matrix Identity;
};


Vector4 mul(const Matrix& left, const Vector4& right);

Matrix operator*(const Matrix& left, const Matrix& right);
Vector4 operator*(const Matrix& left, const Vector4& right);
Vector3 operator*(const Matrix& left, const Vector3& right);

bool operator==(const Matrix& a, const Matrix& b);
bool operator!=(const Matrix& a, const Matrix& b);

void GetFrustumPlanes(const Matrix& pv, Vector4 planes[6]);
