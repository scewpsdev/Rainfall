using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PointLight
{
	const int RESOLUTION = 512;


	public Vector3 offset;
	public Vector3 color;

	public PointShadowMap shadowMap;


	public PointLight(Vector3 position, Vector3 color, GraphicsDevice graphics, float nearPlane = 0.1f)
	{
		this.offset = position;
		this.color = color;

		shadowMap = new PointShadowMap(RESOLUTION, nearPlane, graphics);
	}

	public PointLight(Vector3 position, Vector3 color)
	{
		this.offset = position;
		this.color = color;
	}

	public void destroy(GraphicsDevice graphics)
	{
		if (shadowMap != null)
			shadowMap.destroy(graphics);
	}

	public void updateShadowMap()
	{
		shadowMap.needsUpdate = true;
	}
}
