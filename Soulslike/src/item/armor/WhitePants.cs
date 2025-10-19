using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WhitePants : Armor
{
	public WhitePants()
		: base(ArmorSlot.Body, "white_pants", "White Pants")
	{
		model = Resource.GetModel("item/armor/white_pants/white_pants.gltf");
	}
}
