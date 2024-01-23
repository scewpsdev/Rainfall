using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LightObject : Entity
{
	Vector3 color;

	public LightObject(Vector3 color)
	{
		this.color = color;
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawLight(position, color);
	}
}
