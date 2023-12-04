using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class TorchRaiseAction : Action
{
	public readonly int handID;
	public readonly Item item;


	public TorchRaiseAction(Item item, int handID, bool twoHanded)
		: base(ActionType.TorchRaise)
	{
		this.handID = handID;
		this.item = item;

		if (twoHanded)
		{
			animationName[0] = "torch_raise";
			animationName[1] = "torch_raise";
			animationSet[0] = item.moveset;
			animationSet[1] = item.moveset;
		}
		else
		{
			animationName[handID] = "torch_raise";
			animationSet[handID] = item.moveset;
		}

		mirrorAnimation = handID == 1;

		duration = 1000.0f;
	}
}
