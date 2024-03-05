using System;
using System.Collections.Generic;
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

		public static Entity Load(string path)
		{
			FileStream stream = new FileStream(path + ".bin", FileMode.Open);
			SceneFormat.DeserializeScene(stream, out List<SceneFormat.EntityData> entities, out uint selectedEntity);
			stream.Close();

			Debug.Assert(entities.Count == 1);

			SceneFormat.EntityData entityData = entities[0];
			Entity entity = new Entity();

			entity.name = entityData.name;
			entity.position = entityData.position;
			entity.rotation = entityData.rotation;
			entity.scale = entityData.scale;
			if (entityData.modelPath != null)
			{
				entity.model = Resource.GetModel(CombinePath(entityData.modelPath, path));
				entity.model.isStatic = entityData.isStatic;
			}
			entity.body = entityData.colliders.Count > 0 ? new RigidBody(entity, entityData.isStatic ? RigidBodyType.Static : RigidBodyType.Kinematic) : null;

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

			if (entityData.particles.Count > 0)
			{
				entity.particles = new ParticleSystem(1000);
				entity.particles.copyData(entityData.particles[0]);
				if (entity.particles.textureAtlasPath != null)
					entity.particles.textureAtlas = Resource.GetTexture(CombinePath(entity.particles.textureAtlasPath, path));
			}

			return entity;
		}
	}
}
