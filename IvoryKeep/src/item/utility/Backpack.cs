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
	int bonusActiveSlots = 3;
	bool equipped = false;
	int increaseStorageAmount = 0;


	public Backpack()
		: base("backpack", ItemType.Armor)
	{
		displayName = "Backpack";
		armorSlot = ArmorSlot.Back;

		description = "Increases inventory space";

		baseArmor = 1;
		baseValue = 20;

		sprite = new Sprite(tileset, 3, 4);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/backpack.png", false), 0, 0, 32, 32);
	}

	public override void upgrade()
	{
		base.upgrade();
		bonusActiveSlots++;
		if (equipped) increaseStorageAmount++;
	}

	void resizeStorage(Player player, int amount)
	{
		if (amount < player.activeItems.Length)
		{
			for (int i = amount; i < player.activeItems.Length; i++)
			{
				Item item = player.activeItems[i];
				if (item != null)
					player.dropItem(item);
			}
		}
		Item[] newStorage = new Item[amount];
		Array.Copy(player.activeItems, newStorage, Math.Min(player.activeItems.Length, amount));
		player.activeItems = newStorage;
	}

	public override void onEquip(Player player)
	{
		resizeStorage(player, player.activeItems.Length + bonusActiveSlots);
		equipped = true;
	}

	public override void onUnequip(Player player)
	{
		resizeStorage(player, player.activeItems.Length - bonusActiveSlots);
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
				resizeStorage(player, player.activeItems.Length + increaseStorageAmount);
				increaseStorageAmount = 0;
			}
		}
	}
}
