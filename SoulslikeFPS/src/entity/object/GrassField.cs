using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


struct GrassData
{
	public Vector4 positionRotation;
}

internal class GrassField : Entity
{
	const int NUM_ROWS = 128;
	const int NUM_GRASS_BLADES = NUM_ROWS * NUM_ROWS;
	const float GRASS_PATCH_SIZE = 20.0f;


	GrassData[] data;
	VertexBuffer grassBlade;
	IndexBuffer grassIndices;

	Texture texture;
	Texture noise;
	Shader shader;
	Material material;


	public GrassField()
	{
		data = new GrassData[NUM_GRASS_BLADES];

		for (int i = 0; i < NUM_GRASS_BLADES; i++)
		{
			//int xx = i % 256 % 16 * 4 + i / 256 % 4 % 2 * 2 + i / 1024 % 2;
			//int zz = i % 256 / 16 * 4 + i / 256 % 4 / 2 * 2 + i / 1024 / 2;
			int xx = i % NUM_ROWS;
			int zz = i / NUM_ROWS;
			Vector3 position = Vector3.Zero;
			position.x = xx / (float)NUM_ROWS * GRASS_PATCH_SIZE;
			position.z = zz / (float)NUM_ROWS * GRASS_PATCH_SIZE;
			position += Mathf.RandomVector3(-1, 1) * 0.2f * new Vector3(1, 0, 1);

			float rotation = Mathf.RandomFloat(0.0f, MathF.PI * 2.0f);

			data[i].positionRotation = new Vector4(position, rotation);
		}

		model = Resource.GetModel("map/grass.gltf");
		grassBlade = Renderer.graphics.createVertexBuffer(
			Renderer.graphics.createVideoMemory(new float[] {
				-0.1f, 0.0f, 0.0f,
				0.1f, 0.0f, 0.0f,
				0.0f, 1.0f, 0.0f,
				-0.5f, 1.0f, 0.0f,
			}),
			stackalloc VertexElement[] { new VertexElement(VertexAttribute.Position, VertexAttributeType.Vector3, false) }
		);
		grassIndices = Renderer.graphics.createIndexBuffer(
			Renderer.graphics.createVideoMemory(new short[] { 0, 1, 2 })
		);

		texture = Resource.GetTexture("map/grass.png");
		noise = Resource.GetTexture("texture/perlin1.png");
		shader = Resource.GetShader("shaders/grass/grass.vsh", "shaders/grass/grass.fsh");
		material = Material.Create(shader);
	}

	public override unsafe void draw(GraphicsDevice graphics)
	{
		graphics.createInstanceBuffer(NUM_GRASS_BLADES, 16, out InstanceBufferData grassInstances);
		grassInstances.write(data);

		material.setData(0, new Vector4(position.xz, GRASS_PATCH_SIZE, Time.gameTime));

		//material.setTexture(0, texture);
		material.setTexture(1, noise);

		//material.setTexture(0, grassPatches[i].terrain.heightmap);
		//material.setTexture(1, grassPatches[i].terrain.normalmap);
		//material.setTexture(2, grassPatches[i].terrain.splatMap);

		MeshData* mesh = model.getMeshData(0);
		Renderer.DrawCustomGeometry([mesh->vertexBufferID, mesh->texcoordBufferID], mesh->indexBufferID, grassInstances, getModelMatrix(), material);
	}
}
