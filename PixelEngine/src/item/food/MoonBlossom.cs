using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MoonBlossom : Item
{
	public MoonBlossom()
		: base("moon_blossom", ItemType.Food)
	{
		displayName = "Moon Blossom";
		stackable = true;

		description = "Refills staff charges";

		value = 25;
		canDrop = false;

		sprite = new Sprite(tileset, 12, 4);

		useSound = [Resource.GetSound("sounds/eat.ogg")];
	}

	public override bool use(Player player)
	{
		if (player.handItem != null && player.handItem.type == ItemType.Staff)
		{
			base.use(player);
			player.handItem.staffCharges = (int)MathF.Ceiling(player.handItem.maxStaffCharges * (1 + 0.5f * upgradeLevel));
			GameState.instance.level.addEntity(ParticleEffects.CreateConsumableUseEffect(player, player.direction, 0xFF7eb79b), player.position);
			return true;
		}
		else if (player.offhandItem != null && player.offhandItem.type == ItemType.Staff)
		{
			base.use(player);
			player.offhandItem.staffCharges = (int)MathF.Ceiling(player.offhandItem.maxStaffCharges * (1 + 0.5f * upgradeLevel));
			GameState.instance.level.addEntity(ParticleEffects.CreateConsumableUseEffect(player, player.direction, 0xFF7eb79b), player.position);
			return true;
		}
		return false;
	}
}
