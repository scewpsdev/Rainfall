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
	public bool locked = false;
	public bool finalExit = false;

	public Sprite sprite;
	public FloatRect rect;
	protected uint outline = 0;
	protected float layer;

	protected Sound openSound;


	public Door(Level destination, Door otherDoor = null, bool big = false, float layer = 0)
	{
		this.destination = destination;
		this.otherDoor = otherDoor;
		this.layer = layer;

		sprite = big ? new Sprite(tileset, 0, 9, 2, 2) : new Sprite(tileset, 2, 2);
		rect = big ? new FloatRect(-1.0f, 0.0f, 2.0f, 2.0f) : new FloatRect(-0.5f, 0.0f, 1.0f, 1.0f);

		collider = new FloatRect(-0.4f, 0.0f, 0.8f, 1);

		openSound = Resource.GetSound("sounds/chest_close.ogg");
	}

	public virtual Vector2 getSpawnPoint()
	{
		return position;
	}

	public virtual bool isInteractable(Player player)
	{
		return destination != null && otherDoor != null && !locked || finalExit;
	}

	public virtual void interact(Player player)
	{
		// unlock from the other side
		otherDoor.locked = false;

		if (finalExit)
			GameState.instance.stopRun(true);
		else
			GameState.instance.switchLevel(destination, otherDoor.getSpawnPoint());

		if (otherDoor is LevelTransition)
		{
			LevelTransition transition = otherDoor as LevelTransition;
			if (transition.direction.x != 0)
				player.direction = -(otherDoor as LevelTransition).direction.x;
			if (transition.direction == Vector2i.Down)
				player.velocity.y = 20;
		}

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
