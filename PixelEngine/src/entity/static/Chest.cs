using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum ChestType
{
	Normal,
	Red, // Weapons
	Blue, // Magic
	Green, // Armor
	Silver, // Anything
}

public class Chest : Entity, Interactable, Hittable
{
	ChestType type;
	bool locked;

	bool open = false;
	Item[] items;
	public int coins = 0;

	Sprite sprite;
	Sprite openSprite;
	uint outline = 0;
	bool flipped;

	Sound[] openSound;
	Sound closeSound;
	Sound unlockSound;
	Sound lockedSound;


	public Chest(Item[] items, bool flipped = false, ChestType type = ChestType.Normal)
	{
		this.items = items;
		this.type = type;

		locked = type != ChestType.Normal;

		sprite = type == ChestType.Normal ? new Sprite(tileset, 0, 0) :
			type == ChestType.Red ? new Sprite(tileset, 2, 0) :
			type == ChestType.Blue ? new Sprite(tileset, 4, 0) :
			type == ChestType.Green ? new Sprite(tileset, 6, 0) :
			type == ChestType.Silver ? new Sprite(tileset, 8, 0) : null;
		openSprite = type == ChestType.Normal ? new Sprite(tileset, 1, 0) :
			type == ChestType.Red ? new Sprite(tileset, 3, 0) :
			type == ChestType.Blue ? new Sprite(tileset, 5, 0) :
			type == ChestType.Green ? new Sprite(tileset, 7, 0) :
			type == ChestType.Silver ? new Sprite(tileset, 9, 0) : null;

		collider = new FloatRect(-0.25f, 0.0f, 0.5f, 5 / 16.0f);
		platformCollider = true;
		filterGroup = FILTER_DECORATION;

		this.flipped = flipped;

		openSound = [
			Resource.GetSound("sounds/chest_open1.ogg"),
			Resource.GetSound("sounds/chest_open2.ogg"),
		];
		closeSound = Resource.GetSound("sounds/chest_close.ogg");
		unlockSound = Resource.GetSound("sounds/door_unlock.ogg");
		lockedSound = Resource.GetSound("sounds/door_locked.ogg");
	}

	public Chest(params Item[] items)
		: this(items, false)
	{
	}

	public Chest()
		: this(null, false)
	{
	}

	public override void init(Level level)
	{
		level.addCollider(this);
	}

	public override void destroy()
	{
		level.removeCollider(this);
	}

	public bool hit(float damage, Entity by = null, Item item = null, string byName = null, bool triggerInvincibility = true, bool buffedHit = false)
	{
		if (by != null && byName == "Explosion")
		{
			float distance = (by.position - (position + collider.center)).length;
			if (distance < 1.7f)
			{
				if (distance > 1.1f && !open && items != null && !locked)
					dropItems();
				else if (distance <= 1.1f)
					remove();
				return true;
			}
		}
		return false;
	}

	public bool canInteract(Player player)
	{
		return !open;
	}

	public void onFocusEnter(Player player)
	{
		outline = OUTLINE_COLOR;
	}

	public void onFocusLeft(Player player)
	{
		outline = 0;
	}

	void dropItems()
	{
		for (int i = 0; i < items.Length; i++)
		{
			Vector2 itemVelocity = new Vector2(MathHelper.RandomFloat(-0.2f, 0.2f), 0.5f) * 8;
			Vector2 throwOrigin = position + new Vector2(0, 0.5f);
			ItemEntity obj = new ItemEntity(items[i], null, itemVelocity);
			GameState.instance.level.addEntity(obj, throwOrigin);
		}
		items = null;

		for (int i = 0; i < coins; i++)
		{
			Coin coin = new Coin();
			Vector2 spawnPosition = position + new Vector2(0, 0.5f) + Vector2.Rotate(Vector2.UnitX, i / (float)coins * 2 * MathF.PI) * 0.2f;
			coin.velocity = (spawnPosition - position - new Vector2(0, 0.5f)).normalized * 4;
			GameState.instance.level.addEntity(coin, spawnPosition);
		}
	}

	public void interact(Player player)
	{
		if (locked)
		{
			Item key = player.getItem("iron_key");
			if (key != null)
			{
				locked = false;
				player.removeItemSingle(key);
				Audio.PlayOrganic(unlockSound, new Vector3(position, 0));
			}
			else
			{
				player.hud.showMessage("It's locked.");
				Audio.PlayOrganic(lockedSound, new Vector3(position, 0));
				return;
			}
		}

		open = true;
		GameState.instance.run.chestsOpened++;

		Debug.Assert(items != null);
		dropItems();

		Audio.Play(openSound, new Vector3(position, 0));
	}

	public override void update()
	{
		TileType tile = GameState.instance.level.getTile(position - new Vector2(0, 0.01f));
		if (!(tile != null && (tile.isSolid || tile.isPlatform)))
		{
			velocity.y += -10 * Time.deltaTime;

			float displacement = velocity.y * Time.deltaTime;
			position.y += displacement;
		}
	}

	public override void render()
	{
		Renderer.DrawSprite(position.x - 0.5f, position.y, LAYER_BG, 1, 1, 0, open ? openSprite : sprite, flipped);

		if (outline != 0)
			Renderer.DrawOutline(position.x - 0.5f, position.y, LAYER_BG, 1, 1, 0, open ? openSprite : sprite, flipped, outline);
	}
}
