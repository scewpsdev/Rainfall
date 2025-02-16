using Rainfall;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Cart : Entity
{
	Vector3[] tireLocalPositions;

	public Cart()
	{
		load("cart.rfs");

		tireLocalPositions = [
			new Vector3(0.37931f, 0.138185f, 0.65926f),
			new Vector3(-0.37931f, 0.138185f, 0.65926f),
			new Vector3(0.37931f, 0.138185f, -0.537267f),
			new Vector3(-0.37931f, 0.138185f, -0.537267f)
		];
	}

	public override void init()
	{
		base.init();

		body.setCenterOfMass(Vector3.Zero);
	}

	void updateTire(int i)
	{
		const float suspensionRestPosition = 0.36f;
		const float springStrength = 100;
		const float springDamper = 10;

		Matrix tireTransform = getModelMatrix() * Matrix.CreateTranslation(tireLocalPositions[i]);
		HitData? hit = Physics.Raycast(tireTransform.translation + 0.5f * tireTransform.rotation.up, tireTransform.rotation.down, 1, QueryFilterFlags.Static);
		if (hit != null)
		{
			Vector3 springDir = tireTransform.rotation.up;
			Vector3 tireVelocity = body.getPointVelocity(tireTransform.translation);
			float offset = suspensionRestPosition - (hit.Value.distance - 0.5f);
			float velocity = Vector3.Dot(springDir, tireVelocity);

			float force = (offset * springStrength) - (velocity * springDamper);
			body.addForceAtPosition(springDir * force * 0.9f, tireTransform.translation);
		}
	}

	public override void update()
	{
		base.update();

		for (int i = 0; i < tireLocalPositions.Length; i++)
			updateTire(i);
	}
}
