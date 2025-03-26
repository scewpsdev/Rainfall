using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ThrowItemAction : PlayerAction
{
	public ThrowItemAction(int hand)
		: base("throw_item", hand)
	{
		animationName[hand] = "throw_item";

		followUpCancelTime = 10 / 24.0f;

		addActionEvent(4, (Player player) =>
		{
			ItemEntity entity = player.dropItem(player.getWeapon(hand));
			player.setWeapon(hand, null);

			player.animator.getNodeVelocity(player.rightWeaponNode, out Vector3 velocity, out Quaternion angularVelocity);
			entity.body.setVelocity(player.camera.rotation.forward * 5);
			entity.body.setAngularVelocity(MathHelper.RandomVector3(-1, 1) * 5);
		});

		addSoundEffect(new ActionSfx(Item.equipLight));
	}
}
