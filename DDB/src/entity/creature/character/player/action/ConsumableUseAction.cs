using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class ConsumableUseAction : Action
{
	ItemSlot slot;
	Item item;
	int handID;

	bool wasUsed = false;


	public ConsumableUseAction(ItemSlot slot, int handID)
		: base(ActionType.ConsumableUse, "consumable_use")
	{
		this.slot = slot;
		this.item = slot.item;
		this.handID = handID;

		animationName[handID] = "consumable_use";
		animationSet[handID] = item.moveset;
		mirrorAnimation = handID == 1;

		overrideHandModels[handID] = true;
		handItemModels[handID] = item.model;
	}

	public override void onStarted(Player player)
	{
		base.onStarted(player);

		if (slot.stackSize == 0)
			startTime = 0;
	}

	public override void update(Player player)
	{
		base.update(player);

		if (elapsedTime >= item.consumableUseTime && !wasUsed)
		{
			if (item.consumableThrowable)
			{
				player.level.addEntity(new Throwable(item, player, player.lookDirection, player.getWeaponTransform(handID).translation - player.lookOrigin), player.lookOrigin, player.lookRotation);
				overrideHandModels[handID] = false;
			}
			if (item.consumableHealAmount > 0)
			{
				player.stats.addEffect(new HealEffect(item.consumableHealAmount, item.consumableHealDuration));
			}

			player.inventory.removeItem(slot);

			wasUsed = true;
		}
	}
}
