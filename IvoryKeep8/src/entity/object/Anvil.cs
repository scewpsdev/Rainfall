using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Anvil : Object
{
	bool open;
	Player player;

	int selectedItem = -1;
	List<Item> upgradeItems = new List<Item>();
	List<int> upgradePrices = new List<int>();

	Sound upgradeSound;


	public Anvil()
	{
		displayName = "Anvil";

		sprite = new Sprite(tileset, 1, 2);
		collider = new Hitbox(-0.5f, 0, 1, 10.0f / 16);
		platformCollider = true;
		filterGroup = FILTER_DEFAULT | FILTER_OBJECT;

		upgradeSound = Resource.GetSound("sounds/upgrade.ogg");

		damage = 4;
	}

	public override bool canInteract(Player player)
	{
		return base.canInteract(player) || player.items.Count > 0;
	}

	public override void interact(Player player)
	{
		if (player.isDucked)
			base.interact(player);
		else
		{
			this.player = player;
			openScreen();
		}
	}

	public float getRange()
	{
		return 1.5f;
	}

	void openScreen()
	{
		open = true;
		player.numOverlaysOpen++;

		selectedItem = 0;
		upgradeItems.Clear();
		upgradePrices.Clear();
		for (int i = 0; i < player.items.Count; i++)
		{
			if (player.items[i].upgradable)
			{
				upgradeItems.Add(player.items[i]);
				upgradePrices.Add(player.items[i].upgradeCost);
			}
		}
	}

	void closeScreen()
	{
		open = false;
		player.numOverlaysOpen--;
	}

	public override void update()
	{
		float maxDistance = getRange();
		if (open && (InputManager.IsPressed("UIQuit") || (player.center - position).lengthSquared > maxDistance * maxDistance))
		{
			closeScreen();
		}
	}

	public override void render()
	{
		base.render();

		if (open)
		{
			Vector2 menuAnchor = GameState.instance.camera.worldToScreen(position + new Vector2(0, 2));

			Item upgradedItem = upgradeItems[selectedItem].copy();
			upgradedItem.upgrade();

			int choice = ItemSelector.Render(menuAnchor, "Reinforce", upgradeItems, upgradePrices, player.money, player, true, upgradedItem, true, out bool secondary, out bool closed, ref selectedItem);
			if (choice != -1)
			{
				if (upgradePrices[selectedItem] <= player.money)
				{
					Item item = upgradeItems[choice];
					item.upgrade();
					player.money -= upgradePrices[choice];
					upgradePrices[choice] = item.upgradeCost;
					Audio.Play(upgradeSound, new Vector3(position, 0));
				}
			}
			else if (closed)
			{
				closeScreen();
			}
		}
	}
}
