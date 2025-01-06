using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LargeWizardHat : Item
{
	public LargeWizardHat()
		: base("large_wizard_hat", ItemType.Armor)
	{
		displayName = "Large Wizard Hat";

		baseArmor = 1;
		armorSlot = ArmorSlot.Helmet;
		baseWeight = 0.3f;

		value = 13;

		sprite = new Sprite(tileset, 9, 4);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/wizards_hat.png", false), 0, 0, 32, 32);
		ingameSpriteSize = 2;

		buff = new ItemBuff(this) { manaRecoveryModifier = 2 };
	}

	public override void onEquip(Player player)
	{
		player.itemBuffs.Add(buff);
	}

	public override void onUnequip(Player player)
	{
		player.itemBuffs.Remove(buff);
	}
}
