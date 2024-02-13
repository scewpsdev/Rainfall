using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class BookShelf : Entity
{
	Model model;
	RigidBody body;

	Item[] items;
	int[] amounts;
	Random random;


	public BookShelf(Item[] items, int[] amounts, Random random)
	{
		this.items = items;
		this.amounts = amounts;
		this.random = random;

		model = Resource.GetModel("res/entity/object/shelf/shelf.gltf");
		model.maxDistance = LOD.DISTANCE_MEDIUM;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static);
		body.addBoxCollider(new Vector3(0.05f, 1.0f, 0.3f), new Vector3(-0.95f, 1.0f, 0.0f), Quaternion.Identity);
		body.addBoxCollider(new Vector3(0.05f, 1.0f, 0.3f), new Vector3(0.95f, 1.0f, 0.0f), Quaternion.Identity);
		body.addBoxCollider(new Vector3(1.0f, 0.05f, 0.25f), new Vector3(0.0f, 0.25f, 0.0f), Quaternion.Identity);
		body.addBoxCollider(new Vector3(1.0f, 0.05f, 0.25f), new Vector3(0.0f, 0.75f, 0.0f), Quaternion.Identity);
		body.addBoxCollider(new Vector3(1.0f, 0.05f, 0.25f), new Vector3(0.0f, 1.25f, 0.0f), Quaternion.Identity);
		body.addBoxCollider(new Vector3(1.0f, 0.05f, 0.25f), new Vector3(0.0f, 1.75f, 0.0f), Quaternion.Identity);

		for (int i = 0; i < items.Length; i++)
		{
			Item item = items[i];
			int amount = amounts[i];
			ItemPickup pickup = new ItemPickup(item, amount);
			int level = random.Next() % 3;
			Matrix itemTransform = getModelMatrix()
				* Matrix.CreateTranslation(new Vector3(MathHelper.RandomFloat(-0.8f, 0.8f, random), 0.3f + level * 0.5f + 0.1f, MathHelper.RandomFloat(-0.2f, 0.2f, random)))
				* Matrix.CreateRotation(Vector3.Up, random.NextSingle() * MathF.PI * 2);
			DungeonGame.instance.level.addEntity(pickup, itemTransform.translation, itemTransform.rotation);
		}
	}

	public override void destroy()
	{
		model.destroy();
		body.destroy();
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix());
	}
}
