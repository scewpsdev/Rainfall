using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class IronDoor : Entity, Interactable
{
	Sprite sprite;
	Sprite frameSprite;
	uint outline = 0;

	Item key;
	bool open = false;
	float openProgress = 0.0f;

	Sound unlockSound;


	public IronDoor(Item key)
	{
		this.key = key;

		sprite = new Sprite(TileType.tileset, 2, 8);
		frameSprite = new Sprite(TileType.tileset, 2, 9);

		unlockSound = Resource.GetSound("res/sounds/door_unlock.ogg");
	}

	public IronDoor()
		: this(null)
	{
	}

	public override void init(Level level)
	{
		setOpen(false);
	}

	public void interact(Player player)
	{
		if (!open)
		{
			if (key != null)
			{
				bool hasKey = player.hasItem(key);
				Item lockpick = player.getItem("lockpick");
				if (hasKey || lockpick != null)
				{
					if (hasKey)
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
				}
				else
				{
					player.hud.showMessage("Locked");
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
		GameState.instance.level.setTile(tile.x, tile.y, open ? null : TileType.dummy);

		if (open)
		{
			key = null;
			Audio.PlayOrganic(unlockSound, new Vector3(position, 0));
		}
	}

	public override void update()
	{
		float openDst = open ? 1 : 0;
		openProgress = MathHelper.Lerp(openProgress, openDst, 10 * Time.deltaTime);
	}

	public override void render()
	{
		if (outline != 0)
			Renderer.DrawOutline(position.x - 0.5f + openProgress, position.y, LAYER_BG, openProgress, 1, 0, sprite, false, outline);

		Renderer.DrawSprite(position.x - 0.5f + openProgress, position.y, LAYER_BG, openProgress, 1, 0, sprite);
		Renderer.DrawSprite(position.x - 0.5f, position.y, 1, 1, frameSprite);
	}
}
