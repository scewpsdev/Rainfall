using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class WeaponStand : Entity
{
	Model model;
	RigidBody body;

	Item[] items;

	ItemPickup[] weapons = new ItemPickup[3];
	Matrix[] weaponOffsets = new Matrix[3];


	public WeaponStand(Item[] items)
	{
		this.items = items;

		model = Resource.GetModel("res/entity/object/weapon_stand/weapon_stand.gltf");
		model.configureLODs(LOD.DISTANCE_MEDIUM);

		weaponOffsets[0] = Matrix.CreateTranslation(-0.28f, 1.137f, -0.1788f) * Matrix.CreateRotation(Vector3.Right, MathHelper.ToRadians(-18)) * Matrix.CreateRotation(Vector3.UnitZ, MathF.PI);
		weaponOffsets[1] = Matrix.CreateTranslation(0.0f, 1.137f, -0.1788f) * Matrix.CreateRotation(Vector3.Right, MathHelper.ToRadians(-18)) * Matrix.CreateRotation(Vector3.UnitZ, MathF.PI);
		weaponOffsets[2] = Matrix.CreateTranslation(0.28f, 1.137f, -0.1788f) * Matrix.CreateRotation(Vector3.Right, MathHelper.ToRadians(-18)) * Matrix.CreateRotation(Vector3.UnitZ, MathF.PI);
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static);
		body.addBoxCollider(new Vector3(0.5f, 0.6f, 0.1f), new Vector3(0.0f, 0.6f, -0.1f), Quaternion.FromAxisAngle(Vector3.Right, MathHelper.ToRadians(-18)));

		for (int i = 0; i < 3; i++)
		{
			if (items[i] != null)
			{
				weapons[i] = new ItemPickup(items[i], 1, null, null, false);
				Matrix weaponTransform = getModelMatrix() * weaponOffsets[i];
				DungeonGame.instance.level.addEntity(weapons[i], weaponTransform.translation, weaponTransform.rotation);
			}
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix());
	}
}
