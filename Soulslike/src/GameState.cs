using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GameState : State
{
	public static GameState instance { get; private set; }


	public Scene scene;
	public WorldManager world;

	public Camera camera;
	public Player player;


	public GameState()
	{
		instance = this;
	}

	public override void init()
	{
		scene = new Scene();

		scene.addEntity(world = new WorldManager());

		//DungeonGenerator.Generate(world, scene);
		MapLoader.Run(world, scene);

		/*
		Entity testmap = new Entity();
		testmap.model = Resource.GetModel("testmap.gltf");
		testmap.body = new RigidBody(testmap, RigidBodyType.Static);
		testmap.body.addMeshColliders(testmap.model, Matrix.Identity);
		scene.addEntity(testmap);

		scene.addEntity(new Fireplace(), new Vector3(0, 0, 0));

		scene.addEntity(new ItemEntity(new Longsword()), new Vector3(2, 1.5f, 0));
		scene.addEntity(new ItemEntity(new KingsSword()), new Vector3(3, 1.5f, 0));
		scene.addEntity(new ItemEntity(new BrokenSword()), new Vector3(4, 1.5f, 0));

		scene.addEntity(new Hollow(), new Vector3(2, 0, -2));
		*/

		scene.addEntity(camera = new Camera());
		scene.addEntity(player = new Player(camera), world.spawnPoint);
	}

	public override void destroy()
	{
		scene.destroy();
	}

	public override void update()
	{
		Animator.Update(camera.getModelMatrix());
		ParticleSystem.Update(camera.position, camera.rotation);

		scene.update();
	}

	public override void fixedUpdate(float delta)
	{
		scene.fixedUpdate(delta);
	}

	public override void draw(GraphicsDevice graphics)
	{
		scene.draw(graphics);
	}
}
