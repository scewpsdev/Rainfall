using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class WizardsLegacy : Item
{
	public WizardsLegacy()
		: base("wizards_legacy", ItemType.Relic)
	{
		displayName = "Wizard's Legacy";
		description = "Increases mana gained from killing enemies";
		stackable = true;
		tumbles = false;
		//canDrop = false;

		baseValue = 27;
		maxUpgradeLevel = 3;

		sprite = new Sprite(tileset, 5, 6);

		buff = new ItemBuff(this) { manaRecoveryModifier = 1.5f };
	}

	public override void onKill(Player player, Mob mob)
	{
		base.onKill(player, mob);
		player.refillMana(/*Player.MANA_KILL_REWARD + */ 0.2f + upgradeLevel * 0.05f);
	}
}
