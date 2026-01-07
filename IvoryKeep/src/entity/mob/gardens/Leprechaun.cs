using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Leprechaun : Mob, CoinTarget
{
	public int money = 0;

	public Leprechaun()
		: base("leprechaun")
	{
		displayName = "Leprechaun";

		sprite = new Sprite(Resource.GetTexture("sprites/leprechaun.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 2, 0.5f, true);
		animator.addAnimation("run", 8, 0.5f, true);
		animator.addAnimation("jump", 1, 1, true);
		animator.addAnimation("dead", 1, 1, true);
		animator.setAnimation("idle");

		collider = new Hitbox(-0.3f, 0, 0.6f, 0.75f);

		ai = new LeprechaunAI(this);

		health = 3;
		speed = 3;
		jumpPower = 7;
		//gravity = -16;
		//damage = 0.5f;
	}

	public bool isCoinTargetActive() => isAlive;

	public void giveMoney(int amount)
	{
		money += amount;
	}

	public override void onDeath(Entity by, Item item)
	{
		base.onDeath(by, item);

		while (money > 0)
		{
			CoinType type = Coin.SubtractCoinFromValue(ref money);
			GameState.instance.level.addEntity(new Coin(type), position);
		}
	}
}
