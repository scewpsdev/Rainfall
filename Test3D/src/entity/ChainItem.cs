using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ChainItem : Item
{
	public ChainItem()
	{
		model = Resource.GetModel("chain_item.gltf");
	}

	protected override void onCollect(Cart cart)
	{
		cart.hasChain = true;
	}
}
