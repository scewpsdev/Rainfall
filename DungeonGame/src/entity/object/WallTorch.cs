using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class WallTorch : Entity, Interactable
{
	Model model;
	RigidBody body;
	ParticleSystem fireParticles;

	bool looted = false;


	public WallTorch()
	{
		model = Resource.GetModel("res/entity/object/wall_torch/wall_torch.gltf");

		fireParticles = new ParticleSystem(250);
		fireParticles.textureAtlas = Resource.GetTexture("res/texture/particle/torch_flame.png");
		//fireParticles.atlasColumns = 4;
		fireParticles.frameWidth = 32;
		fireParticles.frameHeight = 32;
		fireParticles.numFrames = 12;
		fireParticles.emissionRate = 120.0f;
		fireParticles.lifetime = 0.8f;
		fireParticles.spawnOffset = new Vector3(0.0f, 0.3f, 0.2f);
		fireParticles.spawnRadius = 0.1f;
		fireParticles.spawnShape = ParticleSpawnShape.Sphere;
		fireParticles.particleSize = 0.2f;
		fireParticles.initialVelocity = new Vector3(0.0f, 0.0f, 0.0f);
		fireParticles.gravity = 2.0f;
		fireParticles.followMode = ParticleFollowMode.Trail;
		fireParticles.additive = true;
		fireParticles.spriteTint = new Vector4(3.0f);
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static, (uint)(PhysicsFilterGroup.Default | PhysicsFilterGroup.Interactable));
		body.addCapsuleCollider(0.1f, 0.6f, new Vector3(0.0f, 0.1f, 0.1f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.ToRadians(19.0f)));
	}

	public override void update()
	{
		base.update();

		if (!looted)
		{
			fireParticles.transform = getModelMatrix();
			fireParticles.update();
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();

		Renderer.DrawSubModelStaticInstanced(model, 1, transform);

		if (!looted)
		{
			Renderer.DrawSubModel(model, 0, transform);

			fireParticles.draw(graphics);

			Vector3 lightPosition = (transform * new Vector4(0.0f, 0.3f, 0.2f, 1.0f)).xyz;
			Renderer.DrawLight(lightPosition, new Vector3(0.965f, 0.604f, 0.329f) * 8.0f);
		}
	}

	public bool canInteract(Entity by)
	{
		return !looted;
	}

	public void interact(Entity by)
	{
		Debug.Assert(!looted);

		if (by is Player)
		{
			Player player = by as Player;
			player.giveItem(Item.Get("torch"), 1);
			fireParticles = null;
			looted = true;
		}
	}
}
