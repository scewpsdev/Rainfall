using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TextAlert : Entity
{
	string txt;
	public float lifetime = 0.6f;
	uint color = 0xFFFFAAAA;

	long startTime;


	public TextAlert(string txt)
	{
		this.txt = txt;
	}

	public override void init(Level level)
	{
		startTime = Time.currentTime;
		velocity = new Vector2(0, 0.5f);
	}

	public override void update()
	{
		position += velocity * Time.deltaTime;

		if ((Time.currentTime - startTime) / 1e9f >= lifetime)
			remove();
	}

	public override void render()
	{
		float progress = (Time.currentTime - startTime) / 1e9f / lifetime;
		uint c = Mathf.ColorAlpha(color, 1 - progress);
		Renderer.DrawWorldTextBMP(position.x - Renderer.MeasureWorldTextBMP(txt).x / 2 / 16, position.y - Renderer.MeasureWorldTextBMP(txt).y / 2 / 16, 0, txt, 1.0f / 16, c/*, true*/);
	}
}
