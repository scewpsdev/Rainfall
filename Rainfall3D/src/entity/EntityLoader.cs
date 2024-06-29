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
		static Dictionary<string, List<SceneFormat.EntityData>> entityDataCache = new Dictionary<string, List<SceneFormat.EntityData>>();
		static Dictionary<Model, MeshCollider> meshColliderCache = new Dictionary<Model, MeshCollider>();
		static Dictionary<Model, ConvexMeshCollider> convexMeshColliderCache = new Dictionary<Model, ConvexMeshCollider>();


		static string CombinePath(string path, string root)
		{
			root = Path.GetDirectoryName(root);
			return root + "/" + path;
		}

		public static List<SceneFormat.EntityData> LoadEntities(string path)
		{
			List<SceneFormat.EntityData> entities;
			if (entityDataCache.ContainsKey(path))
			{
				entities = entityDataCache[path];
			}
			else
			{
				FileStream stream = new FileStream(path + ".bin", FileMode.Open);
				SceneFormat.DeserializeScene(stream, out entities, out uint selectedEntity);
				stream.Close();

				entityDataCache.Add(path, entities);
			}
			return entities;
		}

		public static void CreateEntityFromData(string path, Entity entity)
		{
			List<SceneFormat.EntityData> entities = LoadEntities(path);

			Debug.Assert(entities.Count == 1);
			SceneFormat.EntityData entityData = entities[0];

			CreateEntityFromData(entityData, path, entity);
		}

		public static unsafe void CreateEntityFromData(SceneFormat.EntityData entityData, string path, Entity entity)
		{
			entity.name = entityData.name;
			entity.isStatic = entityData.isStatic;

			entity.position = entityData.position;
			entity.rotation = entityData.rotation;
			entity.scale = entityData.scale;

			if (entityData.modelPath != null)
			{
				entity.model = Resource.GetModel(CombinePath(entityData.modelPath, path));
				if (entity.model != null)
				{
					entity.model.isStatic = entityData.isStatic;
					if (entity.model.isAnimated)
						entity.animator = Animator.Create(entity.model);
				}
			}

			if (entityData.colliders.Count > 0)
			{
				Vector3 centerOfMass = Vector3.Zero;
				if (entity.model != null)
					centerOfMass = entity.model.boundingSphere.center;
				entity.body = new RigidBody(entity, entity.bodyType != RigidBodyType.Null ? entity.bodyType : entityData.rigidBodyType, 1.0f, centerOfMass, entity.bodyFilterGroup, entity.bodyFilterMask);
				for (int i = 0; i < entityData.colliders.Count; i++)
				{
					SceneFormat.ColliderData colliderData = entityData.colliders[i];
					if (colliderData.trigger)
					{
						switch (colliderData.type)
						{
							case SceneFormat.ColliderType.Box:
								entity.body.addBoxTrigger(colliderData.size * 0.5f, colliderData.offset, Quaternion.FromEulerAngles(colliderData.eulers));
								break;
							case SceneFormat.ColliderType.Sphere:
								entity.body.addSphereTrigger(colliderData.radius, colliderData.offset);
								break;
							case SceneFormat.ColliderType.Capsule:
								entity.body.addCapsuleTrigger(colliderData.radius, colliderData.height, colliderData.offset, Quaternion.FromEulerAngles(colliderData.eulers));
								break;
							case SceneFormat.ColliderType.Mesh:
								if (colliderData.meshColliderPath != null)
								{
									Model model = Resource.GetModel(CombinePath(colliderData.meshColliderPath, path));
									meshColliderCache.TryGetValue(model, out MeshCollider meshCollider);
									if (meshCollider == null)
									{
										meshCollider = Physics.CreateMeshCollider(model);
										meshColliderCache.Add(model, meshCollider);
									}
									entity.body.addMeshTrigger(meshCollider, Matrix.CreateTranslation(colliderData.offset));
								}
								break;
							case SceneFormat.ColliderType.ConvexMesh:
								if (colliderData.meshColliderPath != null)
								{
									Model model = Resource.GetModel(CombinePath(colliderData.meshColliderPath, path));
									convexMeshColliderCache.TryGetValue(model, out ConvexMeshCollider meshCollider);
									if (meshCollider == null)
									{
										meshCollider = Physics.CreateConvexMeshCollider(model);
										convexMeshColliderCache.Add(model, meshCollider);
									}
									entity.body.addConvexMeshTrigger(meshCollider, Matrix.CreateTranslation(colliderData.offset));
								}
								break;
							default:
								Debug.Assert(false);
								break;
						}
					}
					else
					{
						switch (colliderData.type)
						{
							case SceneFormat.ColliderType.Box:
								entity.body.addBoxCollider(colliderData.size * 0.5f, colliderData.offset, Quaternion.FromEulerAngles(colliderData.eulers), entity.bodyFriction);
								break;
							case SceneFormat.ColliderType.Sphere:
								entity.body.addSphereCollider(colliderData.radius, colliderData.offset, entity.bodyFriction);
								break;
							case SceneFormat.ColliderType.Capsule:
								entity.body.addCapsuleCollider(colliderData.radius, colliderData.height, colliderData.offset, Quaternion.FromEulerAngles(colliderData.eulers), entity.bodyFriction);
								break;
							case SceneFormat.ColliderType.Mesh:
								if (colliderData.meshColliderPath != null)
								{
									Model model = Resource.GetModel(CombinePath(colliderData.meshColliderPath, path));
									meshColliderCache.TryGetValue(model, out MeshCollider meshCollider);
									if (meshCollider == null)
									{
										meshCollider = Physics.CreateMeshCollider(model);
										meshColliderCache.Add(model, meshCollider);
									}
									entity.body.addMeshCollider(meshCollider, Matrix.CreateTranslation(colliderData.offset), entity.bodyFriction);
								}
								break;
							case SceneFormat.ColliderType.ConvexMesh:
								if (colliderData.meshColliderPath != null)
								{
									Model model = Resource.GetModel(CombinePath(colliderData.meshColliderPath, path));
									convexMeshColliderCache.TryGetValue(model, out ConvexMeshCollider meshCollider);
									if (meshCollider == null)
									{
										meshCollider = Physics.CreateConvexMeshCollider(model);
										convexMeshColliderCache.Add(model, meshCollider);
									}
									entity.body.addConvexMeshCollider(meshCollider, Matrix.CreateTranslation(colliderData.offset), entity.bodyFriction);
								}
								break;
							default:
								Debug.Assert(false);
								break;
						}
					}
				}
			}

			if (entityData.boneColliders != null)
			{
				entity.hitboxData = new Dictionary<string, SceneFormat.ColliderData>();
				entity.hitboxes = new Dictionary<string, RigidBody>();

				foreach (string nodeName in entityData.boneColliders.Keys)
				{
					RigidBody boneCollider = new RigidBody(entity, RigidBodyType.Kinematic, entity.hitboxFilterGroup, entity.hitboxFilterMask);
					entity.hitboxData.Add(nodeName, entityData.boneColliders[nodeName]);
					entity.hitboxes.Add(nodeName, boneCollider);

					SceneFormat.ColliderData colliderData = entityData.boneColliders[nodeName];
					if (colliderData.trigger)
					{
						switch (colliderData.type)
						{
							case SceneFormat.ColliderType.Box:
								boneCollider.addBoxTrigger(colliderData.size * 0.5f, colliderData.offset, Quaternion.FromEulerAngles(colliderData.eulers));
								break;
							case SceneFormat.ColliderType.Sphere:
								boneCollider.addSphereTrigger(colliderData.radius, colliderData.offset);
								break;
							case SceneFormat.ColliderType.Capsule:
								boneCollider.addCapsuleTrigger(colliderData.radius, colliderData.height, colliderData.offset, Quaternion.FromEulerAngles(colliderData.eulers));
								break;
							default:
								Debug.Assert(false);
								break;
						}
					}
					else
					{
						switch (colliderData.type)
						{
							case SceneFormat.ColliderType.Box:
								boneCollider.addBoxCollider(colliderData.size * 0.5f, colliderData.offset, Quaternion.FromEulerAngles(colliderData.eulers));
								break;
							case SceneFormat.ColliderType.Sphere:
								boneCollider.addSphereCollider(colliderData.radius, colliderData.offset);
								break;
							case SceneFormat.ColliderType.Capsule:
								boneCollider.addCapsuleCollider(colliderData.radius, colliderData.height, colliderData.offset, Quaternion.FromEulerAngles(colliderData.eulers));
								break;
							default:
								Debug.Assert(false);
								break;
						}
					}
				}
			}

			for (int i = 0; i < entityData.lights.Count; i++)
			{
				SceneFormat.LightData lightData = entityData.lights[i];
				PointLight light;
				if (entityData.isStatic)
					light = new PointLight(lightData.offset, lightData.color * lightData.intensity, Renderer.graphics);
				else
					light = new PointLight(lightData.offset, lightData.color * lightData.intensity);
				entity.lights.Add(light);
			}

			for (int i = 0; i < entityData.particles.Count; i++)
			{
				ParticleSystem particles = ParticleSystem.Create(Matrix.Identity);
				*particles.handle = entityData.particles[i];
				if (particles.handle->textureAtlasPath[0] != 0)
					particles.handle->textureAtlas = Resource.GetTexture(CombinePath(new string((sbyte*)particles.handle->textureAtlasPath), path)).handle;
				entity.particles.Add(particles);
			}
		}

		public static T Load<T>(string path) where T : Entity, new()
		{
			T t = new T();
			CreateEntityFromData(path, t);
			return t;
		}

		public static Entity Load(string path)
		{
			return Load<Entity>(path);
		}
	}
}
