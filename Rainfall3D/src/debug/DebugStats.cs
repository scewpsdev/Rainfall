using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public static class DebugStats
	{
		public static int Draw(int x, int y, byte color, GraphicsDevice graphics)
		{
			y = graphics.drawDebugInfo(x, y, color);

			y++;

			Span<byte> str = stackalloc byte[128];

			StringUtils.WriteString(str, "Meshes: ");
			StringUtils.AppendInteger(str, Renderer.meshRenderCounter);
			graphics.drawDebugText(x, y++, color, str);

			StringUtils.WriteString(str, "Culled: ");
			StringUtils.AppendInteger(str, Renderer.meshCulledCounter);
			graphics.drawDebugText(x, y++, color, str);

			StringUtils.WriteString(str, "Physics bodies: ");
			StringUtils.AppendInteger(str, RigidBody.numBodies);
			graphics.drawDebugText(x, y++, color, str);

			y++;

			RenderStats renderStats = graphics.getRenderStats();

			StringUtils.WriteString(str, "Geometry Pass: ");
			StringUtils.AppendFloat(str, renderStats.getGpuTime((ushort)Renderer.RenderPass.Geometry) * 1000, 2);
			StringUtils.AppendString(str, " ms");
			graphics.drawDebugText(x, y++, color, str);

			StringUtils.WriteString(str, "Ambient Occlusion Pass: ");
			StringUtils.AppendFloat(str, renderStats.getCumulativeGpuTime((ushort)Renderer.RenderPass.AmbientOcclusion, 2) * 1000, 2);
			StringUtils.AppendString(str, " ms");
			graphics.drawDebugText(x, y++, color, str);

			StringUtils.WriteString(str, "Deferred Pass: ");
			StringUtils.AppendFloat(str, renderStats.getGpuTime((ushort)Renderer.RenderPass.Deferred) * 1000, 2);
			StringUtils.AppendString(str, " ms");
			graphics.drawDebugText(x, y++, color, str);

			StringUtils.WriteString(str, "Forward Pass: ");
			StringUtils.AppendFloat(str, renderStats.getGpuTime((ushort)Renderer.RenderPass.Forward) * 1000, 2);
			StringUtils.AppendString(str, " ms");
			graphics.drawDebugText(x, y++, color, str);

			StringUtils.WriteString(str, "Bloom Pass: ");
			StringUtils.AppendFloat(str, renderStats.getCumulativeGpuTime((ushort)Renderer.RenderPass.BloomDownsample, Renderer.RenderPass.Composite - Renderer.RenderPass.BloomDownsample) * 1000, 2);
			StringUtils.AppendString(str, " ms");
			graphics.drawDebugText(x, y++, color, str);

			StringUtils.WriteString(str, "Tonemapping Pass: ");
			StringUtils.AppendFloat(str, renderStats.getGpuTime((ushort)Renderer.RenderPass.Tonemapping) * 1000, 2);
			StringUtils.AppendString(str, " ms");
			graphics.drawDebugText(x, y++, color, str);

			StringUtils.WriteString(str, "UI Pass: ");
			StringUtils.AppendFloat(str, renderStats.getGpuTime((ushort)Renderer.RenderPass.UI) * 1000, 2);
			StringUtils.AppendString(str, " ms");
			graphics.drawDebugText(x, y++, color, str);

			return y;
		}
	}
}
