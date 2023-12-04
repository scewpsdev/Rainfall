using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Pillar : Entity
{
	Model model;

	RigidBody body;


	public Pillar()
	{
		model = Resource.GetModel("res/entity/object/pillar/pillar.gltf");
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static);
		body.addBoxCollider(new Vector3(1.0f, 0.4f, 1.0f), new Vector3(0.0f, 0.4f, 0.0f), Quaternion.Identity);
		body.addBoxCollider(new Vector3(1.0f, 0.4f, 1.0f), new Vector3(0.0f, 9 - 0.4f, 0.0f), Quaternion.Identity);
		body.addCapsuleCollider(0.8f, 9.0f, new Vector3(0.0f, 4.5f, 0.0f), Quaternion.Identity);
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix());
	}
}
