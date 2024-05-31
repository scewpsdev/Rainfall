using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class AttackAction : Action
{
	const float HIT_FREEZE_FRAMES = 3 / 60.0f;
	const float HIT_SLOWDOWN_STRENGTH = 0.1f;

	const float HIT_KNOCKBACK_DURATION = 0.4f;

	const float PIVOT_WINDOW = 0.2f;
	const float PIVOT_WINDOW_START = 5 / 24.0f;


	public Item item;
	public int handID;
	public Attack attack;
	Item castItem;

	Entity[] projectileEntities;

	public List<Entity> hitEntities = new List<Entity>();

	long lastHittableHitTime = 0;


	public AttackAction(Item item, int handID, Attack attack, Item castItem = null)
		: base("attack")
	{
		this.item = item;
		this.handID = handID;
		this.attack = attack;
		this.castItem = castItem;

		animationName[0] = attack.animation;
		animationSet[0] = item.moveset;

		if (item.twoHanded)
		{
			animationName[1] = attack.animation;
			animationName[2] = attack.animation;
			animationSet[1] = item.moveset;
			animationSet[2] = item.moveset;
		}
		else
		{
			animationName[1 + handID] = attack.animation;
			animationSet[1 + handID] = item.moveset;
		}

		mirrorAnimation = handID == 1;
		rootMotion = true;
		animationTransitionDuration = 0.1f;
		if (attack.followUpCancelTime != 0)
			followUpCancelTime = attack.followUpCancelTime;

		movementSpeedMultiplier = 0.0f;
		rotationSpeedMultiplier = 0.0f;

		staminaCost = attack.staminaCost;
		staminaCostTime = attack.damageTimeStart;

		manaCost = attack.manaCost;
		manaCostTime = attack.damageTimeStart;

		if (attack.damageTimeStart != 0 && attack.damageTimeEnd != 0)
		{
			Sound sfx = (attack.type == AttackType.Light || attack.type == AttackType.Running || attack.type == AttackType.Riposte || attack.type == AttackType.Dodging || attack.type == AttackType.Sneak) ?
				Resource.GetSound("res/item/sfx/swing.ogg")
				: (attack.type == AttackType.Heavy) ?
				Resource.GetSound("res/item/sfx/swing_heavy.ogg")
				: null;
			if (sfx != null)
			{
				float sfxTime = (attack.damageTimeStart + attack.damageTimeEnd) / 2 - sfx.duration / 2;
				addSoundEffect(new ActionSfx(sfx, 2, sfxTime, true));
			}
		}

		if (attack.projectiles != null)
			projectileEntities = new Entity[attack.projectiles.Length];
	}

	public override void onStarted(Player player)
	{
		base.onStarted(player);

		//player.setDirection(player.getInputDirection());
	}

	public override void update(Player player)
	{
		base.update(player);

		player.getHand(handID).hitboxEnabled = elapsedTime >= attack.damageTimeStart && elapsedTime <= attack.damageTimeEnd;

		if (elapsedTime >= attack.parryTimeStart && elapsedTime <= attack.parryTimeEnd)
		{
			player.parryingItem = item;
			player.parryingHand = handID;
		}
		else
		{
			player.parryingItem = null;
			player.parryingHand = -1;
		}

		if (elapsedTime >= PIVOT_WINDOW_START && elapsedTime < PIVOT_WINDOW_START + PIVOT_WINDOW)
			rotationSpeedMultiplier = 1;
		else
			rotationSpeedMultiplier = 0;

		/*
		if (attack.projectiles != null)
		{
			for (int i = 0; i < attack.projectiles.Length; i++)
			{
				AttackProjectile projectile = attack.projectiles[i];

				Matrix projectileTransform = player.getHand(handID).transform;
				if (castItem != null)
					projectileTransform = projectileTransform * Matrix.CreateTranslation(castItem.castOrigin);
				Vector3 projectileStart = player.camera.position + player.camera.rotation.forward * 0.5f;
				Vector3 projectileOffset = projectileTransform.translation - projectileStart;

				if (elapsedTime >= projectile.time && projectileEntities[i] == null)
				{
					EntityType projectileType = EntityType.Get(projectile.name);
					if (projectileType != null)
					{
						Entity projectileEntity = projectileType.create();
						projectileEntities[i] = projectileEntity;

						if (projectileEntity is Projectile)
						{
							Projectile p = projectileEntity as Projectile;
							float damage = item.baseDamage * attack.damageMultiplier;
							float poiseDamage = item.poiseDamage * attack.damageMultiplier;
							p.shoot(player.camera.rotation.forward, projectileOffset + projectile.offset, damage, poiseDamage, player, item, attack);
						}

						GameState.instance.level.addEntity(projectileEntity, projectileStart);

						if (projectile.consumesItem)
							player.inventory.removeItem(player.inventory.getSelectedHandSlot(handID));

						player.stats.consumeMana(attack.manaCost);
						if (projectile.sfx != null)
							Audio.PlayOrganic(projectile.sfx, projectileTransform.translation);
					}
				}

				if (projectileEntities[i] != null)
				{
					if (attack.projectiles[i].follow)
						projectileEntities[i].position = projectileTransform.translation;
				}
			}
		}
		*/

		if ((Time.currentTime - lastHittableHitTime) / 1e9f < HIT_FREEZE_FRAMES)
		{
			animationSpeed = 1.0f * HIT_SLOWDOWN_STRENGTH;
			player.currentActionAnim.animationSpeed = animationSpeed;
		}
		else
		{
			animationSpeed = 1.0f;
			player.currentActionAnim.animationSpeed = animationSpeed;
		}
	}

	public override void onFinished(Player player)
	{
		base.onFinished(player);

		player.getHand(handID).hitboxEnabled = false;

		player.parryingItem = null;
		player.parryingHand = -1;
	}

	public void onContact(RigidBody body, Entity from, Vector3 hitPosition, Vector3 hitDirection)
	{
		Entity entity = body.entity as Entity;
		if (entity != null)
		{
			bool firstHit = !hitEntities.Contains(entity);
			if (firstHit)
			{
				if (entity is Hittable)
				{
					float damage = item.baseDamage * attack.damageMultiplier;
					int poiseDamage = (int)MathF.Ceiling(item.poiseDamage * attack.poiseDamageMultiplier);

					Hittable hittable = entity as Hittable;
					hittable.hit(damage, poiseDamage, from, item, hitPosition, hitDirection, body);
					hitEntities.Add(entity);

					lastHittableHitTime = Time.currentTime;
				}
			}
		}
	}
}
