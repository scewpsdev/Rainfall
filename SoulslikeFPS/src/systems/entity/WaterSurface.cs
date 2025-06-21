using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WaterSurface : Entity
{
	static Material waterMaterial;

	static WaterSurface()
	{
		Shader shader = Resource.GetShader("shaders/water/water.vsh", "shaders/water/water.fsh");
		waterMaterial = new Material(
			shader,
			true
		);
	}

	public override void draw(GraphicsDevice graphics)
	{
		float amplitude = 0.05f;
		float frequency = 1.0f;
		waterMaterial.setData(0, new Vector4(amplitude, frequency, 0, 0));
		Renderer.DrawMesh(model, meshIdx, waterMaterial, getModelMatrix());
	}
}
