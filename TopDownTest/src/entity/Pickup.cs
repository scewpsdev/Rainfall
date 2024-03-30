using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


public abstract class Pickup : Entity, Toucheable, Interactable
{
	protected static SpriteSheet itemSprites = new SpriteSheet(Resource.GetTexture("res/item/items.png", false), 16, 16);

	protected Sprite sprite;
	Texture shadow;

	protected bool autoPickup = false;


	public Pickup(Vector2 position)
	{
		this.position = position;

		shadow = Resource.GetTexture("res/sprites/shadow.png", true);

		collider = new FloatRect(-0.25f, 0.0f, 0.5f, 1.0f);
		hitbox = new FloatRect(-0.25f, 0.0f, 0.5f, 1.0f);
	}

	protected abstract bool onPickup(Player player);

	public bool canInteract(Entity entity)
	{
		return !autoPickup;
	}

	public virtual void getInteractionPrompt(Entity entity, out string text, out uint color)
	{
		text = "[E] Pick up";
		color = 0xFFFFFFFF;
	}

	public void interact(Entity entity)
	{
		Player player = entity as Player;
		onPickup(player);
		player.audio.playSoundOrganic(player.sfxPowerup);
		removed = true;
	}

	public void touch(Entity entity)
	{
		if (autoPickup)
		{
			if (entity is Player)
			{
				Player player = entity as Player;

				if (onPickup(player))
				{
					player.audio.playSoundOrganic(player.sfxPowerup);
					removed = true;
				}
			}
		}
	}

	public override void draw()
	{
		float height = 1.0f + MathF.Sin(Hash.hash(position.x) % 4 + Time.currentTime / 1e9f * 2) * 0.5f;
		if (sprite != null)
		{
			Renderer.DrawVerticalSprite(position.x - 1.0f, position.y, height, 2.0f, 2.0f, sprite, false);
			Renderer.DrawSprite(position.x - 0.5f, position.y - 0.5f, 0.01f, 1, 1, shadow, 0, 0, 8, 8, 0xFFFFFFFF);
			Renderer.DrawLight(position + new Vector2(0.0f, height), new Vector3(1.0f, 0.8f, 0.6f) * 3, 4.0f);
		}
	}
}
