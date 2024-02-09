using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class IronDoor : Door
{
	public IronDoor(Item requiredKey = null)
	{
		this.requiredKey = requiredKey;

		model = Resource.GetModel("res/entity/object/door_iron/door_iron.gltf");
		model.maxDistance = (LOD.DISTANCE_MEDIUM);

		sfxOpen = Resource.GetSound("res/entity/object/door_iron/sfx/unlock.ogg");
		sfxClose = Resource.GetSound("res/entity/object/door_iron/sfx/close.ogg");
		sfxLocked = Resource.GetSound("res/entity/object/door_iron/sfx/locked.ogg");
	}

	public override void init()
	{
		base.init();

		doorBody.addBoxCollider(new Vector3(0.5f, 1.0f, 0.05f), new Vector3(-doorHingeOffset, 1.0f, 0.0f), Quaternion.Identity);
	}
}
