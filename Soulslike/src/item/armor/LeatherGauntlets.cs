using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LeatherGauntlets : Armor
{
	public LeatherGauntlets()
		: base(ArmorSlot.Hands, "leather_gauntlets", "Leather Gauntlets")
	{
		model = Resource.GetModel("item/armor/leather_gauntlets/leather_gauntlets.gltf");

		hidesHands = true;
	}
}
