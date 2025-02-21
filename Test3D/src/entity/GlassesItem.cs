using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GlassesItem : Item
{
	public GlassesItem()
	{
		model = Resource.GetModel("glasses_item.gltf");
	}

	protected override void onCollect(Player player)
	{
		GameState.instance.hasGlasses = true;
	}
}
