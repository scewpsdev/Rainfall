using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HealAction : PlayerAction
{
	public HealAction(int hand)
		: base("heal", hand)
	{
		animationName[hand] = "use";
		animationSet[hand] = Resource.GetModel("item/healing_potion/healing_potion_moveset.gltf");
		overrideWeaponModel[hand] = true;
		weaponModel[hand] = Resource.GetModel("item/healing_potion/healing_potion.gltf");

		viewmodelAim[hand] = 1;
	}
}
