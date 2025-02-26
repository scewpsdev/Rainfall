using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class PhysicsFilter
{
	public const uint Default = 1 << 0;
	public const uint Player = 1 << 1;
	public const uint PlayerHitbox = 1 << 2;
	public const uint Creature = 1 << 3;
	public const uint CreatureHitbox = 1 << 4;
	public const uint Weapon = 1 << 5;
	public const uint Ragdoll = 1 << 6;
	public const uint Pickup = 1 << 7;
	public const uint Interactable = 1 << 8;
}
