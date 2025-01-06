using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Backpack : Item
{
	int bonusActiveSlots = 2;
	int bonusStorageSlots = 3;


	public Backpack()
		: base("backpack", ItemType.Armor)
	{
		displayName = "Backpack";
		armorSlot = ArmorSlot.Back;

		description = "Increases inventory space";

		baseArmor = 1;
		value = 20;

		sprite = new Sprite(tileset, 3, 4);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/backpack.png", false), 0, 0, 16, 16);
	}

	public override void onEquip(Player player)
	{
		Item[] newStorage = new Item[player.activeItems.Length + bonusActiveSlots];
		Array.Copy(player.activeItems, newStorage, player.activeItems.Length);
		player.activeItems = newStorage;
		player.storeCapacity += bonusStorageSlots;
	}

	public override void onUnequip(Player player)
	{
		Item[] newStorage = new Item[player.activeItems.Length - bonusActiveSlots];
		Array.Copy(player.activeItems, newStorage, newStorage.Length);
		for (int i = 0; i < bonusActiveSlots; i++)
		{
			Item item = player.activeItems[player.activeItems.Length - 1 - i];
			if (item != null)
			{
				player.throwItem(item, new Vector2(player.direction, 1) * 3);
				player.removeItem(item);
			}
		}
		player.activeItems = newStorage;
		player.storeCapacity -= bonusStorageSlots;
		for (int i = player.storeCapacity; i < player.storedItems.Count; i++)
		{
			player.dropItem(player.storedItems[i]);
			i--;
		}
	}
}
