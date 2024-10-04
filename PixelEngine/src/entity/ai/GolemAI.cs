using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GolemAI : AdvancedAI
{
	const float dashChargeTime = 0.5f;
	const float dashCooldownTime = 1.0f;
	const float dashSpeed = 8.0f;
	const float jumpSpeed = 1.5f;
	const float jumpAttackSpeed = 4.0f;
	const float dashDistance = 3;
	const float dashTriggerDistance = 3;


	public GolemAI(Mob mob)
		: base(mob)
	{
		aggroRange = 6.0f;
		loseRange = 10.0f;
		loseTime = 3.0f;

		patrol = false;

		AIAction attack = addAction("attack", dashDistance / dashSpeed, dashChargeTime, dashCooldownTime, dashSpeed, (AIAction action, Vector2 toTarget, float targetDistance) => targetDistance < dashTriggerDistance && mob.ai.canSeeTarget);
		attack.onStarted = (AIAction action) =>
		{
			action.ai.mob.animator.getAnimation("attack").duration = action.duration;
		};
		attack.onFinished = (AIAction action) =>
		{
			TileType tile = GameState.instance.level.getTile(mob.position - new Vector2(0, 0.5f));
			if (tile != null)
				GameState.instance.level.addEntity(Effects.CreateImpactEffect(Vector2.Up, 6, 40, MathHelper.ARGBToVector(tile.particleColor).xyz), mob.position + mob.direction * Vector2.Right);
			GameState.instance.camera.addScreenShake(mob.position + mob.direction * Vector2.Right, 1, 3);
		};
		attack.actionCollider = new FloatRect(-0.5f, 0.0f, 1.0f, 1.0f);

		AIAction jumpAttack = addAction("jump", 100, 0, 0, jumpAttackSpeed, (AIAction action, Vector2 toTarget, float targetDistance) => targetDistance < dashTriggerDistance && mob.ai.canSeeTarget);
		jumpAttack.onStarted = (AIAction action) =>
		{
			mob.inputJump = true;
		};
		jumpAttack.onAction = (AIAction action, float elapsed, Vector2 toTarget) =>
		{
			return !(!mob.inputJump && mob.isGrounded);
		};

		AIAction jump = addAction("jump", 100, 0, 0, jumpSpeed, (AIAction action, Vector2 toTarget, float targetDistance) =>
		{
			TileType forwardTile = GameState.instance.level.getTile(mob.position + new Vector2(1.0f * action.ai.walkDirection, 0.5f));
			TileType forwardUpTile = GameState.instance.level.getTile(mob.position + new Vector2(1.0f * action.ai.walkDirection, 1.5f));
			return forwardTile != null && forwardUpTile == null;
		});
		jump.onStarted = (AIAction action) =>
		{
			mob.inputJump = true;
		};
		jump.onAction = (AIAction action, float elapsed, Vector2 toTarget) =>
		{
			return !(!mob.inputJump && mob.isGrounded);
		};
	}
}
