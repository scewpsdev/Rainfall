using Rainfall;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class StatusTriggerText : Entity
{
	Entity entity;
	string text;
	uint color;
	Vector2 offset;
	long startTime;

	const float duration = 1.5f;


	public StatusTriggerText(Entity entity, string text, uint color)
	{
		this.entity = entity;
		this.text = text;
		this.color = color;
	}

	public override void init(Level level)
	{
		startTime = Time.currentTime;
		offset = position - entity.position;
	}

	public override void render()
	{
		float elapsed = (Time.currentTime - startTime) / 1e9f;
		float yoffset = elapsed / duration * 1.2f;

		position = entity.position + offset;
		Vector2 pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, yoffset));

		float alpha = elapsed < 0.5f ? elapsed * 2 : elapsed > duration - 0.5f ? MathF.Max(duration - elapsed, 0) * 2 : 1;
		Renderer.DrawUITextBMP(pos.x - Renderer.MeasureUITextBMP(text).x / 2, pos.y - Renderer.MeasureUITextBMP(text).y, text, 1, Mathf.ColorAlpha(color, alpha));

		if (elapsed >= duration)
			remove();
	}
}
