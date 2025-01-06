using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Pot : Barrel
{
	public Pot(params Item[] items)
		: base(items)
	{
		sprite = Random.Shared.NextSingle() < 0.5f ? new Sprite(tileset, 11, 0, 1, 2) : new Sprite(tileset, 12, 0, 1, 2);
		rect = new(-0.5f, 0, 1, 2);

		collider = new FloatRect(-0.4f, 0.0f, 0.8f, 1.0f);
		platformCollider = true;

		hitSound = Item.woodHit;
		breakSound = Resource.GetSounds("sounds/break_pot", 4);
	}

	public Pot()
		: this(null)
	{
	}
}
