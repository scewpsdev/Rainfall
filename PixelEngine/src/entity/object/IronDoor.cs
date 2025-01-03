using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class IronDoor : Entity, Interactable, Hittable
{
	Sprite sprite;
	Sprite frameSprite;
	uint outline = 0;

	public string key;
	bool open = false;
	float openProgress = 0.0f;

	Sound unlockSound;
	Sound lockedSound;
	Sound closeSound;
	bool closeSoundPlayed;


	public IronDoor(string key)
	{
		this.key = key;

		sprite = key != null ? new Sprite(tileset, 2, 8) : new Sprite(tileset, 3, 8);
		frameSprite = key != null ? new Sprite(tileset, 2, 9) : new Sprite(tileset, 3, 9);

		collider = new FloatRect(-0.25f, 0.0f, 0.5f, 1);

		unlockSound = Resource.GetSound("sounds/door_unlock.ogg");
		lockedSound = Resource.GetSound("sounds/door_locked.ogg");
		closeSound = Resource.GetSound("sounds/door_close.ogg");
	}

	public IronDoor()
		: this(null)
	{
	}

	public override void init(Level level)
	{
		setOpen(false);
	}

	public override void destroy()
	{
		setOpen(true);
	}

	public void interact(Player player)
	{
		if (!open)
		{
			if (key != null)
			{
				Item key = player.getItem(this.key);
				Item lockpick = player.getItem("lockpick");
				if (key != null || lockpick != null)
				{
					if (key != null)
					{
						setOpen(true);
						player.removeItem(key);
						player.hud.showMessage("Used " + key.displayName);
					}
					else if (lockpick != null)
					{
						float succeedChance = 0.7f;
						if (Random.Shared.NextSingle() < succeedChance)
						{
							setOpen(true);
							player.hud.showMessage("Picked lock successfully");
						}
						else
						{
							player.hud.showMessage("The lockpick breaks");
						}
						player.removeItem(lockpick);
					}

					this.key = null;
				}
				else
				{
					player.hud.showMessage("Locked");
					Audio.Play(lockedSound, new Vector3(position, 0));
				}
			}
			else
			{
				setOpen(true);
			}
		}
		else
		{
			setOpen(false);
		}
	}

	public void onFocusEnter(Player player)
	{
		outline = OUTLINE_COLOR;
	}

	public void onFocusLeft(Player player)
	{
		outline = 0;
	}

	public void setOpen(bool open)
	{
		this.open = open;

		Vector2i tile = (Vector2i)Vector2.Floor(position + new Vector2(0, 0.5f));
		if (open)
			level.addCollider(this);
		else
			level.removeCollider(this);

		if (open)
			Audio.PlayOrganic(unlockSound, new Vector3(position, 0));
		else
		{
			HitData[] hits = new HitData[16];
			int numHits = level.overlap((Vector2)tile, (Vector2)tile + 1, hits, FILTER_DEFAULT | FILTER_ITEM | FILTER_PLAYER | FILTER_MOB | FILTER_PROJECTILE);
			for (int i = 0; i < numHits; i++)
			{
				Entity entity = hits[i].entity;
				if (entity is Player || entity is Mob || entity is ItemEntity)
				{
					Vector2 pos = entity.position;
					if (entity is Player)
						pos.x += (entity as Player).direction * 0.25f;
					if (MathHelper.Fract(pos.x) < 0.5f && (level.getTile(tile.x - 1, tile.y) == null || !level.getTile(tile.x - 1, tile.y).isSolid))
						entity.position.x = MathF.Min(entity.position.x, tile.x - entity.collider.max.x);
					else if (level.getTile(tile.x + 1, tile.y) == null || !level.getTile(tile.x + 1, tile.y).isSolid)
						entity.position.x = MathF.Max(entity.position.x, tile.x + 1 - entity.collider.min.x);
				}
			}

			closeSoundPlayed = false;
		}
	}

	public bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		if (item is Bomb)
		{
			remove();
			return true;
		}
		return false;
	}

	public override void update()
	{
		float openDst = open ? 1 : 0;
		openProgress = MathHelper.Lerp(openProgress, openDst, 10 * Time.deltaTime);

		if (MathF.Abs(openDst - openProgress) < 0.01f && !closeSoundPlayed)
		{
			Audio.PlayOrganic(closeSound, new Vector3(position, 0));
			closeSoundPlayed = true;
		}
	}

	public override void render()
	{
		if (outline != 0)
		{
			Renderer.DrawOutline(position.x - openProgress, position.y, LAYER_BG, openProgress, 1, 0, sprite, false, outline);
			Renderer.DrawOutline(position.x - 0.5f, position.y, 1, 1, frameSprite, false, outline);
		}

		Renderer.DrawSprite(position.x - openProgress, position.y, LAYER_BG, openProgress, 1, 0, sprite);
		Renderer.DrawSprite(position.x - 0.5f, position.y, 1, 1, frameSprite);
	}
}
