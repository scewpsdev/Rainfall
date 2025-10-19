using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LostSigil : Item
{
	public LostSigil()
		: base("lost_sigil", ItemType.Relic)
	{
		displayName = "Victory Crown";
		description = "Congratulations on beating Cave Diver!";

		value = 100;
		canDrop = false;
		tumbles = false;

		armorSlot = ArmorSlot.Helmet;
		baseArmor = 2;
		baseWeight = 1;

		sprite = new Sprite(tileset, 13, 8);
		ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/lost_sigil.png", false), 0, 0, 32, 32);
	}
}
