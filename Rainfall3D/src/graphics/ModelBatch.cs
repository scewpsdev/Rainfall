using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace Rainfall
{
	public class ModelBatch
	{
		List<PositionNormalTangent> vertices = new List<PositionNormalTangent>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> indices = new List<int>();

		MaterialData material;

		Model model;


		public ModelBatch(int numVertices = 0, int numIndices = 0)
		{
			if (numVertices != 0)
			{
				vertices.Capacity = numVertices;
				uvs.Capacity = numVertices;
			}
			if (numIndices != 0)
				indices.Capacity = numIndices;
		}

		public void destroy()
		{
			model.destroy();
		}

		public void setMaterial(Material material)
		{
			this.material = material.data;
		}

		public int addVertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 uv)
		{
			int index = vertices.Count;

			PositionNormalTangent vertex;
			vertex.position = position;
			vertex.normal = normal;
			vertex.tangent = tangent;
			vertices.Add(vertex);

			uvs.Add(uv);

			return index;
		}

		public void addTriangle(int i0, int i1, int i2)
		{
			indices.Add(i0);
			indices.Add(i1);
			indices.Add(i2);
		}

		public void addModel(Model model, Matrix transform, int atlasIndex, Vector2i atlasSize)
		{
			material = model.getMaterialData(0).Value;
			for (int i = 0; i < model.meshCount; i++)
			{
				int indexOffset = vertices.Count;
				MeshData mesh = model.getMeshData(i).Value;
				for (int j = 0; j < mesh.vertexCount; j++)
				{
					PositionNormalTangent vertex = mesh.getVertex(j);
					vertex.position = (transform * new Vector4(vertex.position, 1.0f)).xyz;
					vertex.normal = (transform * new Vector4(vertex.normal, 0.0f)).xyz;
					vertex.tangent = (transform * new Vector4(vertex.tangent, 0.0f)).xyz;
					vertices.Add(vertex);

					Vector2 uv = mesh.getUV(j);
					int atlasX = atlasIndex % atlasSize.x;
					int atlasY = atlasIndex / atlasSize.x;
					uv = (uv + new Vector2(atlasX, atlasY)) / atlasSize;
					uvs.Add(uv);
				}
				for (int j = 0; j < mesh.indexCount; j++)
				{
					indices.Add(indexOffset + mesh.getIndex(j));
				}
			}
		}

		public void addModel(Model model, Matrix transform)
		{
			addModel(model, transform, 0, new Vector2i(1, 1));
		}

		public Model createModel()
		{
			if (vertices.Count > 0)
			{
				model = new Model(vertices.Count, CollectionsMarshal.AsSpan(vertices), CollectionsMarshal.AsSpan(uvs), indices.Count, CollectionsMarshal.AsSpan(indices), material);
				vertices = null;
				uvs = null;
				indices = null;
				return model;
			}
			return null;
		}
	}
}
