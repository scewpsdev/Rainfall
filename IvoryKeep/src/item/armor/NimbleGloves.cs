using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class NimbleGloves : Item
{
	public NimbleGloves()
		: base("nimble_gloves", ItemType.Armor)
	{
		displayName = "Nimble Gloves";
		description = "Increases attack rate by 15%";
		armorSlot = ArmorSlot.Gloves;

		baseArmor = 1;
		value = 35;

		sprite = new Sprite(tileset, 9, 9);
		ingameSprite = new Sprite("sprites/items/armor/nimble_gloves.png", 0, 0, 32, 32);
		ingameSpriteLayer = Entity.LAYER_PLAYER_GLOVE;

		buff = new ItemBuff(this) { attackSpeedModifier = 1.15f };

		gloveColor = 0xFF849be4;
	}
}
