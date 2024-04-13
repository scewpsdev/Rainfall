using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class DoorOpenAction : InteractAction
{
	public DoorOpenAction(Door door)
		: base(door, ActionType.DoorOpen, "door_open")
	{
		overrideHandModels[0] = true;
		overrideHandModels[1] = true;
	}
}
