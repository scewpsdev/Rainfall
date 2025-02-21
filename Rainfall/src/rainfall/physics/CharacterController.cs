using System;
using Rainfall;

namespace Rainfall
{
	public enum ControllerCollisionFlag : byte
	{
		Sides = (1 << 0),    //!< Character is colliding to the sides.
		Up = (1 << 1),       //!< Character has collision above.
		Down = (1 << 2)
	}

	public struct ControllerHit
	{
		public Vector3 position;
		public Vector3 normal;
		public float length;
		public Vector3 direction;
	}

	public interface ControllerHitCallback
	{
		void onShapeHit(ControllerHit hit);
	}

	public class CharacterController
	{
		static Dictionary<IntPtr, CharacterController> controllers = new Dictionary<IntPtr, CharacterController>();


		public readonly PhysicsEntity entity;

		internal ControllerHitCallback hitCallback;

		IntPtr controller;
		float _height, _radius;
		Vector3 _offset;

		uint filterMask;


		public CharacterController(PhysicsEntity entity, float radius, Vector3 offset, float height, float stepOffset = 0.1f, uint filterMask = 1, ControllerHitCallback hitCallback = null)
		{
			this.entity = entity;
			this._height = height;
			this._radius = radius;
			this._offset = offset;
			this.filterMask = filterMask;

			this.hitCallback = hitCallback;

			controller = Physics.Physics_CreateCharacterController(radius, height, offset, stepOffset, entity.getPosition());
			controllers.Add(controller, this);
		}

		public void destroy()
		{
			Physics.Physics_DestroyCharacterController(controller);
		}

		public void resize(float height)
		{
			Physics.Physics_ResizeCharacterController(controller, height);
			_height = height;
		}

		public ControllerCollisionFlag move(Vector3 delta)
		{
			return Physics.Physics_MoveCharacterController(controller, delta, filterMask);
		}

		public void setPosition(Vector3 position)
		{
			Physics.Physics_CharacterControllerSetPosition(controller, position);
		}

		public float height
		{
			get { return _height; }
			set
			{
				Physics.Physics_CharacterControllerSetHeight(controller, value);
				_height = value;
			}
		}

		public float radius
		{
			get { return _radius; }
			set
			{
				Physics.Physics_CharacterControllerSetRadius(controller, value);
				_radius = value;
			}
		}

		public Vector3 offset
		{
			get { return _offset; }
			set
			{
				Physics.Physics_CharacterControllerSetOffset(controller, value);
				_offset = value;
			}
		}

		internal static CharacterController GetControllerFromHandle(IntPtr handle)
		{
			if (controllers.ContainsKey(handle))
				return controllers[handle];
			return null;
		}
	}
}
