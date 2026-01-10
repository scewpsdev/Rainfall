using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


public class Backpack : Item
{
	Item[] itemSlots = new Item[3];
	bool equipped = false;
	int increaseStorageAmount = 0;


	public Backpack()
		: base("backpack", ItemType.Armor)
	{
		displayName = "Backpack";
		armorSlot = ArmorSlot.Back;

		description = "Expands the bearer's capacity for quick-access items.";

		baseArmor = 1;
		baseValue = 20;

		sprite = new Sprite(tileset, 3, 4);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/backpack.png", false), 0, 0, 32, 32);
	}

	public override void upgrade()
	{
		base.upgrade();
		Array.Resize(ref itemSlots, itemSlots.Length + 1);
		if (equipped) increaseStorageAmount++;
	}

	void resizePlayerHotbar(Player player, int amount)
	{
		if (amount < player.activeItems.Length)
		{
			for (int i = amount; i < player.activeItems.Length; i++)
			{
				Item item = player.activeItems[i];
				if (item != null)
				{
					if (!player.removeItem(item))
					{
						player.items.Remove(item);
						player.activeItems[ArrayUtils.IndexOf(player.activeItems, item)] = null;
					}
				}
				itemSlots[i - amount] = item;
			}
		}
		Item[] newStorage = new Item[amount];
		Array.Copy(player.activeItems, newStorage, Math.Min(player.activeItems.Length, amount));
		player.activeItems = newStorage;
	}

	public override void onEquip(Player player)
	{
		resizePlayerHotbar(player, player.activeItems.Length + itemSlots.Length);
		for (int i = 0; i < itemSlots.Length; i++)
		{
			if (itemSlots[i] != null)
			{
				player.items.Add(itemSlots[i]);
				player.activeItems[player.activeItems.Length - itemSlots.Length + i] = itemSlots[i];
			}
		}
		Array.Fill(itemSlots, null);
		equipped = true;
	}

	public override void onUnequip(Player player)
	{
		resizePlayerHotbar(player, player.activeItems.Length - itemSlots.Length);
		equipped = false;
	}

	public override void update(Entity entity)
	{
		base.update(entity);

		Player player = entity as Player;
		if (player != null)
		{
			if (increaseStorageAmount != 0)
			{
				resizePlayerHotbar(player, player.activeItems.Length + increaseStorageAmount);
				increaseStorageAmount = 0;
			}
		}
	}
}
