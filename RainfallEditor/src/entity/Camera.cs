﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Camera
{
	public const float FOV = 60;
	public const float NEAR = 0.05f;
	public const float FAR = 500;

	const float MOUSE_SENSITIVITY = 0.004f;


	Vector3 _position = Vector3.Zero;
	Quaternion _rotation = Quaternion.Identity;
	public Vector3 position { get => _position; }
	public Quaternion rotation { get => _rotation; }

	public bool orthographic = false;

	public Vector3 target = Vector3.Zero;
	public float distance = 5.0f;
	public float pitch = 0.0f, yaw = 0.0f;

	Vector2 lastMousePos;

	Vector2 rotationTarget = new Vector2(-1);


	public void updateControls(EditorInstance instance)
	{
		Vector2 mousePos = ImGui.GetMousePos();
		Vector2 mouseDelta = mousePos - lastMousePos;
		lastMousePos = mousePos;

		if (ImGui.IsWindowHovered())
		{
			if (ImGui.IsMouseButtonDown(MouseButton.Left))
			{
				if (ImGui.IsKeyDown(KeyCode.Alt))
				{
					if (ImGui.IsKeyDown(KeyCode.Shift))
					{
						Vector3 right = _rotation.right;
						Vector3 down = _rotation.down;
						float multiplier = 2 * distance * MathF.Tan(0.5f * MathHelper.ToRadians(FOV));
						target -= right * mouseDelta.x / Display.height * multiplier;
						target -= down * mouseDelta.y / Display.height * multiplier;
					}
					else
					{
						pitch -= mouseDelta.y * MOUSE_SENSITIVITY;
						yaw -= mouseDelta.x * MOUSE_SENSITIVITY;

						pitch = MathHelper.Clamp(pitch, -0.5f * MathF.PI, 0.5f * MathF.PI);

						orthographic = false;
					}
				}
			}
			else if (ImGui.IsMouseButtonReleased(MouseButton.Left))
			{
				//if (ImGui.IsKeyDown(KeyCode.LeftAlt) || ImGui.IsKeyDown(KeyCode.LeftShift))
				//	instance.notifyEdit();
			}

			if (!ImGui.IsAnyItemActive())
			{
				if (ImGui.IsKeyPressed(KeyCode.NumPad1) || ImGui.IsKeyPressed(KeyCode.Key1))
				{
					if (ImGui.IsKeyDown(KeyCode.Ctrl))
						rotationTarget = new Vector2(0, MathF.PI);
					else
						rotationTarget = new Vector2(0, 0);
					orthographic = true;
				}
				if (ImGui.IsKeyPressed(KeyCode.NumPad3) || ImGui.IsKeyPressed(KeyCode.Key3))
				{
					if (ImGui.IsKeyDown(KeyCode.Ctrl))
						rotationTarget = new Vector2(0, -MathF.PI * 0.5f);
					else
						rotationTarget = new Vector2(0, MathF.PI * 0.5f);
					orthographic = true;
				}
				if (ImGui.IsKeyPressed(KeyCode.NumPad7) || ImGui.IsKeyPressed(KeyCode.Key7))
				{
					if (ImGui.IsKeyDown(KeyCode.Ctrl))
						rotationTarget = new Vector2(MathF.PI * 0.5f, MathF.PI);
					else
						rotationTarget = new Vector2(-MathF.PI * 0.5f, 0);
					orthographic = true;
				}
			}

			if (rotationTarget.x != -1)
			{
				pitch = MathHelper.LerpAngle((pitch), rotationTarget.x, 16.0f * Time.deltaTime);
				yaw = MathHelper.LerpAngle(yaw, rotationTarget.y, 16.0f * Time.deltaTime);
				if (MathHelper.CompareAngles(pitch, rotationTarget.x, 0.01f) && MathHelper.CompareAngles(yaw, rotationTarget.y, 0.01f))
				{
					pitch = rotationTarget.x;
					yaw = rotationTarget.y;
					rotationTarget = new Vector2(-1);

					//instance.notifyEdit();
				}
			}

			float scroll = ImGui.GetMouseScroll();
			if (scroll != 0)
			{
				distance *= (1 + -scroll * 0.2f);
				//instance.notifyEdit();
			}
		}

		Matrix transform = Matrix.CreateTranslation(target) * Matrix.CreateRotation(Vector3.Up, yaw) * Matrix.CreateRotation(Vector3.Right, pitch) * Matrix.CreateTranslation(0.0f, 0.0f, distance);
		transform.decompose(out _position, out _rotation, out _);
	}

	public void update()
	{
		Audio.UpdateListener(_position, _rotation);
	}

	public void updateView(Matrix view)
	{
		Vector3 newPosition;
		Quaternion newRotation;
		view.inverted.decompose(out newPosition, out newRotation, out _);
		Vector3 newTarget = newPosition + newRotation.forward * distance;
		Vector3 newEulers = newRotation.normalized.eulers;
		target = newTarget;
		pitch = newEulers.x;
		yaw = newEulers.y;
	}

	public Matrix getModelMatrix()
	{
		return Matrix.CreateTranslation(_position) * Matrix.CreateRotation(_rotation);
	}

	public Matrix getProjectionMatrix(float aspect)
	{
		if (orthographic)
			//return Matrix.CreatePerspective(MathHelper.ToRadians(3), aspect, NEAR, FAR) * Matrix.CreateTranslation(0.0f, 0.0f, -40);
			return Matrix.CreateOrthographic(distance * 2 * aspect, distance * 2, -100, +100);
		else
			return Matrix.CreatePerspective(MathHelper.ToRadians(FOV), aspect, NEAR, FAR);
	}

	public Matrix getViewMatrix()
	{
		Matrix model = Matrix.CreateTranslation(_position) * Matrix.CreateRotation(_rotation);
		return model.inverted;
	}
}
