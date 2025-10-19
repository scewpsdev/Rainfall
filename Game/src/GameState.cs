using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class GameState : State
{
	public static GameState instance { get; private set; }


	public Scene scene;

	DirectionalLight sun;
	Cubemap skybox;

	public Camera camera;
	public Player player;


	public GameState()
	{
		instance = this;
	}

	public override void init()
	{
		scene = new Scene();

		sun = new DirectionalLight(new Vector3(-1, -1, 1).normalized, new Vector3(1.0f, 0.9f, 0.7f) * 3, Renderer.graphics);
		skybox = Resource.GetCubemap("texture/sky/overcast_soil_puresky_1k.hdr");

		scene.addEntity(player = new Player(), new Vector3(0, 0, 0));
		scene.addEntity(camera = new PlayerCamera(player));

		Entity map = new Entity();
		map.model = Resource.GetModel("map/testmap.gltf");
		map.body = new RigidBody(map, RigidBodyType.Static);
		map.body.addMeshColliders(Resource.GetModel("map/testmap_collider.gltf"), Matrix.Identity);
		scene.addEntity(map);

		scene.addEntity(new GrassField(), new Vector3(-10, 0, -10));
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

		if (sun != null)
			Renderer.DrawDirectionalLight(sun);

		if (skybox != null)
			Renderer.DrawEnvironmentMap(skybox, 0.25f);
	}
}
