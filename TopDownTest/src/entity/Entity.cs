using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Entity
{
	public Vector3 position = Vector3.Zero;

	public bool removed { get; private set; } = false;


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

	public void remove()
	{
		removed = true;
	}

	public virtual void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
	}

	public Matrix getModelMatrix()
	{
		return Matrix.CreateTranslation(position);
	}
}
