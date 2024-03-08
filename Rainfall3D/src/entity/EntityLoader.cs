using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;


namespace Rainfall
{
	public static class EntityLoader
	{
		static Dictionary<Model, MeshCollider> meshColliderCache = new Dictionary<Model, MeshCollider>();


		static string CombinePath(string path, string root)
		{
			root = Path.GetDirectoryName(root);
			return root + "/" + path;
		}

		public static Entity CreateEntityFromData(SceneFormat.EntityData entityData, string path)
		{
			Entity entity = new Entity();

			entity.name = entityData.name;
			entity.isStatic = entityData.isStatic;

			entity.position = entityData.position;
			entity.rotation = entityData.rotation;
			entity.scale = entityData.scale;

			if (entityData.modelPath != null)
			{
				entity.model = Resource.GetModel(CombinePath(entityData.modelPath, path));
				if (entity.model != null)
					entity.model.isStatic = entityData.isStatic;
			}

			if (entityData.colliders.Count > 0)
				entity.body = new RigidBody(entity, entityData.rigidBodyType);
			for (int i = 0; i < entityData.colliders.Count; i++)
			{
				SceneFormat.ColliderData colliderData = entityData.colliders[i];
				switch (colliderData.type)
				{
					case SceneFormat.ColliderType.Box:
						entity.body.addBoxCollider(colliderData.size * 0.5f, colliderData.offset, Quaternion.FromEulerAngles(colliderData.eulers));
						break;
					case SceneFormat.ColliderType.Sphere:
						entity.body.addSphereCollider(colliderData.radius, colliderData.offset);
						break;
					case SceneFormat.ColliderType.Capsule:
						entity.body.addCapsuleCollider(colliderData.radius, colliderData.height, colliderData.offset, Quaternion.FromEulerAngles(colliderData.eulers));
						break;
					case SceneFormat.ColliderType.Mesh:
						if (colliderData.meshColliderPath != null)
						{
							Model model = Resource.GetModel(CombinePath(colliderData.meshColliderPath, path));
							meshColliderCache.TryGetValue(model, out MeshCollider meshCollider);
							if (meshCollider == null)
							{
								meshCollider = Physics.CreateMeshCollider(model, 0);
								meshColliderCache.Add(model, meshCollider);
							}
							entity.body.addMeshCollider(meshCollider, Matrix.CreateTranslation(colliderData.offset));
						}
						break;
					default:
						Debug.Assert(false);
						break;
				}
			}

			for (int i = 0; i < entityData.lights.Count; i++)
			{
				SceneFormat.LightData lightData = entityData.lights[i];
				PointLight light = new PointLight(lightData.offset, lightData.color * lightData.intensity, Renderer.graphics);
				entity.lights.Add(light);
			}

			for (int i = 0; i < entityData.particles.Count; i++)
			{
				ParticleSystem particles = new ParticleSystem(1000);
				particles.copyData(entityData.particles[0]);
				if (particles.textureAtlasPath != null)
					particles.textureAtlas = Resource.GetTexture(CombinePath(particles.textureAtlasPath, path));
				entity.particles.Add(particles);
			}

			return entity;
		}

		public static Entity Load(string path)
		{
			FileStream stream = new FileStream(path + ".bin", FileMode.Open);
			SceneFormat.DeserializeScene(stream, out List<SceneFormat.EntityData> entities, out uint selectedEntity);
			stream.Close();

			Debug.Assert(entities.Count == 1);

			SceneFormat.EntityData entityData = entities[0];

			return CreateEntityFromData(entityData, path);
		}
	}
}
