using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Pot : Entity, Hittable
{
	Model model, fracturedModel;
	RigidBody body;

	Sound[] sfxBreak;


	public Pot(int type)
	{
		if (type == 0)
		{
			model = Resource.GetModel("res/entity/object/pot/pot.gltf");
			fracturedModel = Resource.GetModel("res/entity/object/pot/pot_fractured.gltf");
		}
		else if (type == 1)
		{
			model = Resource.GetModel("res/entity/object/pot/pot_large.gltf");
			fracturedModel = Resource.GetModel("res/entity/object/pot/pot_large_fractured.gltf");
		}
		else if (type == 2)
		{
			model = Resource.GetModel("res/entity/object/pot/pot_vase.gltf");
			fracturedModel = Resource.GetModel("res/entity/object/pot/pot_vase_fractured.gltf");
		}

		model.maxDistance = (LOD.DISTANCE_MEDIUM);
		model.isStatic = false;
		model.maxDistance = (LOD.DISTANCE_SMALL);
		fracturedModel.isStatic = false;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static);
		body.addCapsuleCollider(0.3f, 0.9f, new Vector3(0.0f, 0.45f, 0.0f), Quaternion.Identity);

		sfxBreak = new Sound[]
		{
			Resource.GetSound("res/entity/object/pot/sfx/break1.ogg"),
			Resource.GetSound("res/entity/object/pot/sfx/break2.ogg"),
			Resource.GetSound("res/entity/object/pot/sfx/break3.ogg"),
			Resource.GetSound("res/entity/object/pot/sfx/break4.ogg"),
		};
	}

	public override void destroy()
	{
		model.destroy();
		body.destroy();
	}

	public void hit(int damage, Entity from, Vector3 hitPosition, Vector3 force, int linkID)
	{
		remove();
		//if (fracturedModel != null)
		//	DungeonGame.instance.level.addEntity(new FracturedObject(fracturedModel, sfxBreak), position, rotation);

		float flaskDropChance = 0.05f;
		if (Random.Shared.NextSingle() < flaskDropChance)
			DungeonGame.instance.level.addEntity(new ItemPickup(Item.Get("flask")), position, Quaternion.Identity);

		float manaFlaskDropChance = 0.05f;
		if (Random.Shared.NextSingle() < manaFlaskDropChance)
			DungeonGame.instance.level.addEntity(new ItemPickup(Item.Get("mana_flask")), position, Quaternion.Identity);

		float firebombDropChance = 0.05f;
		if (Random.Shared.NextSingle() < firebombDropChance)
			DungeonGame.instance.level.addEntity(new ItemPickup(Item.Get("firebomb")), position, Quaternion.Identity);
	}

	public override void update()
	{
	}

	public override void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();
		Renderer.DrawModel(model, transform);
	}
}
