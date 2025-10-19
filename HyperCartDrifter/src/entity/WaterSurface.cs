using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WaterSurface : Entity
{
	static Material material;

	static WaterSurface()
	{
		material = new Material(Resource.GetShader("shaders/water/water.vsh", "shaders/water/water.fsh"), true);
	}


	public WaterSurface(Model model)
	{
		this.model = model;
	}

	public override void destroy()
	{
		Resource.FreeModel(model);
	}

	public override void draw(GraphicsDevice graphics)
	{
		material.setData(0, new Vector4(GameState.instance.camera.position, 0));
		material.setData(1, new Vector4(GameState.instance.camera.near, GameState.instance.camera.far, 0, Time.currentTime / 1e9f));
		material.setData(2, new Vector4(GameState.instance.sun.direction, 0));
		material.setData(3, new Vector4(GameState.instance.sun.color, 0));
		material.setTexture(2, GameState.instance.skybox);
		Renderer.DrawModel(model, getModelMatrix(), material);
	}
}
