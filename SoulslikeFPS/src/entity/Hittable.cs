using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public struct HitData
{
	public int damage;
	public int stagger;
	public bool critical;
	public bool blockable;
	public Vector3 hitDirection;
	public Entity by;
	public Item item;
	public RigidBody hitbox;


	public HitData(int _)
	{
		damage = 10;
		stagger = 1;
		critical = false;
		blockable = true;
		hitDirection = Vector3.Zero;
		by = null;
		item = null;
		hitbox = null;
	}

	public HitData(Entity by, Item item, RigidBody hitbox)
	{
		damage = 10;
		stagger = 1;
		critical = false;
		blockable = true;
		hitDirection = Vector3.Zero;
		this.by = by;
		this.item = item;
		this.hitbox = hitbox;
	}

	public HitData(int damage, bool critical, Vector3 hitDirection, Entity by, Item item, RigidBody hitbox)
	{
		this.damage = damage;
		this.stagger = 1;
		this.critical = critical;
		this.blockable = true;
		this.hitDirection = hitDirection;
		this.by = by;
		this.item = item;
		this.hitbox = hitbox;
	}
}

public interface Hittable
{
	void hit(HitData hit);
	void stagger(int level) { }
}
