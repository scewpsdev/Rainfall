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
		Vector2 size = Renderer.MeasureWorldTextBMP(text, text.Length, 1.0f / 16);
		Renderer.DrawWorldTextBMP(position.x - size.x / 2, position.y - size.y / 2, LAYER_BG, text, 1.0f / 16, color);
	}
}
