using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class IronHelmet : Armor
{
	public IronHelmet()
		: base(ArmorSlot.Head, "iron_helmet", "Iron Helmet")
	{
		model = Resource.GetModel("item/armor/iron_helmet/iron_helmet.gltf");
	}
}
