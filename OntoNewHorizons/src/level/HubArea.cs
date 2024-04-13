using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class HubArea : Entity
{
	Terrain terrain = null;

	List<GrassPatch> grassPatches = new List<GrassPatch>();

	DirectionalLight sun;
	Cubemap skybox;

	AudioSource ambientSource;

	public Camera camera;
	public Player player;


	public HubArea(GraphicsDevice graphics)
	{
		//terrain = new Terrain("res/level/hub/terrain.gltf", "res/level/hub/terrain_collider.gltf", "res/level/hub/splatmap.png", "res/level/hub/grass_coverage.png", graphics);

		for (int x = -2; x < 2; x++)
		{
			for (int z = -2; z < 2; z++)
			{
				grassPatches.Add(new GrassPatch(terrain, new Vector2(x * Renderer.GRASS_PATCH_SIZE, z * Renderer.GRASS_PATCH_SIZE)));
			}
		}

		skybox = Resource.GetCubemap("res/texture/cubemap/hub_cubemap.png");
		sun = new DirectionalLight(new Vector3(0.0f, -1.0f, -1.0f).normalized, new Vector3(1.0f, 0.9f, 0.7f) * 50.0f, graphics);

		ambientSource = Audio.CreateSource(Vector3.Zero);
		ambientSource.isAmbient = true;
		ambientSource.isLooping = true;

		Sound ambientSound = Resource.GetSound("res/level/hub/ambience.ogg");
		ambientSource.playSound(ambientSound);

		//Audio.SetEffect(AudioEffect.Reverb);
	}

	public override void init()
	{
		//terrain.init(this);

		OntoNewHorizons.instance.world.addEntity(camera = new Camera());
		OntoNewHorizons.instance.world.addEntity(player = new Player(camera), new Vector3(-25, terrain.getHeight(-25, 11), 11), Quaternion.Identity);

		Random random = new Random(123);
		for (int i = 0; i < 16; i++)
		{
			Vector3 position = new Vector3(MathHelper.RandomFloat(-10.0f, 10.0f, random), 0.0f, MathHelper.RandomFloat(-10.0f, 10.0f, random));
			position.y = terrain.getHeight(position.x, position.z);
			OntoNewHorizons.instance.world.addEntity(new Tree(), position, Quaternion.FromAxisAngle(Vector3.Up, MathHelper.RandomFloat(0.0f, MathF.PI * 2.0f, random)));
		}

		//level.addEntity(new SkeletonEnemy(), new Vector3(0.0f, 3.0f, 0.0f), Quaternion.Identity);

		OntoNewHorizons.instance.world.addEntity(new Chest(new Item[] { Item.Get("quemick"), Item.Get("firebomb") }, new int[] { 2, 5 }), new Vector3(-2.5f, terrain.getHeight(-2.0f, 0.0f), 0.0f), Quaternion.Identity);
		//room.addEntity(new Door(DoorType.Normal), new Vector3(-2.0f, 0.0f, -3.0f), Quaternion.Identity);

		/*
		level.addEntity(new ItemPickup(Item.Get("shortsword")), new Vector3(0.5f, 3.0f, 0.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		level.addEntity(new ItemPickup(Item.Get("zweihander")), new Vector3(-0.5f, 3.0f, 0.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		level.addEntity(new ItemPickup(Item.Get("longsword")), new Vector3(0.0f, 3.0f, -1.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		level.addEntity(new ItemPickup(Item.Get("longbow")), new Vector3(0.0f, 3.0f, -3.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		level.addEntity(new ItemPickup(Item.Get("arrow"), 5), new Vector3(-0.2f, 3.0f, -3.0f), Quaternion.Identity);
		level.addEntity(new ItemPickup(Item.Get("oak_staff"), 1, null, new Item[] { Item.Get("magic_arrow") }), new Vector3(-0.5f, 3.0f, -2.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		level.addEntity(new ItemPickup(Item.Get("oak_staff"), 1, null, new Item[] { Item.Get("homing_orbs") }), new Vector3(0.0f, 3.0f, -2.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		level.addEntity(new ItemPickup(Item.Get("oak_staff"), 1, null, new Item[] { Item.Get("magic_orb") }), new Vector3(0.5f, 3.0f, -2.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		level.addEntity(new ItemPickup(Item.Get("wooden_round_shield")), new Vector3(0.0f, 3.0f, 0.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		*/
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.SetCamera(camera);

		Renderer.DrawSky(skybox, 1.0f, Matrix.Identity);
		Renderer.SetEnvironmentMap(skybox, 1.0f);
		Renderer.DrawDirectionalLight(sun);

		terrain.draw(graphics);

		for (int i = 0; i < grassPatches.Count; i++)
		{
			grassPatches[i].draw(graphics);
		}

		/*
		Renderer.DrawLight(Quaternion.FromAxisAngle(Vector3.Up, Time.currentTime / 1e9f) * new Vector3(0.0f, terrain.getHeight(0.0f, 0.0f) + 1.0f, 2.0f), new Vector3(2.0f, 0.0f, 0.0f));
		Renderer.DrawLight(Quaternion.FromAxisAngle(Vector3.Up, Time.currentTime / 1e9f) * new Vector3(0.0f, terrain.getHeight(2.0f, 0.0f) + 1.0f, 2.0f) + new Vector3(2.0f, 0.0f, 0.0f), new Vector3(0.0f, 2.0f, 0.0f));
		Renderer.DrawLight(Quaternion.FromAxisAngle(Vector3.Up, Time.currentTime / 1e9f) * new Vector3(0.0f, terrain.getHeight(-2.0f, 0.0f) + 1.0f, 2.0f) + new Vector3(-2.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 10.0f));
		*/
	}
}
