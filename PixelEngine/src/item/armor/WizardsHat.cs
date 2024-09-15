using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WizardsHat : Item
{
	public WizardsHat()
		: base("wizards_hat", ItemType.Armor)
	{
		displayName = "Wizard's Hat";

		armor = 1;
		armorSlot = ArmorSlot.Helmet;

		value = 12;

		sprite = new Sprite(tileset, 9, 4);
		ingameSprite = new Sprite(Resource.GetTexture("res/sprites/wizards_hat.png", false), 0, 0, 32, 32);
		ingameSpriteSize = 2;
	}

	public override void onEquip(Player player)
	{
		player.manaRechargeRate *= 2;
		player.addStatusEffect(new ManaRechargeModifier());
	}

	public override void onUnequip(Player player)
	{
		player.manaRechargeRate /= 2;
		player.removeStatusEffect("mana_recharge_modifier");
	}
}
