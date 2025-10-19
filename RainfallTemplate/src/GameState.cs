using Rainfall;


public class GameState : State
{
	Model cube;
	Cubemap skybox;
	DirectionalLight sun;


	public override void init()
	{
		cube = Resource.GetModel("rainfall/cube.gltf");
		skybox = Resource.GetCubemap("rainfall/sky_cubemap_equirect.png");
		sun = new DirectionalLight(new Vector3(-1, -1, 1).normalized, new Vector3(1.0f, 0.9f, 0.7f) * 10, Renderer.graphics);
	}

	public override void destroy()
	{
	}

	public override void update()
	{
	}

	public override void draw(GraphicsDevice graphics)
	{
		Vector3 cameraPosition = Quaternion.FromAxisAngle(Vector3.Up, Time.gameTime * 0.2f) * new Vector3(10, 4, 10);
		Renderer.SetCamera(cameraPosition, Quaternion.LookAt(cameraPosition, Vector3.Zero), 60, Display.aspectRatio, 0.3f, 1000);

		Renderer.DrawSky(skybox, 2, Quaternion.Identity);
		Renderer.DrawEnvironmentMap(skybox, 1);
		Renderer.DrawDirectionalLight(sun);

		Renderer.DrawModel(cube, Matrix.Identity);
		Renderer.DrawModel(cube, Matrix.CreateTransform(new Vector3(0, -2, 0), Quaternion.Identity, new Vector3(10, 1, 10)));
	}
}