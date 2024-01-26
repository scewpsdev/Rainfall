using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class Tree : Entity
{
	Model model;
	RigidBody body;


	public Tree()
	{
		model = Resource.GetModel("res/entity/decoration/tree/tree.gltf");
		model.configureLODs(LOD.DISTANCE_BIG);
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static);
		body.addCapsuleCollider(0.7f, 2.0f, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(18.0f)));
	}

	public override void draw(GraphicsDevice graphics)
	{
		//Renderer.DrawModel(model, getModelMatrix());
		Matrix transform = getModelMatrix();
		Renderer.DrawSubModel(model, 0, transform);
		Renderer.DrawLeaves(model, 1, transform);
	}
}
