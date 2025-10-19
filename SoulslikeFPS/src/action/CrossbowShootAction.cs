using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class CrossbowShootAction : FirstPersonAction
{
	Crossbow weapon;

	public CrossbowShootAction(Crossbow weapon, int hand)
		: base("crossbow_shoot", hand)
	{
		this.weapon = weapon;

		animationName[hand] = "shoot";
		animationSet[hand] = weapon.moveset;

		animationName[hand ^ 1] = "shoot";
		animationSet[hand ^ 1] = weapon.moveset;

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
