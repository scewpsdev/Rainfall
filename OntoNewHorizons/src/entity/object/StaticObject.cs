using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class StaticObject : Entity
{
	protected Model model;
	protected RigidBody body;


	protected StaticObject()
	{
	}

	public override void draw(GraphicsDevice graphics)
	{
		if (model != null)
			Renderer.DrawModel(model, getModelMatrix());
	}
}
