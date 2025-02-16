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
		scene = new Scene();

		scene.load("testmap.rfs");

		scene.addEntity(cart = new Cart(), new Vector3(0, 1, 0));
		scene.addEntity(camera = new FreeCamera(), new Vector3(0, 3, 4));

		sun = new DirectionalLight(new Vector3(-1).normalized, Vector3.One, Renderer.graphics);
		skybox = Resource.GetCubemap("sky_cubemap_equirect.png");
	}

	public override void destroy()
	{
		scene.destroy();
	}

	public override void update()
	{
		Animator.Update(camera.getModelMatrix());
		ParticleSystem.Update(camera.position);

		scene.update();
	}

	public override void draw(GraphicsDevice graphics)
	{
		scene.draw(graphics);

		Renderer.DrawDirectionalLight(sun);
		Renderer.DrawSky(skybox, 1, Quaternion.Identity);
		Renderer.DrawEnvironmentMap(skybox, 0.25f);
	}
}
