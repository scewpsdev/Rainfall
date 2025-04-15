using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Torch : Entity
{
	Vector3 lightColor;
	Simplex simplex;


	public override void init()
	{
		load("entity/object/torch/torch.rfs");
		lightColor = pointLights[0].color;

		simplex = new Simplex();
	}

	public override void update()
	{
		base.update();

		float lightIntensity = simplex.sample1f(GetHashCode() % 10 + Time.currentTime / 1e9f);
		pointLights[0].color = lightColor * (1 + lightIntensity * 0.2f);
	}
}
