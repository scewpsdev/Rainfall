#include "ParticleSystem.h"

#include "Hash.h"

#include "vector/Math.h"

#include <math.h>
#include <float.h>
#include <stdio.h>


RFAPI ParticleSystem* ParticleSystem_Create(int maxParticles, Matrix transform)
{
	ParticleSystem* system = BX_NEW(Application_GetAllocator(), ParticleSystem) {};

	system->transform = transform;
	system->maxParticles = maxParticles;
	system->particles = (Particle*)BX_ALLOC(Application_GetAllocator(), maxParticles * sizeof(Particle));
	memset(system->particles, 0, sizeof(Particle) * maxParticles);

	uint32_t seed = (uint32_t)Application_GetCurrentTime();
	system->random = Random(seed);
	system->simplex = Simplex(seed);

	ParticleSystem_Restart(system);

	return system;
}

RFAPI void ParticleSystem_Destroy(ParticleSystem* system)
{
	BX_FREE(Application_GetAllocator(), system->particles);
	BX_DELETE(Application_GetAllocator(), system);
}

RFAPI void ParticleSystem_Restart(ParticleSystem* system)
{
	system->systemStarted = Application_GetCurrentTime();
	system->lastEmitted = Application_GetCurrentTime();

	for (int i = 0; i < system->numBursts; i++)
		system->bursts[i].emitted = 0;
}

RFAPI void ParticleSystem_SetTransform(ParticleSystem* system, Matrix transform, float delta, bool applyVelocity)
{
	if (applyVelocity)
	{
		system->entityVelocity = (transform.translation() - system->transform.translation()) / delta;
		system->entityRotationVelocity = transform.rotation() * system->transform.rotation().conjugated();
	}
	system->transform = transform;
}

static int GetNewParticle(ParticleSystem* system)
{
	for (int i = system->inactiveParticleStart; i < system->maxParticles; i++)
	{
		if (!system->particles[i].active)
		{
			system->particles[i].active = true;
			system->inactiveParticleStart++;
			return i;
		}
	}
	return -1;
}

