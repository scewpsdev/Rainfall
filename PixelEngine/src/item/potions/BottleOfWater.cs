using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BottleOfWater : Item
{
	const float COOLDOWN_TIME = 20;

	public bool boiling = false;

	long startTime = -1;

	public BottleOfWater()
		: base("bottle_of_water", ItemType.Potion)
	{
		displayName = "Bottle of Water";
		stackable = true;
		value = 3;
		canDrop = false;

		sprite = new Sprite(tileset, 4, 5);
	}

	public override bool use(Player player)
	{
		if (boiling)
		{
			player.hit(Random.Shared.NextSingle() * 2, null, this, "Boiling Water");
			player.hud.showMessage("The water is scalding hot.");
		}
		else
		{
			player.hud.showMessage("It tastes bland.");
		}
		player.removeItemSingle(this);
		player.giveItem(new GlassBottle());
		return false;
	}

	public override void update(Entity entity)
	{
		if (startTime == -1)
			startTime = Time.currentTime;

		if (boiling && (Time.currentTime - startTime) / 1e9f > COOLDOWN_TIME)
		{
			boiling = false;
			displayName = "Distilled water";
		}
	}
}
