using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class ReverbZone : Entity
{
	Vector3 halfExtents;
	bool value;
	Sound ambientSound;
	float ambientGain;

	Sound lastAmbientSound;
	float lastAmbientGain;
	RigidBody body;


	public ReverbZone(Vector3 size, bool value, Sound ambientSound = null, float ambientGain = 1.0f)
	{
		halfExtents = 0.5f * size;
		this.value = value;
		this.ambientSound = ambientSound;
		this.ambientGain = ambientGain;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static, (uint)PhysicsFilterGroup.EventTriggers, (uint)PhysicsFilterGroup.PlayerControllerKinematicBody);
		body.addBoxTrigger(halfExtents, halfExtents, Quaternion.Identity);
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (otherController != null && otherController.entity is Player || other != null && other.entity is Player)
		{
			if (contactType == ContactType.Found)
			{
				AudioManager.SetReverb(value);
				lastAmbientSound = AudioManager.currentAmbientSound;
				lastAmbientGain = AudioManager.currentAmbientGain;
				AudioManager.SetAmbientSound(ambientSound, ambientGain);
			}
			else if (contactType == ContactType.Lost)
			{
				AudioManager.SetReverb(!value);
				AudioManager.SetAmbientSound(lastAmbientSound, lastAmbientGain);
			}
		}
	}
}
