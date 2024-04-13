using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class GrassPatch
{
	Vector2 position;
	Terrain terrain;


	public GrassPatch(Terrain terrain, Vector2 position)
	{
		this.terrain = terrain;
		this.position = position;
	}

	public void draw(GraphicsDevice graphics)
	{
		Renderer.DrawGrassPatch(terrain, position);
	}
}
