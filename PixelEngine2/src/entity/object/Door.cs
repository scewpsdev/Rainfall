using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum DoorSprite
{
	Small,
	Big,
	Invisible,
}

public class Door : Entity, Interactable
{
	public Level destination;
	public Door otherDoor;
	public bool finalExit = false;

	Sprite sprite;
	FloatRect rect;
	uint outline = 0;
	float layer;

	Sound openSound;


	public Door(Level destination, Door otherDoor = null, bool big = false, float layer = 0)
	{
		this.destination = destination;
		this.otherDoor = otherDoor;
		this.layer = layer;

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

	public float getRange() => 1;

	public override void render()
	{
		Vector3 vertex = ParallaxObject.ParallaxEffect(position, layer);

		Renderer.DrawSprite(vertex.x + rect.position.x, vertex.y + rect.position.y, layer == 0 ? LAYER_BG : vertex.z, rect.size.x, rect.size.y, 0, sprite, false, 0xFFFFFFFF);

		if (outline != 0)
			Renderer.DrawOutline(vertex.x + rect.position.x, vertex.y + rect.position.y, (layer == 0 ? LAYER_BG : vertex.z) + 0.001f, rect.size.x, rect.size.y, 0, sprite, false, outline);
	}
}
