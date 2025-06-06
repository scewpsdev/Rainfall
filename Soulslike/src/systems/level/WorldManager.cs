using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WorldManager : Entity
{
	DirectionalLight sun;
	List<Cubemap> skyboxes = new List<Cubemap>();

	public Vector3 fogColor = Vector3.One;
	public float fogStrength = 0.0f;

	public Matrix spawnPoint = Matrix.Identity;

	public Cubemap skybox;


	public override void init()
	{
		//sun = new DirectionalLight(new Vector3(-1, -1, 1).normalized, new Vector3(1.0f, 0.9f, 0.7f) * 3, Renderer.graphics);
		//Cubemap globalSkybox = Resource.GetCubemap("level/cubemap_equirect.png");
		//skyboxes.Add(globalSkybox);
	}

	public override void destroy()
	{
	}

	public void pushSkybox(Cubemap skybox)
	{
		skyboxes.Add(skybox);
	}

	public void popSkybox(Cubemap skybox)
	{
		skyboxes.Remove(skybox);
	}

	public override void draw(GraphicsDevice graphics)
	{
		if (sun != null)
			Renderer.DrawDirectionalLight(sun);

		if (skyboxes.Count > 0)
		{
			Cubemap skybox = skyboxes[skyboxes.Count - 1];
			if (skybox != null)
			{
				Renderer.DrawEnvironmentMap(skybox, 0.25f);
				Renderer.DrawSky(skybox, 1, Quaternion.Identity);
			}
		}

		GraphicsManager.fogColor = fogColor;
		GraphicsManager.fogStrength = fogStrength;
	}
}
