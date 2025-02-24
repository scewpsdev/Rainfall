using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Arrow : Entity
{
	public Arrow()
	{
		model = Resource.GetModel("arrow.gltf");
	}

	public override void destroy()
	{
		Resource.FreeModel(model);
	}

	public override void draw(GraphicsDevice graphics)
	{
		if (GameState.instance.hasCap && GameState.instance.hasChain && GameState.instance.hasGlasses)
			Renderer.DrawModel(model, Matrix.CreateTranslation(0, MathF.Sin(Time.gameTime * 3), 0) * getModelMatrix());
	}
}
