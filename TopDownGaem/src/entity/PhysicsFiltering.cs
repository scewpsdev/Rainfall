using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class PhysicsFiltering
{
	public const uint DEFAULT = 1 << 0;
	public const uint PLAYER = 1 << 1;
	public const uint PLAYER_HITBOX = 1 << 2;
	public const uint CREATURE = 1 << 3;
	public const uint CREATURE_HITBOX = 1 << 4;
	public const uint WEAPON_HITBOX = 1 << 5;
	public const uint RAGDOLL = 1 << 6;
	public const uint PICKUP = 1 << 7;
	public const uint INTERACTABLE = 1 << 8;
}
