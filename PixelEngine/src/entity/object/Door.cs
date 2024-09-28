using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Door : Entity, Interactable
{
	public Level destination;
	public Door otherDoor;
	public bool finalExit = false;

	Sprite sprite;
	FloatRect rect;
	uint outline = 0;

	Sound openSound;


	public Door(Level destination, Door otherDoor = null, bool big = false)
	{
		this.destination = destination;
		this.otherDoor = otherDoor;

		sprite = big ? new Sprite(TileType.tileset, 0, 9, 2, 2) : new Sprite(TileType.tileset, 2, 2);
		rect = big ? new FloatRect(-1.0f, 0.0f, 2.0f, 2.0f) : new FloatRect(-0.5f, 0.0f, 1.0f, 1.0f);

		collider = new FloatRect(-0.5f, 0.0f, 1, 1);

		openSound = Resource.GetSound("res/sounds/chest_close.ogg");
	}

	public void interact(Player player)
	{
		if (finalExit)
			GameState.instance.stopRun(true);
		else
			GameState.instance.switchLevel(destination, otherDoor.position);
		//Audio.PlayOrganic(openSound, new Vector3(position, 0));
		Audio.PlayBackground(openSound, 0.2f);
	}

	public void onFocusEnter(Player player)
	{
		outline = OUTLINE_COLOR;
	}

	public void onFocusLeft(Player player)
	{
		outline = 0;
	}

	public float getRange() => 2;

	public override void render()
	{
		Renderer.DrawSprite(position.x + rect.position.x, position.y + rect.position.y, LAYER_BG, rect.size.x, rect.size.y, 0, sprite, false, 0xFFFFFFFF);

		if (outline != 0)
			Renderer.DrawOutline(position.x + rect.position.x, position.y + rect.position.y, LAYER_BGBG, rect.size.x, rect.size.y, 0, sprite, false, outline);
	}
}
