using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DirectionalLight
{
	const int SHADOW_MAP_RESOLUTION = 2048;


	public Vector3 direction;
	public Vector3 color;

	// for static shadowmaps
	public Vector3 position;
	public Vector3 volumeSize;

	public DirectionalShadowMap shadowMap;
	public bool dynamicShadowMap = true;


	public DirectionalLight(Vector3 direction, Vector3 color, GraphicsDevice graphics)
	{
		this.direction = direction;
		this.color = color;

		shadowMap = new DirectionalShadowMap(SHADOW_MAP_RESOLUTION, this, graphics);
	}

	public DirectionalLight(Vector3 position, Vector3 direction, Vector3 volumeSize, Vector3 color, GraphicsDevice graphics)
	{
		this.direction = direction;
		this.color = color;
		this.position = position;
		this.volumeSize = volumeSize;

		dynamicShadowMap = false;

		shadowMap = new DirectionalShadowMap(SHADOW_MAP_RESOLUTION, this, graphics);
	}
}
