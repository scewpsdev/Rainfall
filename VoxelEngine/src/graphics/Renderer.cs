using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class Renderer
{
	struct CubeDraw
	{
		internal Vector3 position;
		internal Vector3 size;
		internal Texture texture;
		internal Texture normals;
	}


	static GraphicsDevice graphics;
	static Camera camera;
	static Matrix projection, view;

	static VertexBuffer boxVertexBuffer;
	static IndexBuffer boxIndexBuffer;
	static Shader chunkShader;

	static List<CubeDraw> cubeDraws = new List<CubeDraw>();


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

		chunkShader = Resource.GetShader("res/shaders/chunk/chunk.vs.shader", "res/shaders/chunk/chunk.fs.shader");
	}

	public static void DrawChunk(Vector3 position, Vector3 size, Texture texture, Texture normals)
	{
		cubeDraws.Add(new CubeDraw() { position = position, size = size, texture = texture, normals = normals });
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

	public static void End()
	{
		graphics.resetState();

		graphics.setViewTransform(projection, view);

		foreach (CubeDraw draw in cubeDraws)
		{
			Matrix transform = Matrix.CreateTranslation(draw.position);

			graphics.setCullState(CullState.CounterClockWise);

			graphics.setTransform(transform);

			Vector3 localCameraPosition = transform.inverted * camera.position;
			graphics.setUniform(chunkShader, "u_cameraPosition", new Vector4(localCameraPosition, 0.0f));
			graphics.setUniform(chunkShader, "u_boxSize", new Vector4(draw.size, 0.0f));

			graphics.setTexture(chunkShader, "u_voxels", 0, draw.texture);
			graphics.setTexture(chunkShader, "u_normals", 1, draw.normals);

			graphics.setVertexBuffer(boxVertexBuffer);
			graphics.setIndexBuffer(boxIndexBuffer);

			graphics.draw(chunkShader);
		}

		cubeDraws.Clear();
	}
}
