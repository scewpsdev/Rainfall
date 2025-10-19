using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Foliage : Entity
{
	public Foliage()
	{
		material = Material.Create(Resource.GetShader("shaders/foliage/foliage.vsh", "shaders/foliage/foliage.fsh"));
	}

	public override void init()
	{
		base.init();

		material.copyData(model.getMaterialHandleForMeshNode(meshNode));
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		material.setData(3, new Vector4(0, 0, 0, Time.gameTime));
	}
}
