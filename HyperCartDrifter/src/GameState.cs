using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

	public Model npcPath;

	public Camera camera;
	public DirectionalLight sun;
	public Cubemap skybox;

	Font font;

	public bool hasCap = true;
	public bool hasChain = true;
	public bool hasGlasses = true;
	public bool hasEngine = true;

	long startTime;


	public GameState()
	{
		instance = this;

		FontData fontData = Resource.GetFontData("HomeVideo-BLG6G.ttf");
		font = fontData.createFont(20, true);
	}

	public override void init()
	{
		//loadSupermarket();
		loadStreet();
		//loadRiver();
		//loadRacetrack();

		startTime = Time.currentTime;
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

		scene.addEntity(new Arrow(), new Vector3(-24, 7, 9), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI * 0.5f));

		scene.addEntity(new EventTrigger(new Vector3(2, 10, 22), Vector3.Zero, (RigidBody body) =>
		{
			if (body.entity is Cart)
			{
				if (hasCap && hasChain && hasGlasses)
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

	unsafe void loadStreet()
	{
		loadScene("street.rfs");

		npcPath = Resource.GetModel("street_path.gltf");

		spawnPoint = Matrix.CreateTranslation(50, 0, 0) * Matrix.CreateRotation(Vector3.Up, -MathF.PI * 0.5f);

		scene.addEntity(cart = new Cart(), spawnPoint);
		scene.addEntity(player = new Player(cart), spawnPoint);
		scene.addEntity(camera = new FollowCamera(player));

		int numCars = 30;
		for (int i = 0; i < numCars; i++)
		{
			MeshData* pathMesh = npcPath.getMeshData(0);
			int startPoint = MathHelper.RandomInt(0, pathMesh->vertexCount - 1);
			scene.addEntity(new Car(startPoint), pathMesh->getVertex(startPoint).position);
		}

		scene.addEntity(new EventTrigger(new Vector3(7, 6, 14), Vector3.Zero, (RigidBody body) =>
		{
			if (body.entity is Cart)
			{
				nextLevel = "racetrack";
				hasEngine = true;
			}
		}, PhysicsFilter.Cart), new Vector3(-153, -139, -401));

		skybox = Resource.GetCubemap("sky_cubemap_equirect.png");
		sun = new DirectionalLight(new Vector3(-1).normalized, Vector3.One, Renderer.graphics);

		AudioManager.SetAmbientSound(Resource.GetSound("sounds/street.ogg"));
	}

	unsafe void loadRiver()
	{
		loadScene("river.rfs");

		npcPath = Resource.GetModel("river_path.gltf");

		spawnPoint = Matrix.CreateTranslation(549.223f, -4, 150.594f) * Matrix.CreateRotation(Vector3.Up, MathF.PI * 0.5f);

		scene.addEntity(new WaterSurface(Resource.GetModel("river_water.gltf")), new Vector3(0, -3.2f, 0));

		scene.addEntity(cart = new Cart(), spawnPoint);
		scene.addEntity(player = new Player(cart), spawnPoint);
		scene.addEntity(camera = new FollowCamera(player));

		int numShips = 15;
		for (int i = 0; i < numShips; i++)
		{
			MeshData* pathMesh = npcPath.getMeshData(0);
			int startPoint = MathHelper.RandomInt(0, pathMesh->vertexCount - 1);
			scene.addEntity(new Ship(startPoint), pathMesh->getVertex(startPoint).position);
		}

		//cart.waterLevel = -4;

		scene.addEntity(new EngineItem(), new Vector3(2024.28f, -11.0571f, 20.153f) * 0.25f);

		scene.addEntity(new EventTrigger(new Vector3(143.729f, 39.2209f, 67.3469f) * 0.5f, Vector3.Zero, (RigidBody body) =>
		{
			if (body.entity is Cart)
			{
				if (hasEngine)
				{
					nextLevel = "racetrack";
				}
			}
		}, PhysicsFilter.Cart), new Vector3(2007.04f, -5.42714f, 39.3736f) * 0.25f);

		skybox = Resource.GetCubemap("sky_cubemap_equirect.png");
		sun = new DirectionalLight(new Vector3(-1).normalized, Vector3.One, Renderer.graphics);
	}

	unsafe Vector3 getPoint(int idx)
	{
		MeshData* mesh = npcPath.getMeshData(0);
		Debug.Assert(idx < mesh->vertexCount);
		PositionNormalTangent vertex = mesh->getVertex(idx);
		Vector3 origin = vertex.position;
		origin.y = 1.5f;
		return origin;
		HitData? hit = Physics.Raycast(origin, Vector3.Down, 50, QueryFilterFlags.Static);
		Debug.Assert(hit != null);
		Vector3 point = origin + hit.Value.distance * Vector3.Down;
		return point;
	}

	unsafe void loadRacetrack()
	{
		loadScene("racetrack.rfs");

		npcPath = Resource.GetModel("racetrack_path.gltf");

		int numCars = 25;
		for (int i = 0; i < numCars; i++)
		{
			int startPoint = (npcPath.getMeshData(0)->vertexCount - 17 + i + 1) % npcPath.getMeshData(0)->vertexCount;
			scene.addEntity(new Racecar(startPoint), getPoint(startPoint));
		}

		spawnPoint = Matrix.CreateTranslation(getPoint(npcPath.getMeshData(0)->vertexCount - 17)) * Matrix.CreateRotation(Vector3.Up, MathHelper.ToRadians(-116));  // Matrix.CreateTranslation(-116.315f * 2, 2.08148f, 8.08349f * 2) * Matrix.CreateRotation(Vector3.Up, MathHelper.ToRadians(-116));

		scene.addEntity(cart = new Cart(), spawnPoint);
		scene.addEntity(player = new Player(cart), spawnPoint);
		scene.addEntity(camera = new FollowCamera(player));

		skybox = Resource.GetCubemap("sky_cubemap_equirect.png");
		sun = new DirectionalLight(new Vector3(-1).normalized, Vector3.One, Renderer.graphics);

		AudioManager.SetAmbientSound(Resource.GetSound("sounds/street.ogg"));
	}

	public override void destroy()
	{
		if (scene != null)
		{
			scene.destroy();
			scene = null;

			if (skybox != null)
			{
				Resource.FreeCubemap(skybox);
				skybox = null;
			}

			sun = null;

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
			else if (nextLevel == "racetrack")
				loadRacetrack();

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

		float fadein = 5;
		if ((Time.currentTime - startTime) / 1e9f < fadein)
		{
			string text = "WASD to move";
			int w = font.measureText(text);
			GUI.Text(Display.width / 2 - w / 2, 300, 1, text, font, 0xFFFFFFFF);

			text = "Shift to boost";
			w = font.measureText(text);
			GUI.Text(Display.width / 2 - w / 2, 350, 1, text, font, 0xFFFFFFFF);
		}
	}
}
