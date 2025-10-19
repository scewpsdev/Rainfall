using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ElevatorRoom : Room
{
	Elevator elevator;

	float elevatorTop = 7;
	float elevatorBottom = 0;
	float elevatorSpeed = 1;
	float elevatorCooldown = 4;

	float elevatorHeight = 0;
	float elevatorDst = 7;
	long lastElevatorStop = -1;


	public ElevatorRoom()
	{
	}

	public override void init()
	{
		base.init();

		scene.addEntity(elevator = new Elevator(), getModelMatrix() * Matrix.CreateTranslation(0.5f, 0, 0.5f));
		scene.addEntity(new ElevatorShaft(), getModelMatrix() * Matrix.CreateTranslation(0.5f, 0, 0.5f));
	}

	public override void fixedUpdate(float delta)
	{
		base.fixedUpdate(delta);

		if (lastElevatorStop == -1)
		{
			if (elevatorHeight < elevatorDst)
			{
				elevatorHeight += elevatorSpeed * delta;
				if (elevatorHeight >= elevatorDst)
				{
					elevatorHeight = elevatorDst;
					lastElevatorStop = Time.currentTime;
					elevatorDst = elevatorBottom;
				}
			}
			else if (elevatorHeight > elevatorDst)
			{
				elevatorHeight -= elevatorSpeed * delta;
				if (elevatorHeight <= elevatorDst)
				{
					elevatorHeight = elevatorDst;
					lastElevatorStop = Time.currentTime;
					elevatorDst = elevatorTop;
				}
			}
		}
		else
		{
			float stopTime = (Time.currentTime - lastElevatorStop) / 1e9f;
			if (stopTime >= elevatorCooldown)
			{
				lastElevatorStop = -1;
			}
		}

		elevator.position.y = position.y + elevatorHeight;
	}
}
