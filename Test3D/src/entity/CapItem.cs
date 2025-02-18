using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CapItem : Item
{
	public CapItem()
	{
		model = Resource.GetModel("cap_item.gltf");
	}

	protected override void onCollect(Cart cart)
	{
		cart.hasCap = true;
	}
}
