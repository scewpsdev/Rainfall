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
	Sprite splash2;

	long startTime;


	public SplashScreenState()
	{
		splash = new Sprite(Resource.GetTexture("sprites/ui/splash1.png", false), 0, 0, 140, 22);
		splash2 = new Sprite(Resource.GetTexture("sprites/ui/splash1.png", false), 0, 38, 140, 5);
	}

	public override void init()
	{
		startTime = Time.timestamp;
	}

	public override void update()
	{
		float elapsed = (Time.timestamp - startTime) / 1e9f - 1;
		if (elapsed > DURATION || InputManager.IsPressed("UIConfirm", true))
			IvoryKeep.instance.popState();
	}

	public override void draw(GraphicsDevice graphics)
	{
		float elapsed = (Time.timestamp - startTime) / 1e9f - 1;
		float alpha = MathF.Max(elapsed < FADEIN ? elapsed / FADEIN : elapsed > FADEIN + SHOW ? 1 - (elapsed - FADEIN - SHOW) / FADEOUT : 1, 0);
		uint color = MathHelper.ColorAlpha(0xFFFFFFFF, alpha);
		Renderer.DrawUISprite(Renderer.UIWidth / 2 - splash.width / 2, Renderer.UIHeight / 2 - splash.height / 2, splash.width, splash.height, splash, false, color);

		float alpha2 = MathF.Max(elapsed - 1 < FADEIN ? (elapsed - 1) / FADEIN : elapsed > FADEIN + SHOW ? 1 - (elapsed - FADEIN - SHOW) / FADEOUT : 1, 0);
		uint color2 = MathHelper.ColorAlpha(0xFFFFFFFF, alpha2);
		Renderer.DrawUISprite(Renderer.UIWidth / 2 - splash2.width / 2, Renderer.UIHeight / 2 - splash.height / 2 + splash.height + 10, splash2.width, splash2.height, splash2, false, color2);
	}
}
