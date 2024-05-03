using Rainfall;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public static class SceneFormat
{
	public enum ColliderType
	{
		Box,
		Sphere,
		Capsule,
		Mesh,
		ConvexMesh,
	}

	public struct ColliderData
	{
		public ColliderType type;
		public bool trigger;

		public Vector3 size;
		public Vector3 offset;
		public Vector3 eulers;

		public string meshColliderPath;
		public Model meshCollider;


		public ColliderData(Vector3 size, Vector3 offset = default, Vector3 eulers = default)
		{
			type = ColliderType.Box;
			this.size = size;
			this.offset = offset;
			this.eulers = eulers;
		}

		public ColliderData(float radius, Vector3 offset = default, Vector3 eulers = default)
		{
			type = ColliderType.Sphere;
			size = new Vector3(2 * radius, 0, 0);
			this.offset = offset;
			this.eulers = eulers;
		}

		public ColliderData(float radius, float height, Vector3 offset = default, Vector3 eulers = default)
		{
			type = ColliderType.Capsule;
			size = new Vector3(2 * radius, height, 0);
			this.offset = offset;
			this.eulers = eulers;
		}

		public ColliderData(string path, Vector3 offset = default, Vector3 eulers = default)
		{
			type = ColliderType.Mesh;
			meshColliderPath = path;
			this.offset = offset;
			this.eulers = eulers;
		}

		public float radius
		{
			get => 0.5f * size.x;
			set { size.x = value * 2; }
		}
		public float height
		{
			get => size.y;
			set { size.y = value; }
		}

		public override bool Equals(object obj)
		{
			if (obj is ColliderData)
				return (ColliderData)obj == this;
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator ==(ColliderData a, ColliderData b)
		{
			return a.type == b.type && a.size == b.size && a.offset == b.offset && a.eulers == b.eulers && a.meshColliderPath == b.meshColliderPath && a.meshCollider == b.meshCollider;
		}

		public static bool operator !=(ColliderData a, ColliderData b) => !(a == b);
	}

	public struct LightData
	{
		public Vector3 color;
		public float intensity;
		public Vector3 offset;


		public LightData(Vector3 color, float intensity, Vector3 offset = default)
		{
			this.color = color;
			this.intensity = intensity;
			this.offset = offset;
		}

		public override bool Equals(object obj)
		{
			if (obj is LightData)
				return (LightData)obj == this;
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator ==(LightData a, LightData b)
		{
			return a.color == b.color && a.intensity == b.intensity && a.offset == b.offset;
		}

		public static bool operator !=(LightData a, LightData b) => !(a == b);
	}

	public struct EntityData
	{
		public string name;
		public uint id;
		public bool isStatic;

		public Vector3 position;
		public Quaternion rotation;
		public Vector3 scale;

		public string modelPath;
		public Model model;

		public RigidBodyType rigidBodyType;
		public List<ColliderData> colliders;
		public Dictionary<string, ColliderData> boneColliders = null;

		public List<LightData> lights;
		public List<ParticleSystemData> particles;

		public EntityData(string name, uint id)
		{
			this.name = name;
			this.id = id;

			isStatic = false;

			rotation = Quaternion.Identity;
			scale = Vector3.One;

			colliders = new List<ColliderData>();

			lights = new List<LightData>();
			particles = new List<ParticleSystemData>();
		}

		public unsafe void load(string directory)
		{
			if (modelPath != null)
			{
				model = Resource.GetModel(directory + "/" + modelPath);
				model.isStatic = isStatic;
			}
			for (int i = 0; i < colliders.Count; i++)
			{
				ColliderData collider = colliders[i];
				if ((collider.type == ColliderType.Mesh || collider.type == SceneFormat.ColliderType.ConvexMesh) && collider.meshColliderPath != null)
					collider.meshCollider = Resource.GetModel(directory + "/" + collider.meshColliderPath);
				colliders[i] = collider;
			}
			for (int i = 0; i < particles.Count; i++)
			{
				ParticleSystemData system = particles[i];
				if (system.textureAtlasPath[0] != 0)
					system.textureAtlas = Resource.GetTexture(directory + "/" + new string((sbyte*)system.textureAtlasPath)).handle;
			}
		}
	}


	static unsafe DatObject SerializeEntity(EntityData entity)
	{
		DatObject obj = new DatObject();

		obj.addString("name", entity.name);
		obj.addString("id", entity.id.ToString());
		obj.addBoolean("static", entity.isStatic);
		obj.addVector3("position", entity.position);
		obj.addQuaternion("rotation", entity.rotation);
		obj.addVector3("scale", entity.scale);

		if (entity.modelPath != null)
			obj.addString("model", entity.modelPath);

		if (entity.colliders.Count > 0)
			obj.addIdentifier("rigidBodyType", entity.rigidBodyType.ToString());

		DatArray colliders = new DatArray();
		for (int i = 0; i < entity.colliders.Count; i++)
		{
			DatObject collider = new DatObject();
			collider.addIdentifier("type", entity.colliders[i].type.ToString());
			collider.addBoolean("trigger", entity.colliders[i].trigger);
			collider.addVector3("size", entity.colliders[i].size);
			collider.addVector3("offset", entity.colliders[i].offset);
			collider.addVector3("rotation", entity.colliders[i].eulers);
			if (entity.colliders[i].type == ColliderType.Mesh || entity.colliders[i].type == ColliderType.ConvexMesh)
			{
				if (entity.colliders[i].meshColliderPath != null)
					collider.addString("mesh", entity.colliders[i].meshColliderPath);
			}
			colliders.addObject(collider);
		}
		obj.addArray("colliders", colliders);

		if (entity.boneColliders != null && entity.model != null)
		{
			DatArray boneColliders = new DatArray();

			foreach (string nodeName in entity.boneColliders.Keys)
			{
				DatObject collider = new DatObject();
				collider.addString("bone", nodeName);
				collider.addIdentifier("type", entity.boneColliders[nodeName].type.ToString());
				collider.addBoolean("trigger", entity.boneColliders[nodeName].trigger);
				collider.addVector3("size", entity.boneColliders[nodeName].size);
				collider.addVector3("offset", entity.boneColliders[nodeName].offset);
				collider.addVector3("rotation", entity.boneColliders[nodeName].eulers);
				boneColliders.addObject(collider);
			}

			obj.addArray("boneColliders", boneColliders);
		}

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
			ParticleSystemData particleData = entity.particles[i];

			DatObject particle = new DatObject();

			//particle.addString("name", particleData.name);

			particle.addNumber("lifetime", particleData.lifetime);
			particle.addNumber("size", particleData.size);
			particle.addBoolean("follow", particleData.follow != 0);

			particle.addNumber("emissionRate", particleData.emissionRate);
			particle.addIdentifier("spawnShape", particleData.spawnShape.ToString());
			particle.addVector3("spawnOffset", particleData.spawnOffset);
			if (particleData.spawnShape == ParticleSpawnShape.Circle || particleData.spawnShape == ParticleSpawnShape.Sphere || particleData.spawnShape == ParticleSpawnShape.Line)
				particle.addNumber("spawnRadius", particleData.spawnRadius);
			if (particleData.spawnShape == ParticleSpawnShape.Line)
				particle.addVector3("lineEnd", particleData.lineSpawnEnd);

			particle.addNumber("gravity", particleData.gravity);
			particle.addNumber("drag", particleData.drag);
			particle.addVector3("startVelocity", particleData.startVelocity);
			particle.addNumber("radialVelocity", particleData.radialVelocity);
			particle.addNumber("startRotation", particleData.startRotation);
			particle.addNumber("rotationSpeed", particleData.rotationSpeed);
			particle.addBoolean("applyEntityVelocity", particleData.applyEntityVelocity);
			particle.addBoolean("applyCentrifugalForce", particleData.applyCentrifugalForce);

			if (particleData.textureAtlasPath[0] != 0)
			{
				particle.addString("textureAtlas", new string((sbyte*)particleData.textureAtlasPath));
				particle.addVector2("atlasSize", (Vector2)particleData.atlasSize);
				particle.addNumber("numFrames", particleData.numFrames);
				particle.addBoolean("linearFiltering", particleData.linearFiltering != 0);
			}

			particle.addVector4("color", particleData.color);
			particle.addBoolean("additive", particleData.additive != 0);
			particle.addNumber("emissiveIntensity", particleData.emissiveIntensity);
			particle.addNumber("lightInfluence", particleData.lightInfluence);

			particle.addVector3("randomVelocity", particleData.randomVelocity);
			particle.addNumber("randomRotation", particleData.randomRotation);
			particle.addNumber("randomRotationSpeed", particleData.randomRotationSpeed);
			particle.addNumber("randomLifetime", particleData.randomLifetime);
			particle.addNumber("velocityNoise", particleData.velocityNoise);

			if (particleData.sizeAnim.count > 0)
				particle.addVector2("sizeAnim", new Vector2(particleData.sizeAnim.value0.value, particleData.sizeAnim.value1.value));
			if (particleData.colorAnim.count > 0)
			{
				particle.addVector4("colorAnimStart", particleData.colorAnim.value0.value);
				particle.addVector4("colorAnimEnd", particleData.colorAnim.value1.value);
			}

			if (particleData.bursts != null)
			{
				DatArray bursts = new DatArray();

				for (int j = 0; j < particleData.numBursts; j++)
				{
					DatObject burst = new DatObject();

					burst.addNumber("time", particleData.bursts[j].time);
					burst.addInteger("count", particleData.bursts[j].count);
					burst.addNumber("duration", particleData.bursts[j].duration);

					bursts.addObject(burst);
				}

				particle.addArray("bursts", bursts);
			}

			particles.addObject(particle);
		}
		obj.addArray("particles", particles);

		return obj;
	}

	public static void SerializeScene(List<EntityData> entities, uint selectedEntity, Stream stream)
	{
		DatFile file = new DatFile();

		/*
		file.addVector3("camera_target", instance.camera.target);
		file.addNumber("camera_distance", instance.camera.distance);
		file.addNumber("camera_pitch", instance.camera.pitch);
		file.addNumber("camera_yaw", instance.camera.yaw);
		*/

		DatArray arr = new DatArray();
		for (int i = 0; i < entities.Count; i++)
		{
			DatObject obj = SerializeEntity(entities[i]);
			arr.addObject(obj);
		}
		file.addArray("entities", arr);

		file.addString("selectedEntity", selectedEntity.ToString());

		file.serialize(stream);
	}

	public static byte[] SerializeScene(List<EntityData> entities, uint selectedEntity)
	{
		MemoryStream stream = new MemoryStream();
		SerializeScene(entities, selectedEntity, stream);
		byte[] data = stream.ToArray();
		stream.Close();
		return data;
	}

	static unsafe EntityData DeserializeEntity(DatObject obj)
	{
		obj.getStringContent("name", out string name);
		obj.getStringContent("id", out string idStr);
		EntityData entity = new EntityData(name, uint.Parse(idStr));

		obj.getBoolean("static", out entity.isStatic);

		obj.getVector3("position", out entity.position);
		obj.getQuaternion("rotation", out entity.rotation);
		obj.getVector3("scale", out entity.scale);
		obj.getStringContent("model", out entity.modelPath);

		if (obj.getIdentifier("rigidBodyType", out string rigidBodyType))
			entity.rigidBodyType = Utils.ParseEnum<RigidBodyType>(rigidBodyType);

		if (obj.getArray("colliders", out DatArray colliders))
		{
			for (int i = 0; i < colliders.size; i++)
			{
				ColliderData collider = new ColliderData();
				if (colliders[i].obj.getIdentifier("type", out string type))
					collider.type = Utils.ParseEnum<ColliderType>(type);
				colliders[i].obj.getBoolean("trigger", out collider.trigger);
				if (colliders[i].obj.getVector3("size", out Vector3 size))
					collider.size = size;
				colliders[i].obj.getVector3("offset", out collider.offset);
				colliders[i].obj.getVector3("rotation", out collider.eulers);
				colliders[i].obj.getStringContent("mesh", out collider.meshColliderPath);
				entity.colliders.Add(collider);
			}
		}

		if (obj.getArray("boneColliders", out DatArray boneColliders))
		{
			entity.boneColliders = new Dictionary<string, ColliderData>();

			for (int i = 0; i < boneColliders.size; i++)
			{
				DatObject collider = boneColliders[i].obj;
				collider.getStringContent("bone", out string nodeName);

				ColliderData colliderData = new ColliderData();
				if (collider.getIdentifier("type", out string type))
					colliderData.type = Utils.ParseEnum<ColliderType>(type);
				collider.getBoolean("trigger", out colliderData.trigger);
				if (collider.getVector3("size", out Vector3 size))
					colliderData.size = size;
				collider.getVector3("offset", out colliderData.offset);
				collider.getVector3("rotation", out colliderData.eulers);

				entity.boneColliders.Add(nodeName, colliderData);
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
				ParticleSystemData particleData = new ParticleSystemData(0);

				//particle.getStringContent("name", out particleData.name);

				particle.getNumber("lifetime", out particleData.lifetime);
				particle.getNumber("size", out particleData.size);
				if (particle.getBoolean("follow", out bool follow))
					particleData.follow = (byte)(follow ? 1 : 0);

				particle.getNumber("emissionRate", out particleData.emissionRate);
				if (particle.getIdentifier("spawnShape", out string spawnShape))
					particleData.spawnShape = Utils.ParseEnum<ParticleSpawnShape>(spawnShape);
				particle.getVector3("spawnOffset", out particleData.spawnOffset);
				particle.getNumber("spawnRadius", out particleData.spawnRadius);
				particle.getVector3("lineEnd", out particleData.lineSpawnEnd);

				particle.getNumber("gravity", out particleData.gravity);
				particle.getNumber("drag", out particleData.drag);
				particle.getVector3("startVelocity", out particleData.startVelocity);
				particle.getNumber("radialVelocity", out particleData.radialVelocity);
				particle.getNumber("startRotation", out particleData.startRotation);
				particle.getNumber("rotationSpeed", out particleData.rotationSpeed);
				particle.getBoolean("applyEntityVelocity", out particleData.applyEntityVelocity);
				particle.getBoolean("applyCentrifugalForce", out particleData.applyCentrifugalForce);

				if (particle.getStringContent("textureAtlas", out string atlasPath))
				{
					StringUtils.WriteString(particleData.textureAtlasPath, atlasPath);
					if (particle.getVector2("atlasSize", out Vector2 atlasSize))
						particleData.atlasSize = (Vector2i)Vector2.Round(atlasSize);
					particle.getInteger("numFrames", out particleData.numFrames);
					if (particle.getBoolean("linearFiltering", out bool linearFiltering))
						particleData.linearFiltering = (byte)(linearFiltering ? 1 : 0);
				}

				particle.getVector4("color", out particleData.color);
				if (particle.getBoolean("additive", out bool additive))
				{
					particleData.additive = (byte)(additive ? 1 : 0);

					// for compatibility reasons
					if (additive)
					{
						particleData.emissiveIntensity = 1;
						particleData.lightInfluence = 0;
					}
					else
					{
						particleData.emissiveIntensity = 0;
						particleData.lightInfluence = 1;
					}
				}
				else
					particle.getNumber("emissiveIntensity", out particleData.emissiveIntensity);

				particle.getVector3("randomVelocity", out particleData.randomVelocity);
				particle.getNumber("randomRotation", out particleData.randomRotation);
				particle.getNumber("randomRotationSpeed", out particleData.randomRotationSpeed);
				particle.getNumber("randomLifetime", out particleData.randomLifetime);
				particle.getNumber("velocityNoise", out particleData.velocityNoise);

				if (particle.getVector2("sizeAnim", out Vector2 sizeAnim))
					particleData.sizeAnim = new Gradient_float_2 { value0 = { value = sizeAnim.x, position = 0 }, value1 = { value = sizeAnim.y, position = 1 }, count = 2 };
				if (particle.getVector4("colorAnimStart", out Vector4 colorAnimStart))
				{
					particle.getVector4("colorAnimEnd", out Vector4 colorAnimEnd);
					particleData.colorAnim = new Gradient_Vector4_2 { value0 = { value = colorAnimStart, position = 0 }, value1 = { value = colorAnimEnd, position = 1 }, count = 2 };
				}

				if (particle.getArray("bursts", out DatArray bursts))
				{
					particleData.numBursts = bursts.size;
					particleData.bursts = (ParticleBurst*)Marshal.AllocHGlobal(bursts.size * sizeof(ParticleBurst));
					for (int j = 0; j < bursts.size; j++)
					{
						DatObject burst = bursts[j].obj;

						burst.getNumber("time", out float time);
						burst.getInteger("count", out int count);
						burst.getNumber("duration", out float duration);

						particleData.bursts[j] = new ParticleBurst { time = time, count = count, duration = duration };
					}
				}

				entity.particles.Add(particleData);
			}
		}

		return entity;
	}

	public static void DeserializeScene(Stream stream, out List<EntityData> entities, out uint selectedEntity)
	{
		entities = new List<EntityData>();
		selectedEntity = 0;

		DatFile file = new DatFile(stream);

		/*
		if (file.getVector3("camera_target", out Vector3 cameraTarget))
			instance.camera.target = cameraTarget;
		if (file.getNumber("camera_distance", out float cameraDistance))
			instance.camera.distance = cameraDistance;
		if (file.getNumber("camera_pitch", out float cameraPitch))
			instance.camera.pitch = cameraPitch;
		if (file.getNumber("camera_yaw", out float cameraYaw))
			instance.camera.yaw = cameraYaw;
		*/

		if (file.getArray("entities", out DatArray arr))
		{
			for (int i = 0; i < arr.size; i++)
			{
				if (arr[i].type == DatValueType.Object)
				{
					EntityData entity = DeserializeEntity(arr[i].obj);
					entities.Add(entity);
				}
				else
				{
					Debug.Assert(false);
				}
			}
		}

		if (file.getStringContent("selectedEntity", out string selectedEntityIDStr))
			selectedEntity = uint.Parse(selectedEntityIDStr);
	}

	public static void DeserializeScene(byte[] data, out List<EntityData> entities, out uint selectedEntity)
	{
		MemoryStream stream = new MemoryStream(data);
		DeserializeScene(stream, out entities, out selectedEntity);
		stream.Close();
	}
}
