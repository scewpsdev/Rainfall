using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Lever : Entity, Interactable
{
	public Activatable activatable;

	float animDuration = 3;

	float leverRotation;
	long lastActivate = -1;


	public Lever()
	{
	}

	public override void init()
	{
		base.init();

		load("entity/object/lever/lever.rfs", PhysicsFilter.Default | PhysicsFilter.Interactable);
	}

	public void interact(Player player)
	{
		lastActivate = Time.currentTime;
	}

	public override void update()
	{
		if (lastActivate != -1)
		{
			float elapsed = (Time.currentTime - lastActivate) / 1e9f;
			if (elapsed >= animDuration)
			{
				elapsed = animDuration;
				lastActivate = -1;
				activatable.activate(this);
			}

			float progress = elapsed / animDuration;
			float t1 = 0.2f;
			float t2 = 0.3f;
			float anim = progress < t2 ? MathF.Max(1 - progress / t1, 0) : MathF.Abs(MathF.Sin((progress - t2) / (1 - t2) * 0.5f * MathF.PI));
			leverRotation = MathHelper.Lerp(MathHelper.ToRadians(30), MathHelper.ToRadians(-30), anim);
		}
		else
		{
			leverRotation = MathHelper.ToRadians(-30);
		}
	}

	public override void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();

		Renderer.DrawMesh(model, 0, transform);
		Renderer.DrawMesh(model, 1, transform * Matrix.CreateRotation(Vector3.Right, leverRotation));
	}
}
