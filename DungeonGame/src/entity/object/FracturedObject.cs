﻿using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class FracturedObject : Entity
{
	Model model;
	RigidBody[] bodies;

	AudioSource audio;
	Sound[] sfxBreak;
	float sfxBreakGain;


	public FracturedObject(Model model, Sound[] sfxBreak, float sfxBreakGain = 1.0f)
	{
		this.model = model;
		this.sfxBreak = sfxBreak;
		this.sfxBreakGain = sfxBreakGain;
	}

	public override void init()
	{
		bodies = new RigidBody[model.meshCount];
		for (int i = 0; i < model.meshCount; i++)
		{
			bodies[i] = new RigidBody(null, RigidBodyType.Dynamic, (uint)PhysicsFilterGroup.Debris);
			bodies[i].addConvexMeshCollider(model, i, Matrix.Identity);
			bodies[i].setTransform(position, rotation);
		}

		if (sfxBreak != null)
		{
			audio = new AudioSource(position);
			audio.playSoundOrganic(sfxBreak, sfxBreakGain);
			AIManager.NotifySound(position, 6.0f);
		}
	}

	public override void destroy()
	{
		model.destroy();
		for (int i = 0; i < model.meshCount; i++)
			bodies[i].destroy();
		audio.destroy();
	}

	public override void update()
	{
	}

	public override void draw(GraphicsDevice graphics)
	{
		for (int i = 0; i < model.meshCount; i++)
		{
			bodies[i].getTransform(out Vector3 position, out Quaternion rotation);
			Matrix pieceTransform = Matrix.CreateTransform(position, rotation);
			Renderer.DrawSubModel(model, i, pieceTransform);
		}
	}
}
