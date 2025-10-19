using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


struct SpiderLeg
{
	public Vector2 origin;
	public Vector2 optimalPosition;
	public Vector2 destination;
	public Vector2 position;
	public bool attached;
}

public class Stalker : Mob
{
	const float legLength0 = 0.4f;
	const float legLength1 = 0.6f;
	const float targetForwardOffset = 0.5f;
	const float maxFootDistance = 0.4f;

	SpiderLeg leg0 = new SpiderLeg { origin = new Vector2(-0.25f, 0.05f), optimalPosition = new Vector2(-0.65f, -0.25f) };
	SpiderLeg leg1 = new SpiderLeg { origin = new Vector2(0.25f, 0.05f), optimalPosition = new Vector2(0.65f, -0.25f) };
	SpiderLeg leg2 = new SpiderLeg { origin = new Vector2(-0.25f, 0.05f), optimalPosition = new Vector2(-0.65f, -0.25f) };
	SpiderLeg leg3 = new SpiderLeg { origin = new Vector2(0.25f, 0.05f), optimalPosition = new Vector2(0.65f, -0.25f) };

	long lastLegMoved;
	int lastIt = 0;


	public Stalker()
		: base("stalker")
	{
		displayName = "Stalker";

		sprite = new Sprite(Resource.GetTexture("sprites/mob/dungeons/stalker.png", false), 0, 0, 16, 16);
		animator = new SpriteAnimator();
		animator.addAnimation("idle", 1, 1, true);
		animator.addAnimation("dead", 1, 1, true);
		animator.setAnimation("idle");

		collider = new FloatRect(-0.25f, -0.25f, 0.5f, 0.5f);
		rect = new FloatRect(-0.5f, -0.5f, 1, 1);

		health = 6;
		poise = 0;

		speed = 2;
		gravity = 0;
		direction = -1;

		spawnRate = 0.5f;

		ai = new StalkerAI(this);
	}

	public override void init(Level level)
	{
		HitData hit = level.raycastTiles(position, Vector2.Down, 1.0f);
		if (hit != null)
			position = hit.position + Vector2.Up * 0.25f;

		leg0.position = leg0.destination = position + Vector2.Rotate(leg0.optimalPosition, rotation);
		leg1.position = leg1.destination = position + Vector2.Rotate(leg1.optimalPosition, rotation);
		leg2.position = leg2.destination = position + Vector2.Rotate(leg2.optimalPosition, rotation);
		leg3.position = leg3.destination = position + Vector2.Rotate(leg3.optimalPosition, rotation);
	}

	void updateLeg(Vector2 legOrigin, Vector2 optimalPoint, ref Vector2 legPos, ref Vector2 legDest, ref bool legAttached)
	{
		Vector2 moveDirection = (velocity + impulseVelocity).normalized;
		Vector2 down = Vector2.Rotate(Vector2.Down, rotation);
		Vector2 right = Vector2.Rotate(Vector2.Right, rotation);
		Vector2 toOptimalHoriz = right * Vector2.Dot(right, optimalPoint - legOrigin);

		float destDistance = (legOrigin - legDest).length;
		float distanceFromOptimalPoint = (optimalPoint - legDest).length;
		bool shouldMoveLeg = distanceFromOptimalPoint > maxFootDistance || destDistance < (legLength1 - legLength0) || destDistance > legLength0 + legLength1 || !legAttached;
		float legMoveFrequency = speed * 5.0f;
		if (shouldMoveLeg && (Time.currentTime - lastLegMoved) / 1e9f > 1.0f / legMoveFrequency)
		//if (MathF.Abs(distanceFromOptimalPoint - furthestDistance) < 0.001f && (Time.currentTime - lastLegMoved) / 1e9f > legMoveCooldown)
		{
			lastLegMoved = Time.currentTime;
			lastIt++;

			HitData hit = null;
			if (hit == null)
				hit = level.raycastTiles(legOrigin, toOptimalHoriz.normalized, 1.0f);
			if (hit == null)
				hit = level.raycastTiles(legOrigin + toOptimalHoriz + moveDirection * targetForwardOffset, down, 1.0f);
			if (hit == null)
				hit = level.raycastTiles(optimalPoint, (-toOptimalHoriz.normalized + down * 0.5f).normalized, 1.5f);

			if (hit != null)
			{
				if ((hit.position - legOrigin).length < legLength0 + legLength1)
				{
					legPos = legDest - down * 0.25f;
					legDest = hit.position;
					legAttached = true;
				}
				else
				{
					legDest = legOrigin + (hit.position - legOrigin).normalized * (legLength0 + legLength1);
					legAttached = false;
				}
			}
			else
			{
				legDest = optimalPoint; // position + Vector2.Rotate(Vector2.Down, rotation) * legLength;
				legAttached = false;
			}
		}

		legPos = Vector2.Linear(legPos, legDest, 5 * Time.deltaTime);
	}

	public override void update()
	{
		base.update();

		SpiderLeg[] arr = [leg0, leg1, leg2, leg3];
		int offset = lastIt % 4;
		for (int i = 0; i < 4; i++)
		{
			int idx = (offset + i) % 4;
			ref SpiderLeg leg = ref arr[idx];

			updateLeg(position + Vector2.Rotate(leg.origin, rotation), position + Vector2.Rotate(leg.optimalPosition, rotation), ref leg.position, ref leg.destination, ref leg.attached);
		}

		leg0 = arr[0];
		leg1 = arr[1];
		leg2 = arr[2];
		leg3 = arr[3];
	}

	void renderLeg(Vector2 origin, Vector2 pos, float length0, float length1, int kneeDirection)
	{
		Vector2 toPos = pos - origin;

		float a = length1;
		float b = toPos.length;
		float c = length0;

		if (b > a + c)
		{
			float multiplier = b / (a + c) * 1.1f;
			a *= multiplier;
			c *= multiplier;
		}
		else if (b < MathF.Abs(a - c))
		{
			a = length0;
		}

		float alpha = MathF.Acos((b * b + c * c - a * a) / (2 * b * c));

		Vector2 jointPosition = origin + Vector2.Rotate(toPos / b, -kneeDirection * alpha) * c;

		Renderer.DrawLine(new Vector3(origin, 0), new Vector3(jointPosition, 0), MathHelper.ARGBToVector(0xFF444444));
		Renderer.DrawLine(new Vector3(jointPosition, 0), new Vector3(pos, 0), MathHelper.ARGBToVector(0xFF444444));
	}

	public override void render()
	{
		base.render();

		//Vector2 target0 = position + Vector2.Rotate(leg0.optimalPosition, rotation);// + (velocity + impulseVelocity).normalized * targetForwardOffset;
		//Vector2 target1 = position + Vector2.Rotate(leg1.optimalPosition, rotation);// + (velocity + impulseVelocity).normalized * targetForwardOffset;
		//Renderer.DrawSprite(target0.x, target0.y, 1.0f / 16, 1.0f / 16, null, false, new Vector4(1, 0, 0, 1));
		//Renderer.DrawSprite(target1.x, target1.y, 1.0f / 16, 1.0f / 16, null, false, new Vector4(1, 0, 0, 1));

		renderLeg(position + Vector2.Rotate(leg0.origin, rotation), leg0.position, legLength0, legLength1, 1);
		renderLeg(position + Vector2.Rotate(leg1.origin, rotation), leg1.position, legLength0, legLength1, -1);
		renderLeg(position + Vector2.Rotate(leg2.origin, rotation), leg2.position, legLength0, legLength1, 1);
		renderLeg(position + Vector2.Rotate(leg3.origin, rotation), leg3.position, legLength0, legLength1, -1);
	}
}
