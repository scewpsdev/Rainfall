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
		world.spawnPoint = Matrix.Identity;

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
		scene);
		*/

		LoadMap([
			"level/prison/prison.gltf"
		],
		[
			"level/prison/prison_collider.gltf"
		],
		[
			"level/prison/prison_level.gltf"
		],
		scene);

		world.spawnPoint = Matrix.CreateTranslation(1.5f, 0, -2.5f);

		scene.addEntity(new Hollow(), new Vector3(-2, 0, -2));
		//scene.addEntity(new ItemEntity(new KingsSword()), new Vector3(1, 0, -4));
		//scene.addEntity(new ItemEntity(new WoodenRoundShield()), new Vector3(2, 0, -4));

		world.fogColor = new Vector3(0.3f);
		world.fogStrength = 0.01f;

		AudioManager.SetAmbientSound(Resource.GetSound("sound/ambient/dungeon_ambient_1.ogg"), 0.2f);
	}

	static void LoadMap(string[] models, string[] colliders, string[] nodes, Scene scene)
	{
		foreach (string model in models)
			LoadModel(model, scene);
		foreach (string collider in colliders)
			LoadCollider(collider, scene);
		foreach (string node in nodes)
			LoadNodes(node, scene);
	}

	static void LoadModel(string path, Scene scene)
	{
		Model model = Resource.GetModel(path);
		Entity entity = new Entity() { model = model };
		entity.createModelLights();
		scene.addEntity(entity);
	}

	static void LoadCollider(string path, Scene scene)
	{
		Model model = Resource.GetModel(path);
		Entity entity = new Entity();
		entity.body = new RigidBody(entity, RigidBodyType.Static);
		entity.body.addMeshColliders(model, Matrix.Identity);
		scene.addEntity(entity);
	}

	static void LoadNodes(string path, Scene scene)
	{
		Model model = Resource.GetModel(path);

		Dictionary<string, Entity> entitiesWithID = new Dictionary<string, Entity>();
		Dictionary<string, Entity> entitiesLookingForID = new Dictionary<string, Entity>();

		for (int i = 0; i < model.skeleton.nodes.Length; i++)
		{
			Node node = model.skeleton.nodes[i];
			if (node.name.StartsWith("Item: "))
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
				else if (objectType == "prison_gate")
				{
					Debug.Assert(objectArgs != null);
					entity = new PrisonGate();
					entitiesWithID.Add(objectArgs, entity);
				}
				else if (objectType == "lever")
				{
					Debug.Assert(objectArgs != null);
					entity = new Lever();
					entitiesLookingForID.Add(objectArgs, entity);
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
					if (pair.Value is Lever)
					{
						(pair.Value as Lever).activatable = (Activatable)entity;
					}
				}
			}
		}
	}
}
