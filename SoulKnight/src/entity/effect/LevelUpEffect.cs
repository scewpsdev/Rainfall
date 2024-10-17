using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LevelUpEffect : Entity
{
	long startTime;
	Entity entity;
	Vector2 offset;

	const float duration = 1.5f;


	public LevelUpEffect(Entity entity)
	{
		this.entity = entity;
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
		Vector2i pos = GameState.instance.camera.worldToScreen(position + new Vector2(0, yoffset));

		float alpha = elapsed < 0.5f ? elapsed * 2 : elapsed > duration - 0.5f ? (duration - elapsed) * 2 : 1;
		uint color = MathHelper.ColorAlpha(0xFFFFFFFF, alpha);

		string str = "LEVEL UP";
		Renderer.DrawUITextBMP(pos.x - Renderer.MeasureUITextBMP(str).x / 2, pos.y - Renderer.MeasureUITextBMP(str).y, str, 1, color);

		if (elapsed >= duration)
			remove();
	}
}
