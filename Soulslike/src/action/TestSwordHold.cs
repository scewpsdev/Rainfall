using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TestSwordHold : PlayerAction
{
	public TestSwordHold()
		: base("sword_hold")
	{
		animationName[0] = "idle";
		animationName[1] = "idle";
		animationSet[0] = Resource.GetModel("item/longsword/longsword_moveset.gltf");
		animationSet[1] = Resource.GetModel("item/longsword/longsword_moveset.gltf");
		overrideWeaponModel[0] = true;
		weaponModel[0] = Resource.GetModel("item/longsword/longsword.gltf");

		duration = 5;
	}
}
