using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ReflectionProbe
{
	public readonly Vector3 position;
	public readonly Vector3 size;
	public readonly Vector3 origin;

	internal Cubemap cubemap;
	internal Cubemap cubemapDepth;
	internal RenderTarget[] renderTargets = new RenderTarget[6];

	internal bool needsUpdate = true;


	public ReflectionProbe(int resolution, Vector3 position, Vector3 size, Vector3 origin, GraphicsDevice graphics)
	{
		this.position = position;
		this.size = size;
		this.origin = origin;

		cubemap = graphics.createCubemap(resolution, TextureFormat.RG11B10F, (ulong)TextureFlags.RenderTarget);
		cubemapDepth = graphics.createCubemap(resolution, TextureFormat.D16F, (ulong)TextureFlags.RenderTarget);

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
