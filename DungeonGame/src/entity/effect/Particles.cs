using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Particles
{
	public static ParticleSystem CreateFire(int maxParticles, Vector3 offset = default)
	{
		ParticleSystem particles = new ParticleSystem(maxParticles);

		particles.emissionRate = 200.0f;
		particles.lifetime = 0.7f;
		particles.spawnOffset = offset;
		particles.spawnRadius = 0.05f;
		particles.spawnShape = ParticleSpawnShape.Sphere;
		particles.particleSizeAnim = new Gradient<float>(0.02f, 0.005f);
		particles.initialVelocity = Vector3.Zero;
		particles.gravity = 1.0f;
		particles.follow = true;
		particles.additive = true;
		particles.spriteTint = new Vector4(2.7738395f, 0.7894696f, 0.25998735f, 1.0f) * 0.5f;
		particles.randomRotation = true;

		return particles;
	}
}
