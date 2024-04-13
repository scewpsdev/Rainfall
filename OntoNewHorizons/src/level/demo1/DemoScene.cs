using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class DemoScene : Room
{
	DirectionalLight sun;
	Cubemap skybox;

	ReflectionProbe towerReflection;
	ReflectionProbe dungeonReflection;
	ReflectionProbe arenaReflection;

	Model testModel;


	public DemoScene(RoomType type, Level level)
		: base(type, level)
	{
		skybox = Resource.GetCubemap("res/texture/cubemap/hub_cubemap.png");
		sun = new DirectionalLight(new Vector3(0.5f, -1.0f, -1.0f).normalized, new Vector3(1.0f, 0.9f, 0.7f) * 10.0f, Renderer.graphics);
		testModel = Resource.GetModel("res/level/ModularDungeon.gltf");
	}

	public override void spawn(TileMap tilemap)
	{
		base.spawn(tilemap);


		addEntity(new Dummy(), new Vector3(0.0f, 0.0f, 8.0f), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI));
		//addEntity(new SkeletonEnemy(), new Vector3(3.0f, 1.0f, -7.0f), Quaternion.Identity);
		//addEntity(new SkeletonEnemy(), new Vector3(0.0f, 1.0f, -7.0f), Quaternion.Identity);
		//addEntity(new SkeletonEnemy(), new Vector3(-3.0f, 1.0f, -7.0f), Quaternion.Identity);

		addEntity(new WallTorch(), new Vector3(-3.5f, -35, 56), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI));
		addEntity(new WallTorch(), new Vector3(3.5f, -35, 56), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI));
		//addEntity(new WallTorch(), new Vector3(6.0f, 2.1f, -10.0f), Quaternion.Identity);
		//addEntity(new WallTorch(), new Vector3(-5.0f, 0.0f, -20.0f), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f));
		//addEntity(new WallTorch(), new Vector3(5.0f, 0.0f, -20.0f), Quaternion.FromAxisAngle(Vector3.Up, -MathF.PI * 0.5f));
		//addEntity(new WallTorch(), new Vector3(-2.0f, -2.0f, -28.0f), Quaternion.Identity);
		//addEntity(new WallTorch(), new Vector3(2.0f, -2.0f, -28.0f), Quaternion.Identity);
		addEntity(new Chest(new Item[] { Item.Get("quemick") }, new int[] { 1 }), new Vector3(-13, 16, -69), Quaternion.Identity);
		//addEntity(new Door(DoorType.Normal), new Vector3(-2.0f, 0.0f, -3.0f), Quaternion.Identity);

		//addEntity(new ItemPickup(Item.Get("shortsword")), new Vector3(0.5f, 1.0f, 0.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		//addEntity(new ItemPickup(Item.Get("shortsword"), 1, null, null, false), new Vector3(-2.0411f, 16.7006f, -49.312f), new Quaternion(0.753861f, -0.584183f, 0.237691f, 0.184192f));
		addEntity(new Chest(new Item[] { Item.Get("shortsword"), Item.Get("quemick") }, new int[] { 1, 2 }), new Vector3(-2, 16, -50.0f), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI));
		//addEntity(new ItemPickup(Item.Get("zweihander")), new Vector3(-0.5f, 1.0f, 0.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		addEntity(new ItemPickup(Item.Get("longsword"), 1, null, null, false), new Vector3(-10.594f, 43.6672f, -50.1465f), new Quaternion(0.119981f, 0.03783f, 0.94614f, 0.298317f));
		//addEntity(new ItemPickup(Item.Get("longbow")), new Vector3(0.0f, 1.0f, -3.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		//addEntity(new ItemPickup(Item.Get("arrow"), 5), new Vector3(-0.2f, 1.0f, -3.0f), Quaternion.Identity);
		//addEntity(new ItemPickup(Item.Get("oak_staff"), 1, null, new Item[] { Item.Get("magic_arrow") }), new Vector3(-0.5f, 1.0f, -2.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		//addEntity(new ItemPickup(Item.Get("oak_staff"), 1, null, new Item[] { Item.Get("homing_orbs") }), new Vector3(0.0f, 1.0f, -2.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		//addEntity(new ItemPickup(Item.Get("oak_staff"), 1, null, new Item[] { Item.Get("magic_orb") }), new Vector3(0.5f, 1.0f, -2.0f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		addEntity(new ItemPickup(Item.Get("wooden_round_shield")), new Vector3(-7, 16, -47), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));
		//addEntity(new ItemPickup(Item.Get("dagger")), new Vector3(0.0f, 1.0f, -0.5f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2));

		//addEntity(new Door(DoorType.Windowed), new Vector3(0.0f, 0.0f, -10.0f), Quaternion.Identity);
		//addEntity(new SkeletonEnemy(), new Vector3(0.0f, 0.0f, -10.4f), Quaternion.Identity);

		addEntity(new Ladder(new Vector3(0.5f, 2.4f, 0.2f), new Vector3(0.0f, 0.0f, -0.2f)), new Vector3(9.0f, 23.5f, -55.5f), Quaternion.LookAt(Vector3.Left));
		addEntity(new Ladder(new Vector3(0.5f, 26.0f, 0.2f), new Vector3(0.0f, 0.0f, -0.2f)), new Vector3(7.0f, -12.0f, -58.0f), Quaternion.LookAt(Vector3.Left));
		addEntity(new Ladder(new Vector3(0.5f, 2.4f, 0.2f), new Vector3(0.0f, 0.0f, -0.2f)), new Vector3(-10, 40, -53), Quaternion.LookAt(Vector3.Right));
		addEntity(new Ladder(new Vector3(0.5f, 5.4f, 0.2f), new Vector3(0.0f, 0.0f, -0.2f)), new Vector3(-10, 24, -50), Quaternion.LookAt(Vector3.Left));

		addEntity(new ReverbZone(new Vector3(9.0f, 9.5f, 9.0f)), new Vector3(0, 25.5f, -58), Quaternion.Identity);
		addEntity(new ReverbZone(new Vector3(9.5f, 18.0f, 9.5f)), new Vector3(0, -20.7f, 47), Quaternion.Identity);

		//doorways.Add(new Doorway(room, new Vector3(0.0f, -7.5f, -35.0f), Quaternion.Identity));

		Elevator elevator;
		addEntity(elevator = new Elevator(new Vector3(0, -37, 47), new Vector3(0, 0, 47)), new Vector3(0, -37, 47), Quaternion.Identity);
		addEntity(new PressurePlate(elevator), new Vector3(0, -37, 41), Quaternion.Identity);
		addEntity(new PressurePlate(elevator), new Vector3(0, 0, 53), Quaternion.Identity);


		towerReflection = new ReflectionProbe(64, new Vector3(0, 29, -58), new Vector3(18.001f, 26.001f, 18.001f), new Vector3(0, 25, -58), Renderer.graphics);
		dungeonReflection = new ReflectionProbe(64, new Vector3(0, -19f, 47), new Vector3(19.0f, 37.0f, 19.0f), new Vector3(0, -20.7f, 47), Renderer.graphics);
		arenaReflection = new ReflectionProbe(64, new Vector3(-100, -2, 0), 2 * new Vector3(34.5f, 8, 34.5f), new Vector3(-100, -2, 0), Renderer.graphics);


		addEntity(new Checkpoint(new Vector3(20, 1, 20)), new Vector3(0, 1, 0), Quaternion.Identity);
		addEntity(new Checkpoint(new Vector3(2, 2, 2)), new Vector3(0, 18, -67), Quaternion.Identity);
		addEntity(new Checkpoint(new Vector3(36, 1, 36)), new Vector3(-100, -9, 0), Quaternion.Identity);
		addEntity(new Checkpoint(new Vector3(10, 1, 10)), new Vector3(0, 1, 47), Quaternion.Identity);

		addEntity(new ArenaZone(new Vector3(20, 1, 20)), new Vector3(-100, -9, 0), Quaternion.Identity);


		Renderer.fogIntensity = 0.005f;


		AudioManager.SetAmbientSound(Resource.GetSound("res/level/hub/ambience.ogg"));
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		Renderer.DrawSky(skybox, 1.5f, Matrix.Identity);
		Renderer.SetEnvironmentMap(skybox, 1.5f);
		Renderer.DrawDirectionalLight(sun);

		Renderer.DrawReflectionProbe(towerReflection);
		Renderer.DrawReflectionProbe(dungeonReflection);
		Renderer.DrawReflectionProbe(arenaReflection);

		Renderer.DrawLight(new Vector3(2.013f, -35.5f, 54.5f), new Vector3(4.0f));
		Renderer.DrawLight(new Vector3(-2.013f, -35.5f, 54.5f), new Vector3(4.0f));

		Renderer.DrawModel(testModel, Matrix.Identity);
	}
}
