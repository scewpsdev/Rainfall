using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class State
{
	public Game game;


	public virtual void init()
	{
	}

	public virtual void destroy()
	{
	}

	public virtual void update()
	{
	}

	public virtual void draw(GraphicsDevice graphics)
	{
	}

	public virtual void onKeyEvent(KeyCode key, KeyModifier modifiers, bool down)
	{
	}

	public virtual void onMouseButtonEvent(MouseButton button, bool down)
	{
	}

	public virtual void onGamepadButtonEvent(GamepadButton button, bool down)
	{
	}

	public virtual void onCharEvent(byte length, uint value)
	{
	}

	public virtual void drawDebugStats(int y, byte color, GraphicsDevice graphics)
	{
	}
}
