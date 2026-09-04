using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


public class Milk : Item
{
	public Milk()
		: base("milk", ItemType.Food)
	{
		displayName = "Milk";

		description = "Removes all negative status effects.";
		stackable = true;

		baseValue = 13;

		sprite = new Sprite(tileset, 8, 12);

		useSound = potionUse;

		/*
		addEffect(new ManaEffect(4, 30));

		displayName = "Blue Flask";
		description = "Boosts mana recovery speed for a short amount of time";

		stackable = true;
		canDrop = true;
		upgradable = true;

		value = 12;

		sprite = new Sprite(tileset, 6, 2);
		*/
	}

	public override bool use(Player player)
	{
		if (player.actions.actionQueue.Count <= 1)
		{
			player.actions.queueAction(new PotionDrinkAction(this));
			return false;
		}
		return false;
	}

	public override void applyEffect(Entity entity)
	{
		Player player = entity as Player;
		if (player != null)
		{
			for (int i = 0; i < player.statusEffects.Count; i++)
			{
				if (!player.statusEffects[i].positiveEffect)
				{
					player.removeStatusEffect(player.statusEffects[i--]);
				}
			}
		}
		player.addStatusEffect(new HealStatusEffect(1, 20));
	}
}
