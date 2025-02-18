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

	public Matrix spawnPoint;

	Cart cart;
	Camera camera;
	DirectionalLight sun;
	Cubemap skybox;


	public GameState()
	{
		instance = this;
	}

	public override void init()
	{
		loadSupermarket();
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
		scene.addEntity(camera = new FollowCamera(cart));

		scene.addEntity(new CapItem(), new Vector3(50, 2.5f, 0));
		scene.addEntity(new GlassesItem(), new Vector3(102, 4, -40.3f));
		scene.addEntity(new ChainItem(), new Vector3(80, 5, -90));

		skybox = Resource.GetCubemap("supermarket_cubemap_equirect.png");
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
		Animator.Update(camera.getModelMatrix());
		ParticleSystem.Update(camera.position);

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
