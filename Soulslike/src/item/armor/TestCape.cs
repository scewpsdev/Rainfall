using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TestCape : Armor
{
	public TestCape()
		: base(ArmorSlot.Body, "test_cape", "Test Cape")
	{
		cloth = Resource.GetModel("item/armor/test_cape/test_cape.gltf");
	}
}
