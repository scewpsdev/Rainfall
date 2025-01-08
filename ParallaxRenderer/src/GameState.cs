using Microsoft.Win32;
using Rainfall;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;


public class GameState : State
{
	public static GameState instance;


	Level level;


	public GameState()
	{
		instance = this;

		level = new Level();
		level.addEntity(new Camera(), new Vector3(0, 0, 0));
	}

	public override void init()
	{
		loadScene("test_scene.gltf");
	}

	unsafe void loadScene(string path)
	{
		Model scene = Resource.GetModel(path, false);
		foreach (Node node in scene.skeleton.nodes)
		{
			for (int i = 0; i < node.meshes.Length; i++)
			{
				int meshID = node.meshes[i];
				MeshData* mesh = &scene.scene->meshes[meshID];

				if (mesh->materialID != -1)
				{
					MaterialData* material = &scene.scene->materials[mesh->materialID];
					string texturePath = new string((sbyte*)material->diffuse->path);
					texturePath = StringUtils.AbsolutePath(texturePath, path);
					Texture texture = Resource.GetTexture(texturePath, false);

					int numSubMeshes = mesh->vertexCount / 4;
					for (int k = 0; k < numSubMeshes; k++)
					{
						Vector2 min = new Vector2(float.MaxValue);
						Vector2 max = new Vector2(float.MinValue);
						float layer = 0;
						Vector2 uv0 = new Vector2(float.MaxValue);
						Vector2 uv1 = new Vector2(float.MinValue);
						for (int j = k * 4; j < k * 4 + 4; j++)
						{
							PositionNormalTangent* vertex = &mesh->vertices[j];
							Vector3 position = node.transform * vertex->position;
							min = Vector2.Min(min, position.xy);
							max = Vector2.Max(max, position.xy);
							layer = position.z;

							Vector2 uv = mesh->texcoords[j];
							uv0 = Vector2.Min(uv0, uv);
							uv1 = Vector2.Max(uv1, uv);
						}

						Entity entity = new Entity();

						int u0 = (int)MathF.Round(uv0.x * texture.width);
						int v0 = (int)MathF.Round(uv0.y * texture.height);
						int w = (int)MathF.Round((uv1.x - uv0.x) * texture.width);
						int h = (int)MathF.Round((uv1.y - uv0.y) * texture.height);
						entity.sprite = new Sprite(texture, u0, v0, w, h);

						Vector2 center = (min + max) * 0.5f;
						Vector2 size = max - min;
						entity.rect = new FloatRect(-0.5f * size, size);
						level.addEntity(entity, new Vector3(center, layer));
					}
				}
			}
		}
	}

	public override void destroy()
	{
		level.destroy();
	}

	public override void update()
	{
		level.update();
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.ambientLight = Vector3.One;

		level.render();
	}
}
