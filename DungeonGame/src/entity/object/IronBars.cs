using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class IronBars : Entity
{
	Model model;
	RigidBody body;


	public IronBars()
	{
		model = Resource.GetModel("res/entity/object/iron_bars/iron_bars.gltf");
		model.maxDistance = (LOD.DISTANCE_MEDIUM);
	}

	public void activate()
	{
		position.y = -1.9f;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Kinematic);
		body.addBoxCollider(new Vector3(4.0f, 1.0f, 0.5f), new Vector3(0.0f, 1.0f, 0.0f), Quaternion.Identity);
	}

	public override void destroy()
	{
		model.destroy();
		body.destroy();
	}

	public override void update()
	{
		body.setTransform(position, rotation);
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawModel(model, getModelMatrix());
	}
}
