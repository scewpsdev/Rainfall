#pragma once

#include "Rainfall.h"

#include "vector/Vector.h"
#include "vector/Matrix.h"
#include "vector/Gradient.h"

#include "graphics/Geometry.h"

#include "utils/List.h"
#include "utils/Random.h"
#include "utils/Simplex.h"


enum class ParticleSpawnShape
{
	Point,
	Circle,
	Sphere,
	Line,
};

struct Particle
{
	bool active;
	Vector3 position;
	float rotation;
	Vector3 velocity;
	float rotationVelocity;
	float size;
	float xscale;
	float lifetime;
	float animationFrame;
	Vector4 color;

	int64_t birthTime;
};

struct ParticleBurst
{
	float time;
	int count;
	float duration;

	int emitted;
};


struct ParticleSystem
{
	char name[32] = "";
	Matrix transform = Matrix::Identity;
	Vector3 entityVelocity = Vector3::Zero;
	Quaternion entityRotationVelocity = Quaternion::Identity;

	float lifetime = 1;
	float size = 0.1f;
	bool follow = false;

	float emissionRate = 5;
	ParticleSpawnShape spawnShape = ParticleSpawnShape::Point;
	Vector3 spawnOffset = Vector3::Zero;
	float spawnRadius = 1;
	Vector3 lineSpawnEnd = Vector3::Right;

	float gravity = 0;
	float drag = 0;
	Vector3 startVelocity = Vector3::Zero;
	float radialVelocity = 0;
	float startRotation = 0;
	float rotationSpeed = 0;
	bool applyEntityVelocity = false;
	bool applyCentrifugalForce = false;
	bool rotateAlongMovement = false;
	float movementStretch = 0;

	char textureAtlasPath[256] = "";
	uint16_t textureAtlas = bgfx::kInvalidHandle;
	Vector2i atlasSize = Vector2i(1);
	int numFrames = 1;
	bool randomFrame = false;
	bool linearFiltering = false;

	Vector4 color = Vector4::One;
	bool additive = false;
	float emissiveIntensity = 0;
	float lightInfluence = 1;

	Vector3 randomVelocity = Vector3::Zero;
	float randomRotation = 0;
	float randomRotationSpeed = 0;
	float randomLifetime = 0;
	float velocityNoise = 0;

	Gradient<float, 3> sizeAnim;
	Gradient<Vector4, 3> colorAnim;

	int numBursts = 0;
	ParticleBurst* bursts;

	Random random;
	Simplex simplex;

	int maxParticles;
	Particle* particles;
	int inactiveParticleStart = 0;
	int numParticles = 0;

	int64_t systemStarted, lastEmitted;

	AABB boundingBox;
	Sphere boundingSphere;
};


RFAPI ParticleSystem* ParticleSystem_Create(int maxParticles, Matrix transform);
RFAPI void ParticleSystem_Destroy(ParticleSystem* system);
RFAPI void ParticleSystem_Restart(ParticleSystem* system);
RFAPI void ParticleSystem_EmitParticle(ParticleSystem* system);
RFAPI void ParticleSystem_Update(ParticleSystem* system);
RFAPI bool ParticleSystem_HasFinished(ParticleSystem* system);
