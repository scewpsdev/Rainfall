﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ScrollOfArmorEnchantment : Item
{
	public ScrollOfArmorEnchantment()
		: base("scroll_enchant_armor", ItemType.Scroll)
	{
		displayName = "Scroll of Armor Enchantment";

		value = 22;

		sprite = new Sprite(tileset, 4, 10);
		spellIcon = new Sprite(tileset, 14, 2);
	}

	public override bool use(Player player)
	{
		bool wasUsed = false;
		for (int i = 0; i < player.passiveItems.Count; i++)
		{
			if (player.passiveItems[i].armor > 0)
			{
				player.passiveItems[i].onUnequip(player);
				player.passiveItems[i].upgrade();
				player.passiveItems[i].onEquip(player);
				wasUsed = true;
			}
		}
		if (wasUsed)
			player.hud.showMessage("Your armor shimmers lightly.");
		else
			player.hud.showMessage("The scroll was lost without use.");

		player.level.addEntity(ParticleEffects.CreateScrollUseEffect(player), player.position + player.collider.center);

		return true;
	}
}
