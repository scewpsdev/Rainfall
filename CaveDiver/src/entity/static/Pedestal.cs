using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Pedestal : Entity
{
	Sprite sprite;


	public Pedestal()
	{
		sprite = new Sprite(tileset, 0, 6);
		collider = new FloatRect(-0.5f, 0, 1, 7 / 16.0f);
		platformCollider = true;
	}

	public override void init(Level level)
	{
		level.addEntityCollider(this);
	}

	public override void destroy()
	{
		level.removeEntityCollider(this);
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, LAYER_BG, 1, 1, 0, sprite);
	}
}
