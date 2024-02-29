using Rainfall;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class SceneFormat
{
	static DatObject SerializeEntity(Entity entity)
	{
		DatObject obj = new DatObject();

		obj.addString("name", entity.name);
		obj.addString("id", entity.id.ToString());
		obj.addVector3("position", entity.position);
		obj.addQuaternion("rotation", entity.rotation);
		obj.addVector3("scale", entity.scale);

		if (entity.modelPath != null)
			obj.addString("model", entity.modelPath);

		DatArray colliders = new DatArray();
		for (int i = 0; i < entity.colliders.Count; i++)
		{
			DatObject collider = new DatObject();
			collider.addIdentifier("type", entity.colliders[i].type.ToString());
			collider.addVector3("size", entity.colliders[i].size);
			collider.addVector3("offset", entity.colliders[i].offset);
			collider.addVector3("rotation", entity.colliders[i].eulers);
			if (entity.colliders[i].type == ColliderType.Mesh)
				collider.addString("mesh", entity.colliders[i].meshColliderPath);
			colliders.addObject(collider);
		}
		obj.addArray("colliders", colliders);

		DatArray lights = new DatArray();
		for (int i = 0; i < entity.lights.Count; i++)
		{
			DatObject light = new DatObject();
			light.addVector3("color", entity.lights[i].color);
			light.addNumber("intensity", entity.lights[i].intensity);
			light.addVector3("offset", entity.lights[i].offset);
			lights.addObject(light);
		}
		obj.addArray("lights", lights);

		return obj;
	}

	public static void SerializeScene(EditorInstance instance, Stream stream)
	{
		DatFile file = new DatFile();

		DatArray arr = new DatArray();
		for (int i = 0; i < instance.entities.Count; i++)
		{
			DatObject obj = SerializeEntity(instance.entities[i]);
			arr.addObject(obj);
		}
		file.addArray("entities", arr);

		file.addString("selected", instance.selectedEntity.ToString());

		file.serialize(stream);
	}

	public static byte[] SerializeScene(EditorInstance instance)
	{
		MemoryStream stream = new MemoryStream();
		SerializeScene(instance, stream);
		byte[] data = stream.ToArray();
		stream.Close();
		return data;
	}

	public static void WriteScene(EditorInstance instance, string path)
	{
		FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
		SerializeScene(instance, stream);
		stream.Close();
	}

	static Entity DeserializeEntity(DatObject obj)
	{
		obj.getStringContent("name", out string name);
		Entity entity = new Entity(name);

		if (obj.getStringContent("id", out string idStr))
			entity.id = uint.Parse(idStr);

		obj.getVector3("position", out entity.position);
		obj.getQuaternion("rotation", out entity.rotation);
		obj.getVector3("scale", out entity.scale);
		obj.getStringContent("model", out entity.modelPath);

		if (obj.getArray("colliders", out DatArray colliders))
		{
			for (int i = 0; i < colliders.size; i++)
			{
				ColliderData collider = new ColliderData();
				if (colliders[i].obj.getIdentifier("type", out string type))
					collider.type = Utils.ParseEnum<ColliderType>(type);
				if (colliders[i].obj.getVector3("size", out Vector3 size))
					collider.size = size;
				if (colliders[i].obj.getVector3("offset", out Vector3 offset))
					collider.offset = offset;
				if (colliders[i].obj.getVector3("rotation", out Vector3 eulers))
					collider.eulers = eulers;
				if (colliders[i].obj.getStringContent("mesh", out string meshColliderPath))
				{
					collider.meshColliderPath = meshColliderPath;
					collider.reload();
				}
				entity.colliders.Add(collider);
			}
		}

		if (obj.getArray("lights", out DatArray lights))
		{
			for (int i = 0; i < lights.size; i++)
			{
				LightData light = new LightData();
				if (lights[i].obj.getVector3("color", out Vector3 color))
					light.color = color;
				if (lights[i].obj.getNumber("intensity", out float intensity))
					light.intensity = intensity;
				if (lights[i].obj.getVector3("offset", out Vector3 offset))
					light.offset = offset;
				entity.lights.Add(light);
			}
		}

		entity.reload();

		return entity;
	}

	public static void DeserializeScene(EditorInstance instance, Stream stream)
	{
		instance.reset();

		DatFile file = new DatFile(stream);

		if (file.getArray("entities", out DatArray arr))
		{
			for (int i = 0; i < arr.size; i++)
			{
				if (arr[i].type == DatValueType.Object)
				{
					Entity entity = DeserializeEntity(arr[i].obj);
					instance.entities.Add(entity);
				}
				else
				{
					Debug.Assert(false);
				}
			}
		}
		if (file.getStringContent("selected", out string selectedEntity))
		{
			instance.selectedEntity = uint.Parse(selectedEntity);
		}
	}

	public static void DeserializeScene(EditorInstance instance, byte[] data)
	{
		MemoryStream stream = new MemoryStream(data);
		DeserializeScene(instance, stream);
		stream.Close();
	}

	public static void ReadScene(EditorInstance instance, string path)
	{
		FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
		DeserializeScene(instance, stream);
		stream.Close();
	}
}
