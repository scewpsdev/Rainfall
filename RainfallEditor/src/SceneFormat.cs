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

		DatArray particles = new DatArray();
		for (int i = 0; i < entity.particles.Count; i++)
		{
			ParticleSystem particleData = entity.particles[i];

			DatObject particle = new DatObject();

			particle.addString("name", particleData.name);

			particle.addNumber("lifetime", particleData.lifetime);
			particle.addNumber("size", particleData.size);

			particle.addNumber("emissionRate", particleData.emissionRate);
			particle.addIdentifier("spawnShape", particleData.spawnShape.ToString());
			particle.addVector3("spawnOffset", particleData.spawnOffset);
			if (particleData.spawnShape == ParticleSpawnShape.Circle || particleData.spawnShape == ParticleSpawnShape.Sphere)
				particle.addNumber("spawnRadius", particleData.spawnRadius);
			if (particleData.spawnShape == ParticleSpawnShape.Line)
				particle.addVector3("lineEnd", particleData.lineEnd);
			particle.addBoolean("randomStartRotation", particleData.randomStartRotation);

			particle.addBoolean("follow", particleData.follow);
			particle.addNumber("gravity", particleData.gravity);
			particle.addVector3("startVelocity", particleData.startVelocity);
			particle.addNumber("rotationSpeed", particleData.rotationSpeed);

			if (particleData.textureAtlasPath != null)
			{
				particle.addString("textureAtlas", particleData.textureAtlasPath);
				particle.addVector2("atlasSize", (Vector2)particleData.atlasSize);
				particle.addNumber("numFrames", particleData.numFrames);
				particle.addBoolean("linearFiltering", particleData.linearFiltering);
			}

			particle.addVector4("color", particleData.color);
			particle.addBoolean("additive", particleData.additive);

			particle.addNumber("randomVelocity", particleData.randomVelocity);
			particle.addNumber("randomRotationSpeed", particleData.randomRotationSpeed);
			particle.addNumber("randomLifetime", particleData.randomLifetime);

			if (particleData.sizeAnim != null)
				particle.addVector2("sizeAnim", new Vector2(particleData.sizeAnim.getValue(0), particleData.sizeAnim.getValue(1)));
			if (particleData.colorAnim != null)
			{
				particle.addVector4("colorAnimStart", particleData.colorAnim.getValue(0));
				particle.addVector4("colorAnimEnd", particleData.colorAnim.getValue(1));
			}

			particles.addObject(particle);
		}
		obj.addArray("particles", particles);

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

		if (obj.getArray("particles", out DatArray particles))
		{
			for (int i = 0; i < particles.size; i++)
			{
				DatObject particle = particles[i].obj;
				ParticleSystem particleData = new ParticleSystem(1000);

				particle.getStringContent("name", out particleData.name);

				particle.getNumber("lifetime", out particleData.lifetime);
				particle.getNumber("size", out particleData.size);

				particle.getNumber("emissionRate", out particleData.emissionRate);
				if (particle.getIdentifier("spawnShape", out string spawnShape))
					particleData.spawnShape = Utils.ParseEnum<ParticleSpawnShape>(spawnShape);
				particle.getVector3("spawnOffset", out particleData.spawnOffset);
				if (particleData.spawnShape == ParticleSpawnShape.Circle || particleData.spawnShape == ParticleSpawnShape.Sphere)
					particle.getNumber("spawnRadius", out particleData.spawnRadius);
				if (particleData.spawnShape == ParticleSpawnShape.Line)
					particle.getVector3("lineEnd", out particleData.lineEnd);
				particle.getBoolean("randomStartRotation", out particleData.randomStartRotation);

				particle.getBoolean("follow", out particleData.follow);
				particle.getNumber("gravity", out particleData.gravity);
				particle.getVector3("startVelocity", out particleData.startVelocity);
				particle.getNumber("rotationSpeed", out particleData.rotationSpeed);

				if (particle.getString("textureAtlas", out string textureAtlasPath))
				{
					particleData.textureAtlasPath = textureAtlasPath;
					particleData.reload();

					if (particle.getVector2("atlasSize", out Vector2 atlasSize))
						particleData.atlasSize = (Vector2i)Vector2.Round(atlasSize);
					particle.getNumber("numFrames", out particleData.numFrames);
					particle.getBoolean("linearFiltering", out particleData.linearFiltering);
				}

				particle.getVector4("color", out particleData.color);
				particle.getBoolean("additive", out particleData.additive);

				particle.getNumber("randomVelocity", out particleData.randomVelocity);
				particle.getNumber("randomRotationSpeed", out particleData.randomRotationSpeed);
				particle.getNumber("randomLifetime", out particleData.randomLifetime);

				if (particle.getVector2("sizeAnim", out Vector2 sizeAnim))
					particleData.sizeAnim = new Gradient<float>(sizeAnim.x, sizeAnim.y);
				if (particle.getVector4("colorAnimStart", out Vector4 colorAnimStart))
				{
					particle.getVector4("colorAnimEnd", out Vector4 colorAnimEnd);
					particleData.colorAnim = new Gradient<Vector4>(colorAnimStart, colorAnimEnd);
				}

				entity.particles.Add(particleData);
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
