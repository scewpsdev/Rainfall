using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class ConsumableUseAction : Action
{
	ItemSlot slot;
	public readonly Item item;
	int handID;

	bool followUp = false;
	bool wasUsed = false;


	public ConsumableUseAction(ItemSlot slot, int handID, bool followUp)
		: base(ActionType.ConsumableUse)
	{
		this.slot = slot;
		this.item = slot.item;
		this.handID = handID;
		this.followUp = followUp;

		animationName[handID] = "consumable_use";
		animationSet[handID] = item.moveset;
		mirrorAnimation = handID == 1;

		overrideHandModels[handID] = true;
		handItemModels[handID] = item.model;

		followUpCancelTime = item.consumableFollowUpTime;

		if (item.sfxUse != null)
			addSoundEffect(item.sfxUse, handID, item.consumableUseTime, true);
	}

	public override void onStarted(Player player)
	{
		base.onStarted(player);

		if (followUp)
			elapsedTime = item.consumableFollowUpStart;

		//if (slot.stackSize == 0)
		//	startTime = 0;
	}

	public override void update(Player player)
	{
		base.update(player);

		if (elapsedTime >= item.consumableUseTime && !wasUsed)
		{
			if (item.consumableThrowable)
			{
				DungeonGame.instance.level.addEntity(new Firebomb(item, player, player.lookDirection, player.getWeaponTransform(handID).translation - player.lookOrigin), player.lookOrigin, player.lookRotation);
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
