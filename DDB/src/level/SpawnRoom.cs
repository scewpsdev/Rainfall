using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class SpawnRoom : Entity
{
	Model levelGeometry;
	RigidBody collider;

	Cubemap skybox;

	DirectionalLight sun;


	public SpawnRoom(GraphicsDevice graphics)
	{
		levelGeometry = Resource.GetModel("res/entity/level/spawn/spawn.gltf");
		skybox = Resource.GetCubemap("res/texture/cubemap/hub_cubemap.png");

		sun = new DirectionalLight(new Vector3(-1.0f, -1.0f, -1.0f).normalized, new Vector3(1.0f), graphics);
	}

	public override void init()
	{
		collider = new RigidBody(this, RigidBodyType.Static);
		collider.addMeshColliders(levelGeometry, Matrix.Identity);

		level.addEntity(new Dummy(), new Vector3(0.0f, 0.0f, -4.0f), Quaternion.FromAxisAngle(Vector3.Up, MathF.PI));
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(levelGeometry, Matrix.Identity);
		//Renderer.DrawDirectionalLight(sun);
		for (int y = 0; y < 4; y++)
		{
			for (int x = 0; x < 4; x++)
			{
				Renderer.DrawLight(new Vector3(x * 8.0f - 12.0f, 2.0f, y * 8.0f - 12.0f), new Vector3(1.0f));
			}
		}
		//Renderer.DrawSky(skybox, Matrix.Identity);
		//Renderer.SetEnvironmentMap(skybox);
	}
}
