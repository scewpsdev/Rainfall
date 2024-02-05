using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PointLight
{
	const int RESOLUTION = 256;


	public Vector3 position;
	public Vector3 color;

	public PointShadowMap shadowMap;


	public PointLight(Vector3 position, Vector3 color, GraphicsDevice graphics, float nearPlane = 0.1f)
	{
		this.position = position;
		this.color = color;

		shadowMap = new PointShadowMap(RESOLUTION, nearPlane, graphics);
	}

	public void updateShadowMap()
	{
		shadowMap.needsUpdate = true;
	}
}
