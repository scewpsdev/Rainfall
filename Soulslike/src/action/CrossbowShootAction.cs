using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CrossbowShootAction : PlayerAction
{
	Crossbow weapon;

	public CrossbowShootAction(Crossbow weapon)
		: base("crossbow_shoot")
	{
		this.weapon = weapon;

		animationName[0] = "shoot";
		animationSet[0] = weapon.moveset;
		animationName[1] = "shoot";
		animationSet[1] = weapon.moveset;

		viewmodelAim = 1;
		swayAmount = 0.1f;

		animationTransitionDuration = 0.05f;
	}

	public override void onStarted(Player player)
	{
		Vector3 origin = player.rightWeaponTransform * weapon.castOrigin;
		Vector3 offset = origin - player.camera.position;

		GameState.instance.scene.addEntity(new CrossbowBolt(offset), player.camera.position, Quaternion.LookAt(player.camera.rotation.forward));
	}
}
