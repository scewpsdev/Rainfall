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
		spriteColor = 0xFF567850;
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/large_wizard_hat.png", false), 0, 0, 32, 32);
		ingameSpriteColor = 0xFF567850;

		buff = new ItemBuff(this) { manaRecoveryModifier = 2 };
	}
}
