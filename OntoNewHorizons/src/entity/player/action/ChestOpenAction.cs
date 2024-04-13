using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class ChestOpenAction : InteractAction
{
	public ChestOpenAction(Chest chest)
		: base(chest, ActionType.ChestOpen, "chest_open")
	{
		overrideHandModels[0] = true;
		overrideHandModels[1] = true;
	}
}
