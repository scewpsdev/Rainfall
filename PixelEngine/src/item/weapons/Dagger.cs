using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Dagger : Item
{
	public Dagger()
		: base("dagger", ItemType.Weapon)
	{
		displayName = "Dagger";

		baseDamage = 1;
		baseAttackRange = 1.0f;
		baseAttackRate = 3;

		projectileItem = true;
		projectileSticks = true;
		//projectileAims = true;
		projectileSpins = true;
		isSecondaryItem = true;
		baseWeight = 1;

		value = 4;

		sprite = new Sprite(tileset, 8, 6);
		renderOffset.x = 0.2f;

		useSound = Resource.GetSounds("res/sounds/swing_dagger", 6);

		buff = new ItemBuff() { criticalChanceModifier = 2 };
	}

	public override void onEquip(Player player)
	{
		player.itemBuffs.Add(buff);
	}

	public override void onUnequip(Player player)
	{
		player.itemBuffs.Remove(buff);
	}

	public override bool use(Player player)
	{
		player.actions.queueAction(new AttackAction(this, player.handItem == this));
		return false;
	}

	public override bool useSecondary(Player player)
	{
		ItemEntity entity = player.throwItem(this, player.lookDirection.normalized, 20);
		entity.rotationVelocity = -MathF.PI * 5;
		return true;
	}

	public override void upgrade()
	{
		base.upgrade();
		buff.criticalChanceModifier *= 1.2f;
	}
}
