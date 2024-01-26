using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PointShadowMap
{
	public Cubemap cubemap;
	public RenderTarget[] renderTargets = new RenderTarget[6];

	internal bool needsUpdate = true;


	public PointShadowMap(int resolution, GraphicsDevice graphics)
	{
		cubemap = graphics.createCubemap(resolution, TextureFormat.D16F, (ulong)TextureFlags.RenderTarget);

		for (int i = 0; i < 6; i++)
		{
			renderTargets[i] = graphics.createRenderTarget(new RenderTargetAttachment[]
			{
				new RenderTargetAttachment(cubemap, i, false)
			});
		}
	}
}
