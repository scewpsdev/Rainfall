using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Elevator : Door
{
	public Elevator()
		: base(null, null, false, 0)
	{
		sprite = new Sprite(tileset, 3, 4, 2, 2);
		rect = new FloatRect(-1, 0, 2, 2);

		collider = new Hitbox(-1, 0, 2, 2);
	}

	public override void render()
	{
		base.render();

		Renderer.DrawLight(position + new Vector2(0, 2.5f), new Vector3(0.6f, 0.8f, 1.0f) * 4, 0.7f);
	}
}
