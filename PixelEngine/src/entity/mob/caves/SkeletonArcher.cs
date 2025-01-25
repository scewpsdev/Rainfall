using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class SkeletonArcher : Mob
{
	public SkeletonArcher()
		: base("skeleton_archer")
	{
		displayName = "Skeleton Archer";

		sprite = new Sprite(Resource.GetTexture("sprites/skeleton.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 1, 1, true);
		animator.addAnimation("charge", 1, 1, true);
		animator.addAnimation("run", 4, 0.666f, true);
		animator.addAnimation("dead", 1, 1, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.3f, 0, 0.6f, 0.75f);

		health = 4;
		speed = 2;

		ai = new ArcherAI(this);
	}

	public override void render()
	{
		base.render();

		Item shortbow = Item.GetItemPrototype("shortbow");
		Renderer.DrawSprite(position.x - 0.5f + direction * 0.4f, position.y, LAYER_PLAYER_ITEM_MAIN, 1, 1, direction * MathF.PI * 0.5f, shortbow.sprite, direction == -1);
	}
}
