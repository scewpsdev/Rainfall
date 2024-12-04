using Rainfall;
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

	public ParticleSystem[] particles;


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

		for (int i = 0; i < data.particles.Length; i++)
		{
			ParticleSystemData particleData = data.particles[i];
			byte* textureAtlasPath = particleData.textureAtlasPath;
			if (textureAtlasPath[0] != 0)
				particleData.textureAtlas = Resource.GetTexture(RainfallEditor.CompileAsset(new string((sbyte*)particleData.textureAtlasPath))).handle;
			else
				particleData.textureAtlas = ushort.MaxValue;
			data.particles[i] = particleData;
		}

		if (particles != null)
		{
			foreach (ParticleSystem system in particles)
				ParticleSystem.Destroy(system);
		}
		particles = new ParticleSystem[data.particles.Length];
		for (int i = 0; i < data.particles.Length; i++)
		{
			particles[i] = ParticleSystem.Create(getModelMatrix());
			particles[i].setData(data.particles[i]);
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
		for (int i = 0; i < particles.Length; i++)
		{
			if (!particles[i].hasFinished)
			{
				restartEffect = false;
				break;
			}
		}

		if (particles.Length != data.particles.Length)
			data.particles = ArrayUtils.Resize(data.particles, particles.Length);
		for (int i = 0; i < particles.Length; i++)
		{
			if (restartEffect)
				particles[i].restartEffect();
			particles[i].setTransform(transform, particles[i].handle->applyEntityVelocity);
			data.particles[i] = *particles[i].handle;
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

		for (int i = 0; i < particles.Length; i++)
		{
			Renderer.DrawParticleSystem(particles[i]);
		}
	}

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
