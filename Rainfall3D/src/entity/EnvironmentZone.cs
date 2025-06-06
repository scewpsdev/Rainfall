using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EnvironmentZone : Entity
{
	ReflectionProbe probe;

	internal bool needsUpdate = true;
	bool empty;


	public EnvironmentZone(Vector3 size, bool empty = false)
	{
		scale = size;
		this.empty = empty;
	}

	public override void init()
	{
		if (empty)
		{
			probe = new ReflectionProbe(16, position, scale, 300, position, Renderer.graphics);
			needsUpdate = false;
		}
		else
		{
			probe = new ReflectionProbe(256, position, scale, 300, position, Renderer.graphics);
			probe.ambientLight = new Vector3(0.527f, 0.761f, 1.0f) * 0.005f;
		}
	}

	public override void update()
	{
		if (Input.IsKeyPressed(KeyCode.F5))
			needsUpdate = true;
	}

	public override void draw(GraphicsDevice graphics)
	{
		Renderer.DrawReflectionProbe(probe, needsUpdate);
		needsUpdate = false;
	}
}
