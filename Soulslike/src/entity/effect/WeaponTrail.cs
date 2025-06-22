using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class WeaponTrail
{
	[StructLayout(LayoutKind.Sequential)]
	struct Vertex
	{
		public Vector3 position;
		public Vector3 uv;
	}


	DynamicVertexBuffer vertexBuffer;
	Material material;

	Vertex[] points;


	public WeaponTrail(int numPoints, Vector3 position)
	{
		Span<VertexElement> layout = [new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector3, false), new VertexElement(VertexAttribute.Normal, VertexAttributeType.Vector3, false)];
		vertexBuffer = Renderer.graphics.createDynamicVertexBuffer(layout, numPoints * 2);

		material = new Material(Resource.GetShader("shaders/weapon_trail/weapon_trail.vsh", "shaders/weapon_trail/weapon_trail.fsh"));
		material.setTexture(0, Resource.GetTexture("texture/effect/weapon_trail.png", (uint)SamplerFlags.VClamp));

		points = new Vertex[numPoints * 2];
		for (int i = 0; i < numPoints; i++)
		{
			points[i * 2 + 0].position = position;
			points[i * 2 + 1].position = position;
			points[i * 2 + 0].uv = new Vector3(0, 1, 0);
			points[i * 2 + 1].uv = new Vector3(0, 0, 0);
		}
	}

	public void destroy()
	{
		Renderer.graphics.destroyDynamicVertexBuffer(vertexBuffer);
		material.destroy();
	}

	public void update(Vector3 bas, Vector3 tip, float alpha)
	{
		int numPoints = points.Length / 2;
		for (int i = numPoints - 1; i >= 1; i--)
		{
			points[i * 2] = points[(i - 1) * 2];
			points[i * 2 + 1] = points[(i - 1) * 2 + 1];
		}

		points[0].position = bas;
		points[1].position = tip;

		float xd = (tip - points[3].position).length;
		float yd = (tip - bas).length;
		float scroll = xd / yd;

		points[0].uv.xy = points[2].uv.xy + Vector2.Right * scroll;
		points[1].uv.xy = points[3].uv.xy + Vector2.Right * scroll;
		points[0].uv.z = alpha;
		points[1].uv.z = alpha;

		Renderer.graphics.updateDynamicVertexBuffer(vertexBuffer, 0, Renderer.graphics.createVideoMemory<Vertex>(points));
	}

	public void draw()
	{
		Span<DynamicVertexBuffer> buffers = [vertexBuffer];
		Renderer.DrawCustomGeometry(buffers, null, Matrix.Identity, material, PrimitiveType.TriangleStrip);
	}
}
