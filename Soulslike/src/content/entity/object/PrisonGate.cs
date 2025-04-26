using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PrisonGate : Entity, Activatable
{
	bool open = false;

	float height = 2.2f;
	float openDuration = 5;
	float closeDuration = 2;
	Vector3 closedPosition;
	Vector3 openPosition;

	float animProgress;


	public override void init()
	{
		base.init();

		load("entity/object/prison_gate/prison_gate.rfs", PhysicsFilter.Default);

		closedPosition = position;
		openPosition = position + height * Vector3.Up;
	}

	void setOpen(bool open)
	{
		this.open = open;
		animProgress = 0;
	}

	public void activate(Entity entity)
	{
		setOpen(!open);
	}

	public override void update()
	{
		base.update();

		if (animProgress != 1)
		{
			if (open)
			{
				animProgress += Time.deltaTime / openDuration;

				float t1 = 0.25f;
				float s1 = 0.15f;
				float anim = animProgress < t1 ? Easing.easeOutBack(animProgress / t1) * s1 : (s1 + Easing.easeOutBounce((animProgress - t1) / (1 - t1)) * (1 - s1));
				position = Vector3.Lerp(closedPosition, openPosition, anim);
			}
			else
			{
				animProgress += Time.deltaTime / closeDuration;

				float anim = Easing.easeOutBounce(animProgress);
				position = Vector3.Lerp(openPosition, closedPosition, anim);
			}

			body.setTransform(position, rotation);

			if (animProgress >= 1)
				animProgress = 1;
		}
	}
}
