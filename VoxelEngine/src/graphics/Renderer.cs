using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Renderer
{
	static GraphicsDevice graphics;
	static Camera camera;
	static Matrix projection, view;

	static Texture screen;
	static Shader screenShader;
	static VertexBuffer quad;

	static Texture brickgrid;
	static Texture brickgridLod;
	static Texture brickgridLod2;
	static Vector3 brickgridPosition;
	static Shader voxelShader;

	static VertexBuffer boxVertexBuffer;
	static IndexBuffer boxIndexBuffer;


	public static void Init(GraphicsDevice graphics)
	{
		Renderer.graphics = graphics;

		screen = graphics.createTexture(BackbufferRatio.Equal, false, TextureFormat.RGBA8, (ulong)TextureFlags.ComputeWrite | (uint)SamplerFlags.Clamp | (uint)SamplerFlags.Point);
		screenShader = Resource.GetShader("res/shaders/screen/screen.vs.shader", "res/shaders/screen/screen.fs.shader");
		quad = graphics.createVertexBuffer(graphics.createVideoMemory(stackalloc float[] { -3, -1, 1, -1, 1, 3 }), stackalloc VertexElement[] { new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector2, false) });

		voxelShader = Resource.GetShader("res/shaders/voxel/brickgrid.cs.shader");

		boxVertexBuffer = graphics.createVertexBuffer(
			graphics.createVideoMemory(stackalloc Vector3[] {
			new Vector3(0, 0, 0),
			new Vector3(1, 0, 0),
			new Vector3(1, 0, 1),
			new Vector3(0, 0, 1),
			new Vector3(0, 1, 0),
			new Vector3(1, 1, 0),
			new Vector3(1, 1, 1),
			new Vector3(0, 1, 1),
		}), stackalloc VertexElement[] {
			new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector3, false)
		});
		boxIndexBuffer = graphics.createIndexBuffer(
			graphics.createVideoMemory(stackalloc short[] { 0, 1, 2, 2, 3, 0, 0, 3, 7, 7, 4, 0, 3, 2, 6, 6, 7, 3, 2, 1, 5, 5, 6, 2, 1, 0, 4, 4, 5, 1, 4, 7, 6, 6, 5, 4 })
		);
	}

	public static void DrawVoxels(Texture brickgrid, Texture brickgridLod, Texture brickgridLod2, Vector3 position)
	{
		Renderer.brickgrid = brickgrid;
		Renderer.brickgridLod = brickgridLod;
		Renderer.brickgridLod2 = brickgridLod2;
		Renderer.brickgridPosition = position;
	}

	public static void Begin()
	{
	}

	public static void SetCamera(Camera camera)
	{
		Renderer.camera = camera;
		projection = camera.getProjectionMatrix();
		view = camera.getViewMatrix();
	}

	static void RaytracingPass()
	{
		graphics.resetState();
		graphics.setPass(1);

		graphics.setComputeTexture(0, screen, 0, ComputeAccess.Write);

		graphics.setUniform(voxelShader, "u_projInv", projection.inverted);
		graphics.setUniform(voxelShader, "u_viewInv", view.inverted);

		graphics.setUniform(voxelShader, "u_cameraPosition", new Vector4(camera.position, 0));
		graphics.setUniform(voxelShader, "u_gridPosition", new Vector4(brickgridPosition, 0));
		graphics.setUniform(voxelShader, "u_gridSize", new Vector4(256, 256, 256, 0));

		graphics.setTexture(voxelShader, "s_brickgrid", 1, brickgrid);
		graphics.setTexture(voxelShader, "s_brickgridLod", 2, brickgridLod);
		graphics.setTexture(voxelShader, "s_brickgridLod2", 3, brickgridLod2);

		int threadGroupSize = 32;
		int numX = (Display.viewportSize.x + threadGroupSize - 1) / threadGroupSize;
		int numY = (Display.viewportSize.y + threadGroupSize - 1) / threadGroupSize;
		graphics.computeDispatch(voxelShader, numX, numY, 1);
	}

	static void DisplayPass()
	{
		graphics.resetState();
		graphics.setPass(2);

		graphics.setRenderTarget(null);

		graphics.setDepthTest(DepthTest.None);
		graphics.setCullState(CullState.ClockWise);

		graphics.setVertexBuffer(quad);
		graphics.setTexture(screenShader, "s_frame", 0, screen);

		graphics.draw(screenShader);
	}

	public static void End()
	{
		graphics.setPass(0);

		RaytracingPass();
		DisplayPass();
	}
}
