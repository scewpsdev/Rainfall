using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WorldManager : Entity
{
	Scene scene;

	MapPiece map1;

	DirectionalLight sun;
	List<Cubemap> skyboxes = new List<Cubemap>();

	public Matrix spawnPoint = Matrix.Identity;

	public Cubemap skybox;


	public override void init()
	{
		scene = GameState.instance.scene;

		sun = new DirectionalLight(new Vector3(-1, -1, 1).normalized, new Vector3(1.0f, 0.9f, 0.7f) * 3, Renderer.graphics);
		Cubemap globalSkybox = Resource.GetCubemap("level/cubemap_equirect.png");
		skyboxes.Add(globalSkybox);

		//map1 = loadMap(1);
		//spawnPoint = map1.spawnPoint;
		scene.addEntity(new Entity().load("level/testmap/testmap.rfs"));

		scene.addEntity(new Fireplace(), new Vector3(-1, 0, -2));

		scene.addEntity(new ItemEntity(new Longsword()), new Vector3(2, 1.5f, 0));
		scene.addEntity(new ItemEntity(new KingsSword()), new Vector3(3, 1.5f, 0));
		scene.addEntity(new ItemEntity(new Dagger()), new Vector3(4, 1.5f, 0));

		scene.addEntity(new Hollow(), new Vector3(2, 0, -2));

		AudioManager.SetAmbientSound(Resource.GetSound("sound/ambient/dungeon_ambient_1.ogg"), 0.2f);
	}

	public override void destroy()
	{
	}

	MapPiece loadMap(int map)
	{
		if (SceneFormat.Read($"level/map{map}/map{map}.rfs", out List<SceneFormat.EntityData> entities, out _))
		{
			MapPiece mapPiece = new MapPiece();
			for (int i = 0; i < entities.Count; i++)
			{
				SceneFormat.EntityData entityData = entities[i];

				if (entityData.name.StartsWith("entity_"))
				{
					string entityName = entityData.name.Substring("entity_".Length);
					if (int.TryParse(entityName.Substring(entityName.LastIndexOf('_') + 1), out int _))
						entityName = entityName.Substring(0, entityName.LastIndexOf('_'));

					Entity entity = null;
					if (entityName == "crate")
						entity = new Crate();
					else if (entityName == "iron_door")
						entity = new IronDoor();
					else if (entityName == "torch")
						entity = new Torch();
					else if (entityName.StartsWith("ladder"))
						entity = new Ladder(int.Parse(entityName.Substring(6)));

					if (entity != null)
					{
						scene.addEntity(entity, entityData.position, entityData.rotation, entityData.scale);
						mapPiece.entities.Add(entity);
					}
				}
				else if (entityData.name.StartsWith("creature_"))
				{
					string entityName = entityData.name.Substring("creature_".Length);
					if (int.TryParse(entityName.Substring(entityName.LastIndexOf('_') + 1), out int _))
						entityName = entityName.Substring(0, entityName.LastIndexOf('_'));

					Entity entity = null;
					if (entityName == "hollow")
						entity = new Hollow();

					if (entity != null)
					{
						scene.addEntity(entity, entityData.position, entityData.rotation, entityData.scale);
						mapPiece.entities.Add(entity);
					}
				}
				else if (entityData.name.StartsWith("spawn_"))
				{
					mapPiece.spawnPoint = Matrix.CreateTransform(entityData.position, entityData.rotation);
				}
				else if (entityData.name.StartsWith("item_"))
				{
					string itemName = entityData.name.Substring("item_".Length);
					if (int.TryParse(itemName.Substring(itemName.LastIndexOf('_') + 1), out int _))
						itemName = itemName.Substring(0, itemName.LastIndexOf('_'));

					Item item = null;
					if (itemName == "kings_sword")
						item = new KingsSword();
					else if (itemName == "longsword")
						item = new Longsword();
					else if (itemName == "broken_sword")
						item = new BrokenSword();
					else if (itemName == "dagger")
						item = new Dagger();
					else if (itemName == "darkwood_staff")
						item = new DarkwoodStaff();
					else if (itemName == "crossbow")
						item = new LightCrossbow();
					else if (itemName == "sapphire_ring")
						item = new SapphireRing();

					if (item != null)
					{
						ItemEntity itemEntity = new ItemEntity(item);
						scene.addEntity(itemEntity, entityData.position, entityData.rotation);
						mapPiece.entities.Add(itemEntity);
					}
				}
				else if (entityData.name.StartsWith("envir_"))
				{
					string envirName = entityData.name.Substring("envir_".Length);
					EnvirTrigger trigger = new EnvirTrigger(Resource.GetCubemap($"level/map{map}/{envirName}_cubemap_equirect.png"));
					scene.addEntity(trigger, entityData.position, entityData.rotation, entityData.scale);
					trigger.load(entityData, 0, PhysicsFilter.Player);
					mapPiece.entities.Add(trigger);
				}
				else
				{
					Entity entity = new Entity();
					scene.addEntity(entity, entityData.position, entityData.rotation, entityData.scale);
					entity.load(entityData);
					mapPiece.entities.Add(entity);
				}
			}
			return mapPiece;
		}
		return null;
	}

	public void pushSkybox(Cubemap skybox)
	{
		skyboxes.Add(skybox);
	}

	public void popSkybox(Cubemap skybox)
	{
		skyboxes.Remove(skybox);
	}

	public override void draw(GraphicsDevice graphics)
	{
		if (sun != null)
			Renderer.DrawDirectionalLight(sun);

		Cubemap skybox = skyboxes[skyboxes.Count - 1];
		Renderer.DrawEnvironmentMap(skybox, 0.25f);
		Renderer.DrawSky(skybox, 1, Quaternion.Identity);
	}
}
