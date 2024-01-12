using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Renderer
{
	struct ChunkDraw
	{
		internal Vector3 position;
		internal Vector3 size;
		internal Texture octree;
	}


	const int PASS_GEOMETRY = 0;
	const int PASS_DISPLAY = 1;


	static GraphicsDevice graphics;
	static Camera camera;
	static Matrix projection, view;

	static Texture screen;
	static Shader screenShader;
	static VertexBuffer quad;

	static Texture brickgrid;
	static Texture brickgridLod;
	static Texture brickgridLod2;
	static Texture brickgridLod3;
	static Vector3 brickgridPosition;
	static Shader voxelShader;

	static VertexBuffer boxVertexBuffer;
	static IndexBuffer boxIndexBuffer;
	static Shader chunkShader;

	static List<ChunkDraw> chunkDraws = new List<ChunkDraw>();


	public static void Init(GraphicsDevice graphics)
	{
		Renderer.graphics = graphics;

		screen = graphics.createTexture(BackbufferRatio.Equal, false, TextureFormat.RGBA8, (ulong)TextureFlags.ComputeWrite | (uint)SamplerFlags.Clamp | (uint)SamplerFlags.Point);
		screenShader = Resource.GetShader("res/shaders/screen/screen.vs.shader", "res/shaders/screen/screen.fs.shader");
		quad = graphics.createVertexBuffer(graphics.createVideoMemory(stackalloc float[] { -3, -1, 1, -1, 1, 3 }), stackalloc VertexElement[] { new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector2, false) });

		voxelShader = Resource.GetShader("res/shaders/voxel/brickgrid.cs.shader");
		chunkShader = Resource.GetShader("res/shaders/voxel/chunk.vs.shader", "res/shaders/voxel/chunk.fs.shader");

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

	public static void DrawChunk(Vector3 position, Vector3 size, Texture octree)
	{
		chunkDraws.Add(new ChunkDraw { position = position, size = size, octree = octree });
	}

	public static void DrawVoxels(Texture brickgrid, Texture brickgridLod, Texture brickgridLod2, Texture brickgridLod3, Vector3 position)
	{
		Renderer.brickgrid = brickgrid;
		Renderer.brickgridLod = brickgridLod;
		Renderer.brickgridLod2 = brickgridLod2;
		Renderer.brickgridLod3 = brickgridLod3;
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

	static void GeometryPass()
	{
		graphics.resetState();
		graphics.setPass(0);
		graphics.setRenderTarget(null);

		graphics.setViewTransform(projection, view);

		foreach (ChunkDraw draw in chunkDraws)
		{
			graphics.setCullState(CullState.CounterClockWise);
			graphics.setBlendState(BlendState.Default);

			graphics.setVertexBuffer(boxVertexBuffer);
			graphics.setIndexBuffer(boxIndexBuffer);

			graphics.setTexture(chunkShader, "s_octree", 0, draw.octree);

			graphics.setUniform(chunkShader, "u_boxPosition", draw.position);
			graphics.setUniform(chunkShader, "u_boxSize", draw.size);
			graphics.setUniform(chunkShader, "u_cameraPosition", camera.position - draw.position);

			graphics.draw(chunkShader);
		}
	}

	static void GeometryPass2()
	{
		graphics.resetState();
		graphics.setPass(0);
		graphics.setRenderTarget(null);

		foreach (ChunkDraw draw in chunkDraws)
		{
			graphics.setCullState(CullState.ClockWise);
			graphics.setVertexBuffer(quad);
			graphics.setUniform(screenShader, "iResolution", new Vector4((Vector2)Display.viewportSize, Time.currentTime / 1e9f, 0));
			graphics.setTexture(screenShader, "s_octree", 0, draw.octree);
			graphics.setUniform(screenShader, "u_boxPosition", draw.position);
			graphics.setUniform(screenShader, "u_boxSize", draw.size);
			graphics.setUniform(screenShader, "u_cameraPosition", camera.position - draw.position);

			graphics.draw(screenShader);
		}
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
		graphics.setTexture(voxelShader, "s_brickgridLod3", 4, brickgridLod3);

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

		GeometryPass2();
		//RaytracingPass();
		//DisplayPass();

		chunkDraws.Clear();
	}
}
