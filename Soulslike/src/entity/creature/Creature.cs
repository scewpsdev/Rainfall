using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Creature : Entity, Hittable
{
	public Node rootMotionNode;

	AnimationState idleAnim;

	AnimationState actionAnim1, actionAnim2;
	AnimationState currentActionAnim;

	public CreatureActionManager actionManager;

	public Sound[] slashSound, stabSound;


	public Creature(string name)
	{
		this.name = name;

		hitboxFilterGroup = PhysicsFilter.CreatureHitbox;
		hitboxFilterMask = 0;
		load($"entity/creature/{name}/{name}.rfs", PhysicsFilter.Creature);
		body.lockRotationAxis(true, true, true);

		if (model != null)
			rootMotionNode = model.skeleton.getNode("root");

		animator = Animator.Create(model, this);

		idleAnim = Animator.CreateAnimation(model, "idle", true, 0.4f);
		idleAnim.animationSpeed = 0.005f;

		actionAnim1 = Animator.CreateAnimation(model, "default", false, 0.1f);
		actionAnim2 = Animator.CreateAnimation(model, "default", false, 0.1f);

		actionManager = new CreatureActionManager(this);

		slashSound = Resource.GetSounds("audio/hit_slash", 2);
		stabSound = Resource.GetSounds("audio/hit_stab", 2);
	}

	public void hit(Entity by, Item item)
	{
		actionManager.queueAction(new CreatureStaggerAction());
	}

	void updateActions()
	{
		actionManager.update();
	}

	void updateAnimations()
	{
		Matrix transform = getModelMatrix();

		if (body != null)
		{
			if (body.type == RigidBodyType.Dynamic)
				body.getTransform(out position, out rotation);
			else if (body.type == RigidBodyType.Kinematic)
				body.setTransform(position, rotation);
		}

		if (actionManager.currentAction != null)
		{
			animator.setAnimation(currentActionAnim);
		}
		else
		{
			animator.setAnimation(idleAnim);
		}

		animator.applyAnimation();

		if (hitboxes != null && model != null && animator != null)
			updateBoneHitbox(model.skeleton.rootNode, transform * animator.getNodeLocalTransform(model.skeleton.rootNode));

		for (int i = 0; i < particles.Count; i++)
		{
			//if (Renderer.IsInFrustum(particles[i].boundingSphere.center, particles[i].boundingSphere.radius, transform, Renderer.pv))
			particles[i].setTransform(transform);
		}
	}

	public override void update()
	{
		updateActions();
		updateAnimations();
	}

	public AnimationState getNextActionAnimationState()
	{
		currentActionAnim = currentActionAnim == actionAnim1 ? actionAnim2 : currentActionAnim == actionAnim2 ? actionAnim1 : actionAnim1;
		return currentActionAnim;
	}
}
