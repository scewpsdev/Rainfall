using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class ArenaZone : Entity
{
	Vector3 halfExtents;
	RigidBody body;

	bool active = false;
	List<SkeletonEnemy> enemies = new List<SkeletonEnemy>();
	int currentRound = 0;
	Creature lastEnemy = null;

	IronBars ironBars;


	public ArenaZone(Vector3 halfExtents)
	{
		this.halfExtents = halfExtents;
	}

	public override void init()
	{
		body = new RigidBody(this, RigidBodyType.Static);
		body.addBoxTrigger(halfExtents);

		DungeonGame.instance.level.addEntity(ironBars = new IronBars(), new Vector3(-0.026f, -0.01f, 19.5f), Quaternion.Identity);
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (otherController != null && otherController.entity is Player)
		{
			if (contactType == ContactType.Found)
			{
				active = true;
			}
			else if (contactType == ContactType.Lost)
			{
				active = false;
			}
		}
	}

	void initRound()
	{
		int numEnemies = (int)MathF.Ceiling(currentRound / 3.0f);
		for (int i = 0; i < numEnemies; i++)
		{
			float x = position.x + MathHelper.RandomFloatGaussian() * halfExtents.x;
			float z = position.z + MathHelper.RandomFloatGaussian() * halfExtents.z;
			float y = position.y - halfExtents.y;
			SkeletonEnemy enemy = new SkeletonEnemy();
			DungeonGame.instance.level.addEntity(enemy, new Vector3(x, y, z), Quaternion.FromAxisAngle(Vector3.Up, Random.Shared.NextSingle() * MathF.PI * 2.0f));
			enemies.Add(enemy);
		}
	}

	void onLastKill(int round, Creature creature)
	{
		if (round == 6)
		{
			DungeonGame.instance.level.addEntity(new ItemPickup(Item.Get("longbow")), creature.position + new Vector3(0.0f, 1.0f, 0.0f), Quaternion.Identity);
			ironBars.activate();
		}
	}

	public override void update()
	{
		if (active)
		{
			int numAlive = 0;
			Creature enemy = null;
			for (int i = 0; i < enemies.Count; i++)
			{
				if (enemies[i].stats.health > 0)
				{
					numAlive++;
					enemy = enemies[i];
				}
			}

			if (numAlive == 0 && lastEnemy != null)
				onLastKill(currentRound, lastEnemy);

			if (numAlive == 1)
				lastEnemy = enemy;
			else
				lastEnemy = null;

			if (numAlive == 0)
			{
				currentRound++;
				initRound();
			}
		}
	}
}
