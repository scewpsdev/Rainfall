using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Testmap2 : Room
{
	DirectionalLight sun;
	Cubemap skybox;

	Cubemap dungeonSkybox;

	Model waterModel;

	ReflectionProbe topReflection;
	ReflectionProbe dungeonReflection;


	public Testmap2(RoomType type, Level level)
		: base(type, level)
	{
		//skybox = Resource.GetCubemap("res/texture/cubemap/hub_cubemap.png");
		skybox = Resource.GetCubemap("res/texture/cubemap/sunset_cubemap.hdr");
		sun = new DirectionalLight(new Vector3(-1.0f, -0.05f, -1.0f).normalized, new Vector3(1.0f, 0.9f, 0.7f) * 10.0f, Renderer.graphics);

		dungeonSkybox = Resource.GetCubemap("res/texture/cubemap/dungeon_cubemap.png");

		waterModel = Resource.GetModel("res/level/room/testmap2/testmap2_water.gltf");
	}

	public override void spawn(TileMap tilemap)
	{
		base.spawn(tilemap);


		addEntity(new Dummy(), new Vector3(0.0f, 0.0f, 8.0f), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI));

		addEntity(new WallTorch(), new Vector3(-3.6f, -35, 55.5f), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI));
		addEntity(new WallTorch(), new Vector3(3.6f, -35, 55.5f), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI));

		addEntity(new ReverbZone(new Vector3(9.5f, 18.0f, 9.5f)), new Vector3(0, -20.7f, 47), Quaternion.Identity);


		Elevator elevator;
		addEntity(elevator = new Elevator(new Vector3(0, -37, 47), new Vector3(0, 0, 47)), new Vector3(0, -37, 47), Quaternion.Identity);
		addEntity(new PressurePlate(elevator), new Vector3(0, -37, 41), Quaternion.Identity);
		addEntity(new PressurePlate(elevator), new Vector3(0, 0, 53), Quaternion.Identity);


		topReflection = new ReflectionProbe(64, new Vector3(0, 3, 48), new Vector3(18, 6, 18), new Vector3(0, 3, 48), Renderer.graphics);
		dungeonReflection = new ReflectionProbe(64, new Vector3(0, -19f, 47), new Vector3(19.0f, 37.0f, 19.0f), new Vector3(0, -20.7f, 47), Renderer.graphics);

		EnvironmentTransition transition = new EnvironmentTransition(new Vector3(3, 3, 6.5f), Vector3.Back);
		transition.to.skybox = dungeonSkybox;
		transition.to.skyboxIntensity = 1.0f;
		transition.to.sun = null;
		transition.to.fogColor = Vector3.Zero;
		transition.to.fogIntensity = 0.2f;
		transition.from.skybox = skybox;
		transition.from.skyboxIntensity = 0.3f;
		transition.from.sun = sun;
		addEntity(transition, new Vector3(0, -34, 63.5f), Quaternion.Identity);


		addEntity(new Checkpoint(new Vector3(20, 1, 20)), new Vector3(0, 1, 0), Quaternion.Identity);
		addEntity(new Checkpoint(new Vector3(10, 1, 10)), new Vector3(0, 1, 47), Quaternion.Identity);


		//doorways.Add(new Doorway(room, new Vector3(0.0f, -7.5f, -35.0f), Quaternion.Identity));


		//Renderer.fogStrength = 0.01f;
		//Renderer.fogColor = new Vector3(0.05f);

		GraphicsManager.skybox = skybox;
		GraphicsManager.skyboxIntensity = 0.3f;
		GraphicsManager.environmentMap = skybox;
		GraphicsManager.environmentMapIntensity = 0.3f;


		AudioManager.SetAmbientSound(Resource.GetSound("res/level/room/testmap2/ambience.ogg"), 0.3f);
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		//Renderer.DrawWater(new Vector3(-240.0f, -6.0f, 0.0f), 480.0f);
		Renderer.DrawWater(new Vector3(0.0f), waterModel);

		//Renderer.DrawSky(skybox, 0.3f, Matrix.Identity);
		//Renderer.SetEnvironmentMap(skybox, 0.3f);
		//Renderer.DrawDirectionalLight(sun);

		Renderer.DrawReflectionProbe(topReflection);
		Renderer.DrawReflectionProbe(dungeonReflection);

		Renderer.DrawLight(new Vector3(2.013f, -35.5f, 54.5f), new Vector3(4.0f));
		Renderer.DrawLight(new Vector3(-2.013f, -35.5f, 54.5f), new Vector3(4.0f));
	}
}
