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
		scene);
		*/

		world.spawnPoint = Matrix.CreateTranslation(1.5f, 0, -2.5f);

		scene.addEntity(new Hollow(), new Vector3(-2, 0, -2));
		scene.addEntity(new ItemEntity(new KingsSword()), new Vector3(1, 0, -4));
		scene.addEntity(new ItemEntity(new WoodenRoundShield()), new Vector3(2, 0, -4));

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
				string objectName = node.name;
				if (objectName.LastIndexOf(' ') > 7)
					objectName = objectName.Substring(0, objectName.LastIndexOf(' '));
				objectName = objectName.Substring(8);

				Entity entity = null;
				if (objectName.StartsWith("ladder"))
				{
					int height = int.Parse(objectName.Substring(7));
					entity = new Ladder(height);
				}
				else if (objectName.StartsWith("crate"))
				{
					entity = new Crate();
				}
				scene.addEntity(entity, node.transform);
			}
		}
	}
}
