using Rainfall;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


public class Trail
{
	public Vector2[] points;
	Vector4 color;


	public Trail(int numPoints, Vector4 color, Vector2 position)
	{
		points = new Vector2[numPoints];
		this.color = color;

		Array.Fill(points, position);
	}

	public void update()
	{
		for (int i = points.Length - 1; i >= 1; i--)
			points[i] = points[i - 1];
	}

	public void setPosition(Vector2 position)
	{
		points[0] = position;
	}

	public void render()
	{
		for (int i = 0; i < points.Length - 1; i++)
		{
			float alpha = 1 - i / (float)(points.Length - 1);
			alpha = alpha * alpha;
			Renderer.DrawLine(new Vector3(points[i], 0), new Vector3(points[i + 1], 0), color * new Vector4(1, 1, 1, alpha));
		}
	}
}

public class WeaponTrail
{
	public Vector3[] points0;
	public Vector3[] points1;

	Sprite sprite;
	Vector4 color;
	bool additive;


	public WeaponTrail(int numPoints, Sprite sprite, Vector4 color, bool additive, Vector2 tip, Vector2 bas)
	{
		points0 = new Vector3[numPoints];
		points1 = new Vector3[numPoints];

		this.sprite = sprite;
		this.color = color;
		this.additive = additive;

		Array.Fill(points0, new Vector3(tip, 0));
		Array.Fill(points1, new Vector3(bas, 0));
	}

	public void update()
	{
		for (int i = points0.Length - 1; i >= 1; i--)
			points0[i] = points0[i - 1];
		for (int i = points1.Length - 1; i >= 1; i--)
			points1[i] = points1[i - 1];
	}

	public void setPosition(Vector2 tip, Vector2 bas)
	{
		points0[0].xy = tip;
		points1[0].xy = bas;

		float distance = (points0[1].xy - points0[0].xy).length;
		points0[0].z = points0[1].z + distance;
	}

	public void render(Vector2 offset = default)
	{
		float distance = 0;
		for (int i = 0; i < points0.Length - 1; i++)
		{
			Vector3 vertex0 = new Vector3(offset, 0) + points0[i];
			Vector3 vertex1 = new Vector3(offset, 0) + points0[i + 1];
			Vector3 vertex2 = new Vector3(offset, 0) + points1[i];
			Vector3 vertex3 = new Vector3(offset, 0) + points1[i + 1];
			//v2 = Vector2.Lerp(v2, v0, i / (float)(trail.points.Length - 1));
			//v3 = Vector2.Lerp(v3, v1, (i + 1) / (float)(trail.points.Length - 1));
			//float alpha = 1 - i / (float)(points0.Length - 1);
			float alpha = 1;
			//alpha = alpha * alpha;

			Texture texture = null;
			int u0 = 0, v0 = 0, w = 0, h = 0;

			if (sprite != null)
			{
				texture = sprite.spriteSheet.texture;
				u0 = sprite.position.x;
				v0 = sprite.position.y;
				w = sprite.size.x;
				h = sprite.size.y;

				float fract = vertex0.z - vertex1.z;
				float toffset = vertex0.z;
				distance += fract;

				u0 += (int)(toffset * 16) % w;
				w = (int)MathF.Ceiling(fract * w);
				u0 -= w;

				u0 = Math.Max(u0, sprite.position.x);
				w = Math.Max(w, sprite.position.x + sprite.size.x - u0);
			}

			Renderer.DrawSpriteEx(new Vector3(vertex3.xy, 0), new Vector3(vertex2.xy, 0), new Vector3(vertex0.xy, 0), new Vector3(vertex1.xy, 0), texture, u0, v0, w, h, color * new Vector4(1, 1, 1, alpha), additive);
		}
	}
}

public class MobWeaponTrail : Entity
{
	WeaponTrail trail;

	Mob mob;
	Vector2 offset;
	float startAngle, endAngle;
	float range;
	float duration;
	float cooldown;
	bool sideways;

	long startTime;

	public MobWeaponTrail(Mob mob, Vector2 offset, float startAngle, float endAngle, float range, float duration, float cooldown, bool sideways = true)
	{
		this.mob = mob;
		this.offset = offset;
		this.startAngle = startAngle;
		this.endAngle = endAngle;
		this.range = range;
		this.duration = duration;
		this.cooldown = cooldown;
		this.sideways = sideways;

		startTime = Time.currentTime;

		trail = new WeaponTrail(30, null, new Vector4(1, 1, 1, 0.9f), false, getWeaponTip(), getWeaponTip(0.99f));
	}

	Vector2 getWeaponTip(float fract = 1.0f)
	{
		bool flip = mob.direction < 0;
		Vector2 position = new Vector2(range * fract, 0);
		float elapsed = (Time.currentTime - startTime) / 1e9f;
		float angle = MathHelper.Lerp(startAngle, endAngle, elapsed / duration);
		position = Vector2.Rotate(position, angle);
		if (sideways)
			position.y *= 0.5f;
		position += offset;
		if (flip)
			position.x *= -1;
		//position += mob.position;
		return position;
	}

	public override void update()
	{
		position = mob.position;
		trail.update();

		float elapsed = (Time.currentTime - startTime) / 1e9f;
		float progress = MathF.Min(elapsed / duration, 1);
		float thickness = MathF.Abs(progress - 0.5f) + 0.5f;
		trail.setPosition(getWeaponTip(), getWeaponTip(thickness));

		if (elapsed >= duration + cooldown)
			remove();
	}

	public override void render()
	{
		trail.render(mob.position);
	}
}
