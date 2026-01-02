using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ScrollOfWeaponEnchantment : Item
{
	public ScrollOfWeaponEnchantment()
		: base("scroll_enchant_weapon", ItemType.Scroll)
	{
		displayName = "Scroll of Weapon Enchantment";

		baseValue = 35;
		rarity = 4;

		sprite = new Sprite(tileset, 4, 10);
		spellIcon = new Sprite(tileset, 15, 2);
	}

	public override bool use(Player player)
	{
		if (player.handItem != null)
		{
			player.handItem.onUnequip(player);
			player.handItem.upgrade();
			player.handItem.onEquip(player);
			player.hud.showMessage("Your weapon shimmers lightly.");
			Audio.PlayOrganic(heal, new Vector3(player.position, 0));
		}
		else if (player.offhandItem != null)
		{
			player.offhandItem.onUnequip(player);
			player.offhandItem.upgrade();
			player.offhandItem.onEquip(player);
			player.hud.showMessage("Your weapon shimmers lightly.");
			Audio.PlayOrganic(heal, new Vector3(player.position, 0));
		}
		else
		{
			player.hud.showMessage("The scroll was lost without use.");
		}

		player.level.addEntity(ParticleEffects.CreateScrollUseEffect(player), player.position + player.collider.center);

		return true;
	}
}
