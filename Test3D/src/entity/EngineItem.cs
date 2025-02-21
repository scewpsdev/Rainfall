using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EngineItem : Item
{
	public EngineItem()
	{
		model = Resource.GetModel("engine_item.gltf");
	}

	protected override void onCollect(Player player)
	{
		GameState.instance.hasEngine = true;
	}
}
