using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Ogre : Creature
{
	public Ogre()
		: base("ogre")
	{
		setHealth(200);

		addAttack(new CreatureAttack("attack1", "attack1", new Vector2i(16, 30), 35, DamageType.Blunt) { turnFrameStart = 5, turnFrameEnd = 25, turnSpeed = 1 });
		addAttack(new CreatureAttack("attack_backswing", "attack_backswing", new Vector2i(16, 26), 35, DamageType.Blunt)
		{
			triggerDistanceMin = 0,
			triggerDistanceMax = 2,
			triggerAngleMin = -250,
			triggerAngleMax = -90
		});
		addAttack(new CreatureAttack("attack_roar", "attack_roar", Vector2i.Zero, 0, DamageType.Magic, null)
		{
			rarity = 0.1f,
			blockable = false,
			triggerDistanceMin = 1,
			triggerDistanceMax = 10,
			events = [new CreatureActionEvent(35 / 24.0f, (Creature creature) =>
			{
				creature.scene.addEntity(new ParticleEffect("effect/shockwave.rfs", creature), creature.position + Vector3.Up * 2);
				creature.scene.addEntity(new DamageVolume(2, 1.0f, 1, 2, creature), creature.position);
			}) ]
		});
		//addAttack(new CreatureAttack("attack2", "attack2", new Vector2i(16, 30), 35, DamageType.Strike, "attack1"));

		weaponReach = 0.9f;
		weaponRadius = 0.1f;

		scale = new Vector3(2.272f);

		ai = new CreatureAI(this);
	}
}
