using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


public class ResizableLadder : LadderRegion
{
	int height;

	Model top, middle;


	public ResizableLadder(int height)
		: base(new Vector3(0.5f, height * 0.5f, 0.1f), new Vector3(0.0f, height * 0.5f, 0.0f))
	{
		this.height = height;

		top = Resource.GetModel("res/entity/object/ladder/ladder_top.gltf");
		top.maxDistance = (LOD.DISTANCE_MEDIUM);

		middle = Resource.GetModel("res/entity/object/ladder/ladder_middle.gltf");
		middle.maxDistance = (LOD.DISTANCE_MEDIUM);
	}

	public override void destroy()
	{
		base.destroy();

		top.destroy();
		middle.destroy();
	}

	public override void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();
		for (int i = 0; i < height - 1; i++)
		{
			Renderer.DrawModel(middle, transform * Matrix.CreateTranslation(0, i, 0));
		}
		Renderer.DrawModelStaticInstanced(top, transform * Matrix.CreateTranslation(0, height - 1, 0));
	}
}
