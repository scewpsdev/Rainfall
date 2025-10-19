using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ReflectionProbe
{
	public Vector3 position;
	public Vector3 size;
	public Vector3 origin;
	public float farPlane;
	public Vector3 ambientLight = Vector3.Zero;

	internal int resolution;
	internal Cubemap cubemap;
	internal Cubemap cubemapDepth;
	internal RenderTarget[] renderTargets = new RenderTarget[6];


	public ReflectionProbe(int resolution, Vector3 position, Vector3 size, float farPlane, Vector3 origin, GraphicsDevice graphics)
	{
		this.position = position;
		this.size = size;
		this.origin = origin;
		this.farPlane = farPlane;
		this.resolution = resolution;

		cubemap = graphics.createCubemap(resolution, TextureFormat.RG11B10F, true, (ulong)TextureFlags.RenderTarget);
		cubemapDepth = graphics.createCubemap(resolution, TextureFormat.D16F, false, (ulong)TextureFlags.RenderTarget);

		for (int i = 0; i < 6; i++)
		{
			renderTargets[i] = graphics.createRenderTarget(new RenderTargetAttachment[]
			{
				new RenderTargetAttachment(cubemap, i, true),
				new RenderTargetAttachment(cubemapDepth, i, false)
			});
		}
	}
}