RFAPI void ParticleSystem_EmitParticle(ParticleSystem* system, float delta)
{
	int particleID = GetNewParticle(system);
	if (particleID == -1)
		return;

	Vector3 position = Vector3::Zero;
	float rotation = system->startRotation;
	if (system->randomRotation > 0)
		rotation += system->random.nextFloat(-PI, PI) * system->randomRotation;

	if (system->spawnShape == ParticleSpawnShape::Circle)
	{
		// sqrt the random number to get an even distribution in the circle
		float r = system->spawnRadius * sqrtf(system->random.nextFloat());
		float theta = system->random.nextFloat() * 2 * PI;
		position = Vector3(r * cosf(theta), 0, r * sinf(theta));
	}
	else if (system->spawnShape == ParticleSpawnShape::Sphere)
	{
		float r = system->spawnRadius * powf(system->random.nextFloat(), 0.333333f);
		float theta = system->random.nextFloat() * 2 * PI;
		float phi = system->random.nextFloat() * PI;
		position = Vector3(r * sinf(phi) * cosf(theta), r * sinf(phi) * sinf(theta), r * cosf(phi));
	}
	else if (system->spawnShape == ParticleSpawnShape::Line)
	{
		float t = system->random.nextFloat();
		position = mix(Vector3::Zero, system->lineSpawnEnd, t);
		if (system->spawnRadius > 0)
		{
			float r = system->spawnRadius * system->random.nextFloat();
			float theta = system->random.nextFloat() * 2 * PI;
			float phi = system->random.nextFloat() * PI;
			position += Vector3(r * sinf(phi) * cosf(theta), r * sinf(phi) * sinf(theta), r * cosf(phi));
		}
	}

	Vector3 localPosition = position;
	if (!system->follow)
		position = system->transform * (position + system->spawnOffset);

	Vector3 velocity = system->startVelocity;
	if (!system->follow)
		velocity = (system->transform * Vector4(velocity, 0)).xyz;
	if (system->applyEntityVelocity)
		velocity += system->entityVelocity;
	if (system->applyCentrifugalForce)
	{
		float rotationAngle = system->entityRotationVelocity.getAngle();
		if (rotationAngle != 0)
		{
			Vector3 rotationAxis = system->entityRotationVelocity.getAxis();
			// to be exact, angular velocity would be w = angle / 2pi / t. but we would multiply by 2pi anyways for calculating the linear velocity (v = w * 2pi * r)
			float angularVelocity = rotationAngle / delta;
			Vector3 fromCenter = position - system->transform.translation();
			Vector3 projectedCenter = system->transform.translation() + rotationAxis * dot(rotationAxis, fromCenter);
			Vector3 fromRotationAxis = position - projectedCenter;
			Vector3 centrifugalVelocity = angularVelocity * fromRotationAxis; // w * 2pi * r
			velocity += centrifugalVelocity;
		}
	}
	if (system->randomVelocity.lengthSquared() > 0)
		velocity += system->random.nextVector3(-1, 1).normalized() * system->randomVelocity;
	if (system->radialVelocity != 0)
	{
		if (system->spawnShape == ParticleSpawnShape::Point)
			velocity += RandomPointOnSphere(system->random) * system->radialVelocity;
		else
		{
			Vector3 r = localPosition.normalized() * system->radialVelocity;
			if (!system->follow)
				r = (system->transform * Vector4(localPosition, 0)).xyz;
			velocity += r;
		}
	}

	float rotationVelocity = 0.0f;
	if (system->randomRotationSpeed > 0)
		rotationVelocity += system->random.nextFloat(-1, 1) * system->randomRotationSpeed;

	float particleLifetime = system->lifetime;
	if (system->randomLifetime > 0)
		particleLifetime *= 1 + system->random.nextFloat(-system->randomLifetime, system->randomLifetime);

	Particle* particle = &system->particles[particleID];
	memset(particle, 0, sizeof(Particle));
	particle->active = true;
	particle->position = position;
	particle->rotation = rotation;
	particle->velocity = velocity;
	particle->rotationVelocity = rotationVelocity;
	particle->size = system->size;
	particle->xscale = 1;
	particle->lifetime = particleLifetime;
	particle->animationFrame = 0;
	particle->color = system->color;
	particle->birthTime = Application_GetCurrentTime();

	if (system->randomFrame)
	{
		float animationFrame = system->random.next() % system->numFrames / (float)(system->atlasSize.x * system->atlasSize.y);
		particle->animationFrame = animationFrame;
	}
}

