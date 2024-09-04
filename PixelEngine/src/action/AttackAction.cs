using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;


public class AttackAction : EntityAction
{
	public Item weapon;
	public int direction;

	public List<Entity> hitEntities = new List<Entity>();


	public AttackAction(Item weapon, bool mainHand)
		: base("attack", mainHand)
	{
		this.weapon = weapon;

		duration = 1.0f / weapon.attackRate;
	}

	public override void onStarted(Player player)
	{
		direction = player.direction;
	}

	public override void update(Player player)
	{
		base.update(player);

		if (inDamageWindow)
		{
			Span<HitData> hits = new HitData[16];
			int numHits = GameState.instance.level.raycastNoBlock(player.position + new Vector2(0, player.getWeaponOrigin(mainHand).y), new Vector2(MathF.Cos(currentAngle) * direction, MathF.Sin(currentAngle)), currentRange, hits, Entity.FILTER_MOB | Entity.FILTER_DEFAULT);
			for (int i = 0; i < numHits; i++)
			{
				if (hits[i].entity != null && hits[i].entity != player && hits[i].entity is Hittable && !hitEntities.Contains(hits[i].entity))
				{
					Hittable hittable = hits[i].entity as Hittable;
					hittable.hit(weapon.attackDamage * player.attack, player, weapon);
					hitEntities.Add(hits[i].entity);
				}
			}
		}
	}

	public float currentProgress
	{
		get => MathF.Min(elapsedTime / duration + elapsedTime / duration * weapon.attackCooldown, 1);
	}

	public bool inDamageWindow
	{
		get => elapsedTime / duration * 2 < 1.5f;
	}

	public float currentRange
	{
		get => weapon.stab ? currentProgress * weapon.attackRange : weapon.attackRange;
	}

	public float currentAngle
	{
		get => weapon.stab ? 0.0f : (1 - currentProgress) * weapon.attackAngle - 0.25f * MathF.PI;
	}

	public override Matrix getItemTransform()
	{
		if (weapon.stab)
		{
			Matrix weaponTransform = Matrix.CreateTranslation(-0.5f * weapon.size.x + currentRange, 0, 0);
			return weaponTransform;
		}
		else
		{
			float rotation = currentAngle;
			Matrix weaponTransform = Matrix.CreateRotation(Vector3.UnitZ, rotation)
				* Matrix.CreateTranslation(currentRange - 0.5f * weapon.size.x, 0.0f, 0);
			return weaponTransform;
		}
	}
}
