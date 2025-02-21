using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Player : Entity
{
	Cart cart;

	Model cap;
	Model chain;
	Model glasses;

	AnimationState defaultPose;
	AnimationState rideForwardAnim, rideLeftForwardAnim, rideRightForwardAnim;
	AnimationState rideAnim, rideLeftAnim, rideRightAnim;
	AnimationState rideBackwardsAnim, rideLeftBackwardsAnim, rideRightBackwardsAnim;

	Ragdoll ragdoll;
	long ragdollSince = -1;


	public Player(Cart cart)
	{
		this.cart = cart;

		model = Resource.GetModel("bob.gltf");
		cap = Resource.GetModel("cap.gltf");
		chain = Resource.GetModel("chain.gltf");
		glasses = Resource.GetModel("glasses.gltf");

		animator = Animator.Create(model, this);
		defaultPose = Animator.CreateAnimation(model, "default");
		rideForwardAnim = Animator.CreateAnimation(model, "ride_forward", true, 0.5f);
		rideLeftForwardAnim = Animator.CreateAnimation(model, "ride_left_forward", true, 0.5f);
		rideRightForwardAnim = Animator.CreateAnimation(model, "ride_right_forward", true, 0.5f);
		rideAnim = Animator.CreateAnimation(model, "ride", true, 0.5f);
		rideLeftAnim = Animator.CreateAnimation(model, "ride_left", true, 0.5f);
		rideRightAnim = Animator.CreateAnimation(model, "ride_right", true, 0.5f);
		rideBackwardsAnim = Animator.CreateAnimation(model, "ride_backwards", true, 0.5f);
		rideLeftBackwardsAnim = Animator.CreateAnimation(model, "ride_left_backwards", true, 0.5f);
		rideRightBackwardsAnim = Animator.CreateAnimation(model, "ride_right_backwards", true, 0.5f);
		animator.setAnimation(rideAnim);
	}

	public void eject(Vector3 velocity)
	{
		position += Vector3.Up;

		if (velocity.length > 10)
			velocity = velocity.normalized * 10;

		animator.setAnimation(defaultPose, true);
		animator.update();
		animator.applyAnimation();

		if (SceneFormat.Read("ragdoll.rfs", out List<SceneFormat.EntityData> entities, out uint _))
		{
			SceneFormat.EntityData entityData = entities[0];
			ragdoll = new Ragdoll(this, model.skeleton.getNode("Hip"), animator, getModelMatrix(), velocity, entityData.boneColliders, PhysicsFilter.Ragdoll, PhysicsFilter.Default);
			ragdollSince = Time.currentTime;
		}
	}

	public override void update()
	{
		if (ragdoll != null)
		{
			ragdoll.update();
			ragdoll.getTransform().decompose(out position, out rotation);

			if (ragdollSince != -1 && (Time.currentTime - ragdollSince) / 1e9f >= 3)
			{
				Vector3 velocity = ragdoll.getHitboxForNode(ragdoll.rootNode).getVelocity();
				if (velocity.length < 0.5f || velocity.y < -1000)
				{
					cart.respawn();
					ragdoll.destroy();
					ragdoll = null;
					ragdollSince = -1;
				}
			}
		}
		else
		{
			position = cart.position;
			rotation = cart.rotation;

			Vector2i directionInput = Vector2i.Zero;
			if (Input.IsKeyDown(KeyCode.W))
				directionInput.y++;
			if (Input.IsKeyDown(KeyCode.S))
				directionInput.y--;
			if (Input.IsKeyDown(KeyCode.A))
				directionInput.x--;
			if (Input.IsKeyDown(KeyCode.D))
				directionInput.x++;

			if (directionInput == new Vector2i(1, 1))
				animator.setAnimation(rideRightForwardAnim);
			else if (directionInput == new Vector2i(0, 1))
				animator.setAnimation(rideForwardAnim);
			else if (directionInput == new Vector2i(-1, 1))
				animator.setAnimation(rideLeftForwardAnim);
			else if (directionInput == new Vector2i(1, 0))
				animator.setAnimation(rideRightAnim);
			else if (directionInput == new Vector2i(0, 0))
				animator.setAnimation(rideAnim);
			else if (directionInput == new Vector2i(-1, 0))
				animator.setAnimation(rideLeftAnim);
			else if (directionInput == new Vector2i(1, -1))
				animator.setAnimation(rideRightBackwardsAnim);
			else if (directionInput == new Vector2i(0, -1))
				animator.setAnimation(rideBackwardsAnim);
			else if (directionInput == new Vector2i(-1, -1))
				animator.setAnimation(rideLeftBackwardsAnim);

			//animator.update();

			base.update();
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		if (GameState.instance.hasCap)
			Renderer.DrawModel(cap, getModelMatrix(), animator);
		if (GameState.instance.hasChain)
			Renderer.DrawModel(chain, getModelMatrix(), animator);
		if (GameState.instance.hasGlasses)
			Renderer.DrawModel(glasses, getModelMatrix(), animator);
	}
}