RFAPI void ParticleSystem_Update(ParticleSystem* system, Quaternion invCameraRotation, float delta)
{
	int64_t now = Application_GetCurrentTime();
	if (system->emissionRate > 0.0f)
	{
		if (now - system->lastEmitted > 1e9 / system->emissionRate)
		{
			int numParticles = (int)floorf((now - system->lastEmitted) / 1e9f * system->emissionRate);
			numParticles = min(numParticles, (int)ceilf(system->emissionRate * 2));
			for (int i = 0; i < numParticles; i++)
				ParticleSystem_EmitParticle(system, delta);
			system->lastEmitted = now;
		}
	}
	if (system->numBursts > 0)
	{
		float elapsed = (now - system->systemStarted) / 1e9f;
		for (int i = 0; i < system->numBursts; i++)
		{
			ParticleBurst* burst = &system->bursts[i];
			if (elapsed >= burst->time && burst->emitted < burst->count)
			{
				int shouldEmitted = burst->duration > 0 ? (int)(fminf((elapsed - burst->time) / burst->duration, 1.0f) * burst->count) : burst->count;
				int d = shouldEmitted - burst->emitted;
				if (d > 0)
				{
					for (int j = 0; j < d; j++)
						ParticleSystem_EmitParticle(system, delta);
					burst->emitted = shouldEmitted;
				}
			}
		}
	}

	Vector3 minpos = Vector3(FLT_MAX), maxpos = Vector3(FLT_MIN);
	float maxRadiusSq = 0.0f;

	system->numParticles = 0;
	for (int i = 0; i < system->maxParticles; i++)
	{
		Particle* particle = &system->particles[i];
		if (particle->active)
		{
			if (i == system->inactiveParticleStart)
				system->inactiveParticleStart++;

			float particleTimer = (now - particle->birthTime) / 1e9f;

			particle->velocity.y += 0.5f * system->gravity * delta;
			particle->velocity += system->drag * particle->velocity.lengthSquared() * -particle->velocity.normalized() / 2;
			particle->position += particle->velocity * delta;
			if (system->velocityNoise > 0)
			{
				float t = (Application_GetCurrentTime() + hash(i)) / 1e9f;
				Vector3 velocityNoise = Vector3(system->simplex.sample1f(t), system->simplex.sample1f(t + 100), system->simplex.sample1f(t + 200)).normalized();
				particle->position += system->velocityNoise * velocityNoise * delta;
			}
			particle->velocity.y += 0.5f * system->gravity * delta;

			minpos = min(minpos, particle->position - particle->size);
			maxpos = max(maxpos, particle->position + particle->size);
			maxRadiusSq = fmaxf(maxRadiusSq, (particle->position - system->boundingSphere.center).lengthSquared());

			particle->rotation += particle->rotationVelocity * delta;

			if (system->rotateAlongMovement)
			{
				Vector3 screenSpaceVelocity = invCameraRotation * particle->velocity;
				particle->rotation = atan2f(screenSpaceVelocity.y, screenSpaceVelocity.x);
				particle->xscale = 1 + system->movementStretch * screenSpaceVelocity.xy.length();
			}

			float progress = particleTimer / particle->lifetime;

			if (system->sizeAnim.count > 0)
				particle->size = system->sizeAnim.getValue(progress);

			if (system->colorAnim.count > 0)
				particle->color = system->colorAnim.getValue(progress);

			if (system->numFrames > 0 && !system->randomFrame)
			{
				float animationFrame = particleTimer / particle->lifetime * system->numFrames / (system->atlasSize.x * system->atlasSize.y);
				particle->animationFrame = animationFrame;
			}

			if ((now - particle->birthTime) / 1e9f >= particle->lifetime)
			{
				particle->active = false;
			}
			else
			{
				system->numParticles++;
			}
		}

		if (!particle->active && system->inactiveParticleStart > i)
		{
			system->inactiveParticleStart = i;
		}
	}

	system->boundingBox.min = minpos;
	system->boundingBox.max = maxpos;

	system->boundingSphere.center = (minpos + maxpos) * 0.5f;
	system->boundingSphere.radius = sqrtf(maxRadiusSq);

	if (system->follow)
	{
		system->boundingSphere.center += system->transform * (system->boundingSphere.center + system->spawnOffset);
		system->boundingBox = TransformBoundingBox(system->boundingBox, system->transform);
	}
}

RFAPI int ParticleSystem_GetNumParticles(ParticleSystem* system)
{
	return system->numParticles;
}

RFAPI Particle* ParticleSystem_GetParticleData(ParticleSystem* system)
{
	return system->particles;
}

RFAPI bool ParticleSystem_HasFinished(ParticleSystem* system)
{
	if (system->emissionRate > 0)
		return false;

	if (system->numParticles > 0)
		return false;

	bool allBurstsEmitted = true;
	for (int j = 0; j < system->numBursts; j++)
	{
		if (system->bursts[j].emitted < system->bursts[j].count)
		{
			allBurstsEmitted = false;
			break;
		}
	}

	return allBurstsEmitted;
}
