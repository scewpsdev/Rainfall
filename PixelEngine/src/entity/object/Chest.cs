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

public class Chest : Container
{
	ChestType type;
	bool locked;

	bool open = false;

	Sprite closedSprite, openSprite;
	bool flipped;

	Sound[] openSound;
	Sound closeSound;
	Sound unlockSound;
	Sound lockedSound;


	public Chest(Item[] items, bool flipped = false, ChestType type = ChestType.Normal)
		: base(items)
	{
		this.type = type;

		locked = type != ChestType.Normal;

		closedSprite = type == ChestType.Normal ? new Sprite(tileset, 0, 0) :
			type == ChestType.Red ? new Sprite(tileset, 2, 0) :
			type == ChestType.Blue ? new Sprite(tileset, 4, 0) :
			type == ChestType.Green ? new Sprite(tileset, 6, 0) :
			type == ChestType.Silver ? new Sprite(tileset, 8, 0) : null;
		openSprite = type == ChestType.Normal ? new Sprite(tileset, 1, 0) :
			type == ChestType.Red ? new Sprite(tileset, 3, 0) :
			type == ChestType.Blue ? new Sprite(tileset, 5, 0) :
			type == ChestType.Green ? new Sprite(tileset, 7, 0) :
			type == ChestType.Silver ? new Sprite(tileset, 9, 0) : null;
		sprite = closedSprite;

		collider = new FloatRect(-0.25f, 0.0f, 0.5f, 5 / 16.0f);
		platformCollider = true;
		tumbles = false;
		numRestRotations = 2;

		health = locked ? 5 : 3;

		this.flipped = flipped;

		hitSound = Item.woodHit;
		breakSound = [Resource.GetSound("sounds/break_wood.ogg")];

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

	public override bool canInteract(Player player)
	{
		return !open;
	}

	public override void interact(Player player)
	{
		if (player.isDucked)
			base.interact(player);
		else
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
			sprite = openSprite;
			GameState.instance.run.chestsOpened++;

			Debug.Assert(items != null);
			dropItems();

			Audio.Play(openSound, new Vector3(position, 0));
		}
	}

	protected override void breakContainer()
	{
		if (locked)
		{
			float itemLostChance = 0.8f;
			if (Random.Shared.NextSingle() < itemLostChance)
				items = null;
		}
		base.breakContainer();
	}
}
