using Rainfall;
using Rainfall.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


public class Entity
{
	static uint idHash = (uint)Time.timestamp;

	static uint GenerateID()
	{
		idHash = Hash.hash(idHash);
		return idHash;
	}


	public SceneFormat.EntityData data;

	public bool showDebugColliders;
	public bool[] showDebugBoneColliders;


	public Entity(string name)
	{
		data = new SceneFormat.EntityData(name, GenerateID());
	}

	public Entity(string name, uint id)
	{
		data = new SceneFormat.EntityData(name, id);
	}

	public unsafe void reload()
	{
		if (data.modelPath != null)
		{
			data.model = Resource.GetModel(RainfallEditor.CompileAsset(data.modelPath));
			if (data.model != null)
				showDebugBoneColliders = new bool[data.model.skeleton.nodes.Length];
		}
		else
		{
			data.model = null;
		}

		for (int i = 0; i < data.colliders.Count; i++)
		{
			SceneFormat.ColliderData collider = data.colliders[i];
			if (collider.meshColliderPath != null)
				collider.meshCollider = Resource.GetModel(RainfallEditor.CompileAsset(collider.meshColliderPath));
			else
				collider.meshCollider = null;
			data.colliders[i] = collider;
		}

		for (int i = 0; i < data.particles.Count; i++)
		{
			ParticleSystemData particleData = data.particles[i];
			byte* textureAtlasPath = particleData.textureAtlasPath;
			if (textureAtlasPath != null)
				particleData.textureAtlas = Resource.GetTexture(RainfallEditor.CompileAsset(new string((sbyte*)particleData.textureAtlasPath))).handle;
			else
				particleData.textureAtlas = ushort.MaxValue;
			data.particles[i] = particleData;
		}
	}

	public void init()
	{
	}

	public void destroy()
	{
	}

	public unsafe void update()
	{
		Matrix transform = getModelMatrix();

		bool restartEffect = true;
		for (int i = 0; i < data.particles.Count; i++)
		{
			ParticleSystemData particles = data.particles[i];
			if (Rainfall.Native.ParticleSystem.ParticleSystem_HasFinished(&particles) == 0)
			{
				restartEffect = false;
				break;
			}
		}

		for (int i = 0; i < data.particles.Count; i++)
		{
			ParticleSystemData particleData = data.particles[i];
			if (restartEffect)
				Rainfall.Native.ParticleSystem.ParticleSystem_Restart(&particleData);
			Rainfall.Native.ParticleSystem.ParticleSystem_SetTransform(&particleData, transform, 1);
			data.particles[i] = particleData;
		}
	}

	public unsafe void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();

		if (data.model != null)
		{
			Renderer.DrawModel(data.model, transform);
		}

		for (int i = 0; i < data.lights.Count; i++)
		{
			Renderer.DrawLight(transform * data.lights[i].offset, data.lights[i].color * data.lights[i].intensity);
		}

		if (RainfallEditor.instance.currentTab.selectedEntity == data.id && showDebugColliders)
		{
			// Debug colliders
			for (int i = 0; i < data.colliders.Count; i++)
			{
				SceneFormat.ColliderData collider = data.colliders[i];
				uint color = collider.trigger ? 0xFFFF0000 : 0xFF00FF00;
				Renderer.DrawDebugCollider(collider, transform, color);
			}

			if (data.model != null)
			{
				if (data.model.scene->numAnimations > 0)
					Renderer.DrawDebugSkeleton(data.model.skeleton, data.boneColliders, transform, 0xFF7F7FFF, showDebugBoneColliders);
			}
		}

		for (int i = 0; i < data.particles.Count; i++)
		{
			ParticleSystemData particleData = data.particles[i];
			Renderer3D_DrawParticleSystem(&particleData);
		}
	}

	[DllImport(Native.DllName, CallingConvention = CallingConvention.Cdecl)]
	extern static unsafe void Renderer3D_DrawParticleSystem(ParticleSystemData* particleSystem);

	unsafe ParticleSystemData? getParticlesByName(string name)
	{
		foreach (ParticleSystemData particle in data.particles)
		{
			byte* namePtr = particle.name;
			if (StringUtils.CompareStrings(name, namePtr))
				return particle;
		}
		return null;
	}

	public string newParticleName()
	{
		int idx = 0;
		string name = "Particles";
		while (getParticlesByName(name) != null)
		{
			name = "Particles" + ++idx;
		}
		return name;
	}

	public Matrix getModelMatrix(Vector3 offset)
	{
		return Matrix.CreateTranslation(data.position + offset) * Matrix.CreateRotation(data.rotation) * Matrix.CreateScale(data.scale);
	}

	public Matrix getModelMatrix()
	{
		return getModelMatrix(Vector3.Zero);
	}
}
