using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PointShadowMap
{
	public const int resolution = 512;

	public Cubemap cubemap;
	public RenderTarget[] renderTargets = new RenderTarget[6];
	internal float nearPlane;

	public bool needsUpdate = true;


	public PointShadowMap(float nearPlane, GraphicsDevice graphics)
	{
		this.nearPlane = nearPlane;

		cubemap = graphics.createCubemap(resolution, TextureFormat.D16F, (ulong)TextureFlags.RenderTarget);

		for (int i = 0; i < renderTargets.Length; i++)
		{
			renderTargets[i] = graphics.createRenderTarget(new RenderTargetAttachment[]
			{
				new RenderTargetAttachment(cubemap, i, false)
			});
		}
	}

	public void destroy(GraphicsDevice graphics)
	{
		graphics.destroyCubemap(cubemap);

		for (int i = 0; i < renderTargets.Length; i++)
		{
			graphics.destroyRenderTarget(renderTargets[i]);
		}
	}
}
