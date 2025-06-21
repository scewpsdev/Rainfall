using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;


public class MapLoader
{
	public static void Run(WorldManager world, Scene scene)
	{
		/*
		LoadMap([
			"level/testmap/testmap1.gltf",
			"level/testmap/testmap2.gltf"
		],
		[
			"level/testmap/testmap1_collider.gltf",
			"level/testmap/testmap2_collider.gltf"
		],
		[
			"level/testmap/testmap_level.gltf"
		],
		world, scene);
		*/

		/*
		LoadMap([
			"level/prison/prison.gltf"
		],
		[
			"level/prison/prison_collider.gltf"
		],
		[
			"level/prison/prison_level.gltf"
		],
		world, scene);
		*/

		/*
		LoadMap([
			"level/tutorial/cave.gltf"
		],
		[
			"level/tutorial/cave_collider.gltf"
		],
		[
			"level/tutorial/cave_level.gltf"
		],
		world, scene);
		*/

		///*
		LoadMap([
			"level/cemetery/cemetery.gltf"
		],
		[
			"level/cemetery/cemetery_collider.gltf"
		],
		[
			"level/cemetery/cemetery_level.gltf"
		],
		world, scene);
		//*/

		//world.spawnPoint = Matrix.CreateTranslation(1.5f, 0, -2.5f);

		//scene.addEntity(new Hollow(), new Vector3(-2, 0, -2));
		//scene.addEntity(new ItemEntity(new KingsSword()), new Vector3(1, 0, -4));
		//scene.addEntity(new ItemEntity(new WoodenRoundShield()), new Vector3(2, 0, -4));

		//world.fogColor = new Vector3(0.3f);
		//world.fogStrength = 0.01f;

		//AudioManager.SetAmbientSound(Resource.GetSound("sound/ambient/dungeon_ambient_1.ogg"), 0.2f);
	}

	static void LoadMap(string[] models, string[] colliders, string[] nodes, WorldManager world, Scene scene)
	{
		foreach (string model in models)
			LoadModel(model, world, scene);
		foreach (string collider in colliders)
			LoadCollider(collider, world, scene);
		foreach (string node in nodes)
			LoadNodes(node, world, scene);
	}

	static void LoadModel(string path, WorldManager world, Scene scene)
	{
		Model model = Resource.GetModel(path);

		for (int i = 0; i < model.meshCount; i++)
		{
			string name = model.getMeshName(i);
			if (name.StartsWith("__water"))
			{
				WaterSurface water = new WaterSurface();
				water.model = model;
				water.meshIdx = i;
				water.isStatic = true;
				scene.addEntity(water);
			}
			else
			{
				Entity entity = new Entity();
				entity.model = model;
				entity.meshIdx = i;
				entity.isStatic = true;
				scene.addEntity(entity);
			}
		}

		{
			Entity entity = new Entity();
			entity.isStatic = true;

			for (int i = 0; i < model.lightCount; i++)
			{
				LightData light = model.getLight(i);
				Node lightNode = model.skeleton.getNode(light.nodeId);
				Vector3 lightPosition = lightNode.transform * light.position;
				if (light.type == LightType.Point)
				{
					entity.pointLights.Add(new PointLight(lightPosition, light.color, Renderer.graphics));
				}
				else
				{
					entity.directionalLight = new DirectionalLight(lightPosition, (lightNode.transform * new Vector4(light.direction, 0)).xyz, new Vector3(500, 500, 500), light.color, Renderer.graphics);
				}
			}

			scene.addEntity(entity);
		}
	}

	static void LoadCollider(string path, WorldManager world, Scene scene)
	{
		Model model = Resource.GetModel(path);
		Entity entity = new Entity();
		entity.body = new RigidBody(entity, RigidBodyType.Static);
		entity.body.addMeshColliders(model, Matrix.Identity);
		scene.addEntity(entity);
	}

	static void LoadNodes(string path, WorldManager world, Scene scene)
	{
		Model model = Resource.GetModel(path);

		Dictionary<string, Entity> entitiesWithID = new Dictionary<string, Entity>();
		Dictionary<string, Entity> entitiesLookingForID = new Dictionary<string, Entity>();

		for (int i = 0; i < model.skeleton.nodes.Length; i++)
		{
			Node node = model.skeleton.nodes[i];
			if (node.name.StartsWith("Cubemap"))
			{
				string name = null;
				if (node.name.StartsWith("Cubemap:"))
				{
					name = node.name.Substring(8);
					int spaceIdx = name.LastIndexOf(' ');
					if (spaceIdx != -1)
						name = name.Substring(0, spaceIdx);
				}
				Vector3 position = node.transform.translation;
				Vector3 size = node.transform.scale * 2;
				EnvironmentZone cubemap = new EnvironmentZone(size, name == "empty");
				scene.addEntity(cubemap, position);
			}
			else if (node.name.StartsWith("Spawn: "))
			{
				string spawnName = node.name.Substring(7);
				world.spawnPoints.Add(spawnName, node.transform);
			}
			else if (node.name.StartsWith("Item: "))
			{
				string itemName = node.name;
				if (itemName.LastIndexOf(' ') > 5)
					itemName = itemName.Substring(0, itemName.LastIndexOf(' '));
				itemName = itemName.Substring(6);
				Item item = Item.GetType(itemName);
				scene.addEntity(new ItemEntity(item.copy()), node.transform);
			}
			else if (node.name.StartsWith("Object: "))
			{
				string objectType = node.name;
				if (objectType.LastIndexOf(' ') > 7)
					objectType = objectType.Substring(0, objectType.LastIndexOf(' '));
				objectType = objectType.Substring(8);

				string objectArgs = null;
				int argDelim = objectType.LastIndexOf(':');
				if (argDelim != -1)
				{
					objectArgs = objectType.Substring(argDelim + 1);
					objectType = objectType.Substring(0, argDelim);
				}

				Entity entity = null;
				if (objectType == "ladder")
				{
					Debug.Assert(objectArgs != null);
					int height = int.Parse(objectArgs);
					entity = new Ladder(height);
				}
				else if (objectType == "crate")
				{
					entity = new Crate();
				}
				else if (objectType == "torch")
				{
					entity = new TorchEntity();
				}
				else if (objectType == "campfire")
				{
					entity = new Fireplace();
				}
				else
				{
					Debug.Assert(false);
				}
				scene.addEntity(entity, node.transform);
			}

			foreach (var pair in entitiesLookingForID)
			{
				if (entitiesWithID.TryGetValue(pair.Key, out Entity entity))
				{
					//if (pair.Value is Lever)
					//{
					//	(pair.Value as Lever).activatable = (Activatable)entity;
					//}
				}
			}
		}
	}
}
