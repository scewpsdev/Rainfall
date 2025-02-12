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

	FirstPersonCamera camera;
	DirectionalLight sun;


	public GameState()
	{
		instance = this;
	}

	public override void init()
	{
		scene = new Scene();

		scene.load("testmap.rfs");

		scene.addEntity(camera = new FirstPersonCamera());
		scene.addEntity(new Player(camera));

		sun = new DirectionalLight(new Vector3(-1).normalized, Vector3.One, Renderer.graphics);
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
	}
}
