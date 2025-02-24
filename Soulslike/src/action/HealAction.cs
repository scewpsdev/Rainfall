using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HealAction : PlayerAction
{
	public HealAction()
		: base("heal")
	{
		animationName[0] = "use";
		animationSet[0] = Resource.GetModel("item/healing_potion/healing_potion_moveset.gltf");
		overrideWeaponModel[0] = true;
		weaponModel[0] = Resource.GetModel("item/healing_potion/healing_potion.gltf");

		viewmodelAim = 1;
	}
}
