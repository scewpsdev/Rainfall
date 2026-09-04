using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class QuestlineLoganStaff : Item
{
	public QuestlineLoganStaff()
		: base("questline_logan_staff", ItemType.Relic)
	{
		displayName = "Lost Staff of the King's Scribe";
		description = "Once the symbol of a prosperous reign, now a tarnished relic of a descent into darkness.";

		baseValue = 1;
		canDrop = false;

		//armorSlot = ArmorSlot.Helmet;
		//baseArmor = 1;
		//baseWeight = 1;

		//sprite = new Sprite(tileset, 2, 6);
		//renderOffset.x = 0.2f;

		sprite = new Sprite(tileset, 13, 11);
		//ingameSprite = new Sprite(Resource.GetTexture("sprites/items/armor/lost_sigil.png", false), 0, 0, 32, 32);
	}
}
