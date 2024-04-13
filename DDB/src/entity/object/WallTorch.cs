using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class WallTorch : StaticObject
{
	ParticleSystem fireParticles;


	public WallTorch()
	{
		model = Resource.GetModel("res/entity/object/wall_torch/wall_torch.gltf");

		fireParticles = new ParticleSystem(250);
		fireParticles.textureAtlas = Resource.GetTexture("res/texture/particle/torch_flame.png");
		fireParticles.atlasColumns = 4;
		fireParticles.frameWidth = 32;
		fireParticles.frameHeight = 32;
		fireParticles.numFrames = 12;
		fireParticles.emissionRate = 120.0f;
		fireParticles.lifetime = 2.0f;
		fireParticles.spawnOffset = new Vector3(0.0f, 0.3f, 0.2f);
		fireParticles.spawnRadius = 0.1f;
		fireParticles.spawnShape = ParticleSpawnShape.Sphere;
		fireParticles.particleSize = 0.2f;
		fireParticles.initialVelocity = new Vector3(0.0f, 0.2f, 0.0f);
		fireParticles.gravity = 0.0f;
		fireParticles.followMode = ParticleFollowMode.Trail;
		fireParticles.additive = true;
		fireParticles.spriteTint = 0xffffffff;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static);
		body.addCapsuleCollider(0.1f, 0.6f, new Vector3(0.0f, 0.1f, 0.1f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.ToRadians(19.0f)));
	}

	public override void update()
	{
		base.update();

		fireParticles.transform = getModelMatrix();
		//fireParticles.update();
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		fireParticles.draw(graphics);

		Vector3 lightPosition = (getModelMatrix() * new Vector4(0.0f, 0.3f, 0.2f, 1.0f)).xyz;
		Renderer.DrawLight(lightPosition, new Vector3(0.965f, 0.604f, 0.329f) * 8.0f);
	}
}
