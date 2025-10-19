using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LeatherGauntlets : Armor
{
	public unsafe LeatherGauntlets()
		: base(ArmorType.Gloves, "leather_gauntlets", "Leather Gauntlets")
	{
		model = Resource.GetModel("item/leather_gauntlets/leather_gauntlets.gltf");

		hidesHands = true;
	}
}
