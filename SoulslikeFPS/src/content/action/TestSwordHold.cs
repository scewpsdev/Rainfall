using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TestSwordHold : FirstPersonAction
{
	public TestSwordHold(int hand)
		: base("sword_hold", hand)
	{
		animationName[hand] = "idle";
		animationName[hand ^ 1] = "idle";
		animationSet[hand] = Resource.GetModel("item/longsword/longsword_moveset.gltf");
		animationSet[hand ^ 1] = Resource.GetModel("item/longsword/longsword_moveset.gltf");
		overrideWeaponModel[hand] = true;
		weaponModel[hand] = Resource.GetModel("item/longsword/longsword.gltf");

		duration = 5;
	}
}
