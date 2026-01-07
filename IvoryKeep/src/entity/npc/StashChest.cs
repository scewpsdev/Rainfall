using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum StashChestMode
{
	None,
	Store,
	Retrieve,
}

public class StashChest : Object, Interactable
{
	StashChestMode mode;

	Sprite closedSprite, openSprite;

	bool open = true;
	bool menuOpen = false;
	int selectedItem = 0;


	public StashChest(StashChestMode mode)
	{
		this.mode = mode;

		displayName = "Stash";

		closedSprite = new Sprite(tileset, 10, 0);
		openSprite = new Sprite(tileset, 11, 0);
		sprite = openSprite;

		collider = new Hitbox(-0.5f, 0, 1, 10.0f / 16);
		platformCollider = true;
		filterGroup = FILTER_OBJECT;

		open = GameState.instance.currentlyStashedItem == null;
	}

	public override void init(Level level)
	{
		level.addEntityCollider(this);
	}

	public override void destroy()
	{
		level.removeEntityCollider(this);
	}

	void openScreen()
	{
		menuOpen = true;
		GameState.instance.player.numOverlaysOpen++;
		selectedItem = 0;
	}

	void closeScreen()
	{
		mode = StashChestMode.None;
		menuOpen = false;
		GameState.instance.player.numOverlaysOpen--;
	}

	public override bool canInteract(Player player)
	{
		return open;
	}

	public override void interact(Player player)
	{
		if (!menuOpen)
		{
			openScreen();
		}
	}

	public float getRange()
	{
		return 2;
	}

	public override void update()
	{
		base.update();

		if (menuOpen)
		{
			Player player = GameState.instance.player;
			float maxDistance = getRange();
			if ((player.position + player.collider.center - position).lengthSquared > maxDistance * maxDistance)
			{
				closeScreen();
			}
		}
	}

	public override void render()
	{
		base.render();

		if (menuOpen)
		{
			if (mode == StashChestMode.None)
			{
				Vector2 menuAnchor = GameState.instance.camera.worldToScreen(position + new Vector2(0, 2));
				List<string> options = ["Store Item", "Retrieve Item"];
				int choice = InteractableMenu.Render(menuAnchor, "Stash", options, out bool closed, ref selectedItem);

				if (choice != -1)
				{
					mode = choice == 0 ? StashChestMode.Store : StashChestMode.Retrieve;
					selectedItem = 0;
				}
				else if (closed)
				{
					menuOpen = false;
					GameState.instance.player.numOverlaysOpen--;
				}
			}
			else if (mode == StashChestMode.Store)
			{
				Vector2 menuAnchor = GameState.instance.camera.worldToScreen(position + new Vector2(0, 2));
				int choice = ItemSelector.Render(menuAnchor, "Stash item", GameState.instance.player.items, null, 0, GameState.instance.player, true, null, false, out bool secondary, out bool closed, ref selectedItem);
				if (choice != -1)
				{
					Item selected = GameState.instance.player.items[choice];
					GameState.instance.currentlyStashedItem = selected;
					GameState.instance.save.stashedItems.Add(selected);
					GameState.instance.player.removeItem(selected);

					open = false;
					sprite = closedSprite;

					closeScreen();
				}
				else if (closed)
				{
					mode = StashChestMode.None;
				}
			}
			else
			{
				Vector2 menuAnchor = GameState.instance.camera.worldToScreen(position + new Vector2(0, 2));
				int choice = ItemSelector.Render(menuAnchor, "Retrieve item", GameState.instance.save.stashedItems, null, 0, null, true, null, false, out bool secondary, out bool closed, ref selectedItem);
				if (choice != -1)
				{
					Item selected = GameState.instance.save.stashedItems[choice];
					GameState.instance.save.stashedItems.Remove(selected);
					GameState.instance.player.giveItem(selected);

					open = false;
					sprite = closedSprite;

					closeScreen();
				}
				else if (closed)
				{
					mode = StashChestMode.None;
				}
			}
		}
	}
}
