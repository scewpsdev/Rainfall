using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


internal class RagdollEntity : Entity
{
	Model model;
	Animator startingPose;

	public Ragdoll ragdoll { get; private set; }
	uint filterGroup, filterMask;

	Dictionary<string, BoneHitbox> hitboxes;

	AudioSource audio;
	Sound[] impactSounds;

	Model cube;


	public RagdollEntity(Model model, Animator startingPose, Dictionary<string, BoneHitbox> hitboxes = null, uint filterGroup = 1, uint filterMask = 0x0000FFFF)
	{
		this.model = model;
		this.startingPose = startingPose;
		this.hitboxes = hitboxes;
		this.filterGroup = filterGroup;
		this.filterMask = filterMask;

		impactSounds = new Sound[] {
			Resource.GetSound("res/entity/creature/sfx/impact1.ogg"),
			Resource.GetSound("res/entity/creature/sfx/impact2.ogg"),
			Resource.GetSound("res/entity/creature/sfx/impact3.ogg"),
			Resource.GetSound("res/entity/creature/sfx/impact4.ogg"),
		};

		cube = Resource.GetModel("res/models/cube.gltf");
	}

	public override void init()
	{
		ragdoll = new Ragdoll(this, model, model.skeleton.getNode("Hip"), startingPose, getModelMatrix(), hitboxes, filterGroup, filterMask);
		audio = Audio.CreateSource(position);
	}

	public override void destroy()
	{
		ragdoll.destroy();
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (Random.Shared.NextSingle() < 0.2f)
			audio.playSoundOrganic(impactSounds);
	}

	public override void update()
	{
		ragdoll.update();
		ragdoll.getTransform(out position, out rotation);

		audio.updateTransform(position);
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix() * Matrix.CreateRotation(Vector3.Up, MathF.PI), ragdoll.animator);

		/*
		for (int i = 0; i < ragdoll.boneLinks.Count; i++)
		{
			ragdoll.getLinkTransform(i, out Vector3 position, out Quaternion rotation);
			Matrix linkTransform = Matrix.CreateTranslation(position) * Matrix.CreateRotation(rotation);
			Node node = ragdoll.nodes[i];
			if (ragdoll.ragdollColliderData.ContainsKey(node))
			{
				Vector3 colliderOffset = ragdoll.ragdollColliderData[node].Item1;
				Vector3 colliderSize = ragdoll.ragdollColliderData[node].Item2;
				Renderer.DrawModel(cube, linkTransform * Matrix.CreateTranslation(colliderOffset) * Matrix.CreateScale(2 * colliderSize));
			}
		}
		*/
	}
}
