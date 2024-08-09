using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class TutorialText : Entity
{
	string text;
	uint color;


	public TutorialText(string text, uint color)
	{
		this.text = text;
		this.color = color;
	}

	public override void render()
	{
		Vector2i pos = GameState.instance.camera.worldToScreen(position);
		Vector2i size = Renderer.MeasureUITextBMP(text, text.Length, 1);
		Renderer.DrawUITextBMP(pos.x - size.x / 2, pos.y - size.y / 2, text, 1, color);
	}
}
