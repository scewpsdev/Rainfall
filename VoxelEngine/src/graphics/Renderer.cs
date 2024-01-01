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

	static VertexBuffer boxVertexBuffer;
	static IndexBuffer boxIndexBuffer;

	static Texture brickgrid;
	static Texture brickgridLod;
	static Vector3 brickgridPosition;
	static Shader voxelShader;


	public static void Init(GraphicsDevice graphics)
	{
		Renderer.graphics = graphics;

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

		voxelShader = Resource.GetShader("res/shaders/voxel/brickgrid.vs.shader", "res/shaders/voxel/brickgrid.fs.shader");
	}

	public static void DrawVoxels(Texture brickgrid, Texture brickgridLod, Vector3 position)
	{
		Renderer.brickgrid = brickgrid;
		Renderer.brickgridLod = brickgridLod;
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

		graphics.setViewTransform(projection, view);

		graphics.setCullState(CullState.CounterClockWise);

		graphics.setUniform(voxelShader, "u_cameraPosition", new Vector4(camera.position, 0));
		graphics.setUniform(voxelShader, "u_gridPosition", new Vector4(brickgridPosition, 0));
		graphics.setUniform(voxelShader, "u_gridSize", new Vector4(256, 256, 256, 0));

		graphics.setTexture(voxelShader, "s_brickgrid", 0, brickgrid);
		graphics.setTexture(voxelShader, "s_brickgridLod", 1, brickgridLod);

		graphics.setVertexBuffer(boxVertexBuffer);
		graphics.setIndexBuffer(boxIndexBuffer);

		graphics.draw(voxelShader);
	}

	public static void End()
	{
		graphics.setPass(0);

		GeometryPass();
	}
}
