using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DirectionalLight
{
	const int SHADOW_MAP_RESOLUTION = 8192;


	public Vector3 direction;
	public Vector3 color;

	public ShadowMap shadowMap;


	public DirectionalLight(Vector3 direction, Vector3 color, GraphicsDevice graphics)
	{
		this.direction = direction;
		this.color = color;

		shadowMap = new ShadowMap(SHADOW_MAP_RESOLUTION, this, graphics);
	}
}
