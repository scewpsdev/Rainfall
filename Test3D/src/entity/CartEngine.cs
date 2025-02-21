using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CartEngine : Entity
{
	public bool active = false;


	public CartEngine()
	{
		load("engine.rfs");
	}

	public override unsafe void update()
	{
		base.update();

		setTransform(GameState.instance.cart.getModelMatrix());
		particles[0].setTransform(GameState.instance.cart.getModelMatrix(), true);

		particles[0].handle->emissionRate = active ? 130 : 0;
	}
}
