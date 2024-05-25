using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class ItemEquipAction : Action
{
	public ItemEquipAction(Item item, int handID)
		: base("item_equip")
	{
		//if (item == null)
		//	item = Item.Get("default");

		if (item.twoHanded)
		{
			animationName[0] = "equip";
			animationName[1] = "equip";
			animationSet[0] = item.moveset;
			animationSet[1] = item.moveset;
		}
		else
		{
			animationName[handID] = "equip";
			animationSet[handID] = item.moveset;
		}

		mirrorAnimation = handID == 1;

		animationTransitionDuration = 0.0f;

		//if (item.equipSound != null)
		//	addSoundEffect(new ActionSfx(item.equipSound, 1.0f, 0.0f, false));
	}
}
