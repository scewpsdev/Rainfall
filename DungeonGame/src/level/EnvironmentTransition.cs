using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


struct EnvironmentState
{
	public Cubemap skybox;
	public float skyboxIntensity;
	public DirectionalLight sun;
	public Vector3 fogColor;
	public float fogIntensity;
}

internal class EnvironmentTransition : Entity
{
	Vector3 halfExtents;
	RigidBody body;

	Vector3 direction;

	public EnvironmentState to = new EnvironmentState { skyboxIntensity = 1.0f, fogColor = new Vector3(1.0f) }, from = new EnvironmentState { skyboxIntensity = 1.0f, fogColor = new Vector3(1.0f) };


	public EnvironmentTransition(Vector3 halfExtents, Vector3 direction)
	{
		this.halfExtents = halfExtents;
		this.direction = direction;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static);
		body.addBoxTrigger(halfExtents);
	}

	public override void destroy()
	{
		body.destroy();
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (otherController != null && otherController.entity is Player)
		{
			Vector3 toPlayer = (((Player)otherController.entity).position - position).normalized;
			float dir = Vector3.Dot(toPlayer, direction);

			EnvironmentState newState = new EnvironmentState();

			if (contactType == ContactType.Found && dir < 0.0f ||
				contactType == ContactType.Lost && dir > 0.0f)
			{
				newState = to;
			}
			else if (contactType == ContactType.Found && dir > 0.0f ||
				contactType == ContactType.Lost && dir < 0.0f)
			{
				newState = from;
			}
			else
			{
				Debug.Assert(false);
			}

			GraphicsManager.environmentMap = newState.skybox;
			GraphicsManager.environmentMapIntensity = newState.skyboxIntensity;
			GraphicsManager.sun = newState.sun;
			Renderer.fogColor = newState.fogColor;
			Renderer.fogIntensity = newState.fogIntensity;
		}
	}
}
