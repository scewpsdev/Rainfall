using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum DoorType
{
	Normal,
	Windowed,
}

public class WoodenDoor : Door
{
	DoorType type;


	public WoodenDoor(DoorType type)
	{
		this.type = type;

		if (type == DoorType.Normal)
		{
			model = Resource.GetModel("res/entity/object/door/door.gltf");
		}
		else
		{
			model = Resource.GetModel("res/entity/object/door/door_windowed.gltf");
		}
		frame = Resource.GetModel("res/entity/object/door/door_frame.gltf");
		frame.isStatic = true;

		model.maxDistance =(LOD.DISTANCE_MEDIUM);
		frame.maxDistance = (LOD.DISTANCE_MEDIUM);

		doorHingeOffset = 0.75f;

		sfxOpen = Resource.GetSound("res/entity/object/door/sfx/open.ogg");
		sfxClose = Resource.GetSound("res/entity/object/door/sfx/close.ogg");
	}

	public override void init()
	{
		base.init();

		frameBody.addBoxCollider(new Vector3(0.4f, 1.5f, 0.2f), new Vector3(-1.1f, 1.5f, 0.0f), Quaternion.Identity);
		frameBody.addBoxCollider(new Vector3(0.4f, 1.5f, 0.2f), new Vector3(1.1f, 1.5f, 0.0f), Quaternion.Identity);
		frameBody.addBoxCollider(new Vector3(0.7f, 0.35f, 0.2f), new Vector3(0.0f, 2.65f, 0.0f), Quaternion.Identity);

		if (type == DoorType.Normal)
		{
			doorBody.addBoxCollider(new Vector3(0.7f, 1.15f, 0.1f), new Vector3(-doorHingeOffset, 1.15f, 0.0f), Quaternion.Identity, 0.0f);
			doorBody.setTransform(position, rotation);
		}
		else if (type == DoorType.Windowed)
		{
			doorBody.addBoxCollider(new Vector3(0.7f, 0.7f, 0.1f), new Vector3(-doorHingeOffset, 0.7f, 0.0f), Quaternion.Identity, 0.0f);
			doorBody.addBoxCollider(new Vector3(0.7f, 0.25f, 0.1f), new Vector3(-doorHingeOffset, 2.05f, 0.0f), Quaternion.Identity, 0.0f);
			doorBody.addBoxCollider(new Vector3(0.2f, 0.2f, 0.1f), new Vector3(-doorHingeOffset - 0.5f, 1.6f, 0.0f), Quaternion.Identity, 0.0f);
			doorBody.addBoxCollider(new Vector3(0.2f, 0.2f, 0.1f), new Vector3(-doorHingeOffset + 0.5f, 1.6f, 0.0f), Quaternion.Identity, 0.0f);
			doorBody.setTransform(position, rotation);
		}
		else
		{
			Debug.Assert(false);
		}
	}
}
