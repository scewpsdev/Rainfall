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

	string nextLevel = null;

	public Matrix spawnPoint;

	public Cart cart;
	public Player player;

	Camera camera;
	DirectionalLight sun;
	Cubemap skybox;


	public GameState()
	{
		instance = this;
	}

	public override void init()
	{
		//loadSupermarket();
		//loadStreet();
		//loadRiver();
		loadRacetrack();
	}

	void loadScene(string path)
	{
		destroy();

		scene = new Scene();
		scene.load(path, () => { Entity entity = new Entity(); entity.bodyFriction = 0; entity.bodyRestitution = 1.0f; entity.isStatic = true; return entity; });

		//sun = new DirectionalLight(new Vector3(-1).normalized, Vector3.One, Renderer.graphics);
	}

	void loadSupermarket()
	{
		loadScene("supermarket.rfs");

		spawnPoint = Matrix.CreateTranslation(-14, 0, 18);

		scene.addEntity(cart = new Cart(), spawnPoint);
		scene.addEntity(player = new Player(cart), spawnPoint);
		scene.addEntity(camera = new FollowCamera(player));

		scene.addEntity(new CapItem(), new Vector3(50, 3f, -7));
		scene.addEntity(new GlassesItem(), new Vector3(102, 4, -40.3f));
		scene.addEntity(new ChainItem(), new Vector3(80, 5, -90));

		scene.addEntity(new EventTrigger(new Vector3(2, 10, 22), Vector3.Zero, (RigidBody body) =>
		{
			if (body.entity is Cart)
			{
				if (player.hasCap && player.hasChain && player.hasGlasses)
				{
					nextLevel = "street";
				}
				else
				{
					cart.respawn();
				}
			}
		}, PhysicsFilter.Cart), new Vector3(-27, 12, 9));

		skybox = Resource.GetCubemap("supermarket_cubemap_equirect.png");
	}

	void loadStreet()
	{
		loadScene("street.rfs");

		spawnPoint = Matrix.CreateTranslation(50, 0, 0) * Matrix.CreateRotation(Vector3.Up, -MathF.PI * 0.5f);

		scene.addEntity(cart = new Cart(), spawnPoint);
		scene.addEntity(player = new Player(cart), spawnPoint);
		scene.addEntity(camera = new FollowCamera(player));

		scene.addEntity(new EventTrigger(new Vector3(206, 20, 137), Vector3.Zero, (RigidBody body) =>
		{
			if (body.entity is Cart)
			{
				nextLevel = "river";
			}
		}, PhysicsFilter.Cart), new Vector3(-57, -147, -401));

		skybox = Resource.GetCubemap("sky_cubemap_equirect.png");
		sun = new DirectionalLight(new Vector3(-1).normalized, Vector3.One, Renderer.graphics);
	}

	void loadRiver()
	{
		loadScene("river.rfs");

		spawnPoint = Matrix.CreateTranslation(0, -4, 0) * Matrix.CreateRotation(Vector3.Up, -MathF.PI * 0.5f);

		scene.addEntity(cart = new Cart(), spawnPoint);
		scene.addEntity(player = new Player(cart), spawnPoint);
		scene.addEntity(camera = new FollowCamera(player));

		//cart.waterLevel = -4;

		scene.addEntity(new CapItem(), new Vector3(2024.28f, -11.0571f, 20.153f) * 0.25f);

		scene.addEntity(new EventTrigger(new Vector3(143.729f, 39.2209f, 67.3469f) * 0.5f, Vector3.Zero, (RigidBody body) =>
		{
			if (body.entity is Cart)
			{
				nextLevel = "racetrack";
			}
		}, PhysicsFilter.Cart), new Vector3(2007.04f, -5.42714f, 39.3736f) * 0.25f);

		skybox = Resource.GetCubemap("sky_cubemap_equirect.png");
		sun = new DirectionalLight(new Vector3(-1).normalized, Vector3.One, Renderer.graphics);
	}

	void loadRacetrack()
	{
		loadScene("racetrack.rfs");

		spawnPoint = Matrix.CreateTranslation(-116.315f * 2, 2.08148f, 8.08349f * 2) * Matrix.CreateRotation(Vector3.Up, MathHelper.ToRadians(-116));

		scene.addEntity(cart = new Cart(), spawnPoint);
		scene.addEntity(player = new Player(cart), spawnPoint);
		scene.addEntity(camera = new FollowCamera(player));

		skybox = Resource.GetCubemap("sky_cubemap_equirect.png");
		sun = new DirectionalLight(new Vector3(-1).normalized, Vector3.One, Renderer.graphics);
	}

	public override void destroy()
	{
		if (scene != null)
		{
			scene.destroy();
			scene = null;

			sun = null;
			skybox = null;

			spawnPoint = Matrix.Identity;
		}
	}

	public override void update()
	{
		if (nextLevel != null)
		{
			if (nextLevel == "street")
				loadStreet();
			else if (nextLevel == "river")
				loadRiver();

			nextLevel = null;
		}

		if (cart.body.getVelocity().y < -1000)
			cart.respawn();

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

		if (sun != null)
			Renderer.DrawDirectionalLight(sun);
		if (skybox != null)
		{
			Renderer.DrawSky(skybox, 1, Quaternion.Identity);
			Renderer.DrawEnvironmentMap(skybox, 0.5f);
		}
	}
}
