using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ScrollOfProtection : Item
{
	SpectralShieldEntity shield1, shield2;


	public ScrollOfProtection()
		: base("scroll_of_protection", ItemType.Scroll)
	{
		displayName = "Scroll of Protection";
		description = "Summons spectral wards that orbit the caster.";

		baseValue = 29;

		sprite = new Sprite(tileset, 4, 10);
		spellIcon = new Sprite(tileset, 8, 11);
	}

	public override bool use(Player player)
	{
		player.level.addEntity(shield1 = new SpectralShieldEntity(null, player), player.position);
		player.level.addEntity(shield2 = new SpectralShieldEntity(null, player, MathF.PI), player.position);
		return true;
	}
}
