using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;


public class SplashScreenState : State
{
	const float FADEIN = 1;
	const float SHOW = 2;
	const float FADEOUT = 2;
	const float DURATION = FADEIN + SHOW + FADEOUT + 1;

	Sprite splash;

	long startTime;


	public SplashScreenState()
	{
		splash = new Sprite(Resource.GetTexture("splash.png", false));
	}

	public override void init()
	{
		startTime = Time.currentTime;
	}

	public override void update()
	{
		float elapsed = (Time.currentTime - startTime) / 1e9f - 1;
		if (elapsed > DURATION || InputManager.IsPressed("UIConfirm", true))
			IvoryKeep.instance.popState();
	}

	public override void draw(GraphicsDevice graphics)
	{
		float elapsed = (Time.currentTime - startTime) / 1e9f - 1;
		float alpha = MathF.Max(elapsed < FADEIN ? elapsed / FADEIN : elapsed > FADEIN + SHOW ? 1 - (elapsed - FADEIN - SHOW) / FADEOUT : 1, 0);
		uint color = MathHelper.ColorAlpha(0xFFFFFFFF, alpha);
		Renderer.DrawUISprite(Renderer.UIWidth / 2 - splash.width / 2, Renderer.UIHeight / 2 - splash.height / 2, splash.width, splash.height, splash, false, color);
	}
}
