using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TutorialText : Entity
{
	string text;
	int scale;
	uint color;


	public TutorialText(string text, int scale, uint color)
	{
		this.text = text;
		this.scale = scale;
		this.color = color;
	}

	public override void render()
	{
		Vector2i pos = GameState.instance.camera.worldToScreen(position);
		Vector2i size = Renderer.MeasureUIText(text, text.Length, scale);
		Renderer.DrawUIText(pos.x - size.x / 2, pos.y - size.y / 2, text, scale, color);
	}
}
