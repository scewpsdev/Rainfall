using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum PhysicsFilterGroup : uint
{
	Default = 1 << 0,

	PlayerController = 1 << 1,
	CreatureMovementBody = 1 << 3,
	ItemPickup = 1 << 4,
	Ragdoll = 1 << 5,
	Weapon = 1 << 6,
	Debris = 1 << 7,

	PlayerControllerKinematicBody = 1 << 16,
	CreatureHitbox = 1 << 17,
	Interactable = 1 << 18,
}

public enum PhysicsFilterMask : uint
{
	All = 0x0000FFFF,

	PlayerController = All ^ PhysicsFilterGroup.ItemPickup ^ PhysicsFilterGroup.Ragdoll ^ PhysicsFilterGroup.Debris,
	CreatureMovementBody = All ^ PhysicsFilterGroup.ItemPickup ^ PhysicsFilterGroup.Ragdoll ^ PhysicsFilterGroup.Weapon | PhysicsFilterGroup.PlayerControllerKinematicBody,
	ItemPickup = All ^ PhysicsFilterGroup.PlayerController,
	Ragdoll = All ^ PhysicsFilterGroup.PlayerController | PhysicsFilterGroup.PlayerControllerKinematicBody,
	Weapon = All ^ PhysicsFilterGroup.CreatureMovementBody | PhysicsFilterGroup.CreatureHitbox,

	PlayerControllerKinematicBody = PhysicsFilterGroup.Ragdoll,
	CreatureHitbox = PhysicsFilterGroup.Weapon,
}
