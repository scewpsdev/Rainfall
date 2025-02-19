using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


public abstract class Item : Entity
{
	const float RADIUS = 2;


	public Item()
	{
		load("highlight.rfs");

		body = new RigidBody(this, RigidBodyType.Kinematic);
		body.addSphereTrigger(RADIUS, Vector3.Zero);
	}

	protected abstract void onCollect(Player player);

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (other != null && other.entity is Cart)
		{
			onCollect(GameState.instance.player);
			GameState.instance.scene.addEntity(new ItemCollectEffect(other.entity as Cart), other.entity.getPosition() + Vector3.Up);
			remove();
		}
	}

	public override unsafe void update()
	{
		modelTransform = Matrix.CreateTranslation(0, MathF.Sin(Time.gameTime) * 0.5f, 0) * Matrix.CreateRotation(Vector3.Up, Time.gameTime) * Matrix.CreateScale(5);
		particles[0].handle->spawnOffset = new Vector3(0, MathF.Sin(Time.gameTime) * 0.5f, 0);

		base.update();
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		Renderer.DrawLight(position, new Vector3(1, 0.9f, 0.4f) * 5);
	}
}
