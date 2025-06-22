using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Elevator : Entity
{
	public Elevator()
	{
		load("entity/object/elevator/elevator.rfs");
	}
}

public class ElevatorShaft : Entity
{
	public ElevatorShaft()
	{
		load("entity/object/elevator/elevator_shaft.rfs");
	}
}
