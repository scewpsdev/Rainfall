using Rainfall;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EquipAction : PlayerAction
{
	public EquipAction(Item item, int hand)
		: base("equip", hand)
	{
		animationName[hand] = "equip";
		animationSet[hand] = item.moveset;

		if (item.twoHanded)
		{
			animationName[hand ^ 1] = "equip";
			animationSet[hand ^ 1] = item.moveset;
		}

		mirrorAnimation = hand == 1;

		followUpCancelTime = 0;

		//viewmodelAim = 1;
		animationTransitionDuration = 0;

		if (item.equipSound != null)
			addSoundEffect(new ActionSfx(item.equipSound));
	}
}
