using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DebugLine : Entity
{
	Vector3 p1, p2;
	uint color;


	public DebugLine(Vector3 p1, Vector3 p2, uint color)
	{
		this.p1 = p1;
		this.p2 = p2;
		this.color = color;
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawDebugLine(p1, p2, color);
	}
}
