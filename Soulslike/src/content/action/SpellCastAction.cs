using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SpellCastAction : PlayerAction
{
	Staff weapon;

	bool casted = false;

	public SpellCastAction(Staff weapon, int hand)
		: base("spell_cast", hand)
	{
		this.weapon = weapon;

		animationName[hand] = "cast";
		animationSet[hand] = weapon.moveset;

		viewmodelAim[hand] = 0.5f;
	}

	public override void update(Player player)
	{
		base.update(player);

		if (!casted && elapsedTime >= 19 / 24.0f)
		{
			casted = true;

			Vector3 origin = player.rightWeaponTransform * weapon.castOrigin;
			Vector3 offset = origin - player.camera.position;

			GameState.instance.scene.addEntity(new SpellProjectile(offset), player.camera.position, Quaternion.LookAt(player.camera.rotation.forward));
		}
	}
}
