using Rainfall;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
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

	public void reload()
	{
		if (data.modelPath != null)
		{
			data.model = Resource.GetModel(RainfallEditor.CompileAsset(data.modelPath));
			showDebugBoneColliders = new bool[data.model.skeleton.nodes.Length];
		}
		else
			data.model = null;

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
			if (data.particles[i].textureAtlasPath != null)
				data.particles[i].textureAtlas = Resource.GetTexture(RainfallEditor.CompileAsset(data.particles[i].textureAtlasPath));
			else
				data.particles[i].textureAtlas = null;
		}
	}

	public void init()
	{
	}

	public void destroy()
	{
	}

	public void update()
	{
		Matrix transform = getModelMatrix();

		bool restartEffect = true;
		for (int i = 0; i < data.particles.Count; i++)
		{
			if (data.particles[i].emissionRate > 0)
				restartEffect = false;
			if (data.particles[i].numParticles > 0)
				restartEffect = false;

			bool allBurstsEmitted = true;
			if (data.particles[i].bursts != null && data.particles[i].bursts.Count > 0 && data.particles[i].numParticles == 0)
			{
				for (int j = 0; j < data.particles[i].bursts.Count; j++)
				{
					if (data.particles[i].bursts[j].emitted < data.particles[i].bursts[j].count)
					{
						allBurstsEmitted = false;
						break;
					}
				}
			}

			if (!allBurstsEmitted)
				restartEffect = false;
		}

		for (int i = 0; i < data.particles.Count; i++)
		{
			if (restartEffect)
				data.particles[i].restartEffect();
			data.particles[i].update(transform);
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
				Vector4 color = collider.trigger ? new Vector4(1, 0, 0, 1) : new Vector4(0, 1, 0, 1);
				Renderer.DrawDebugCollider(collider, transform, color);
			}

			if (data.model != null)
			{
				if (data.model.scene->numAnimations > 0)
					Renderer.DrawDebugSkeleton(data.model.skeleton, data.boneColliders, transform, new Vector4(0.5f, 0.5f, 1, 1), showDebugBoneColliders);
			}
		}

		for (int i = 0; i < data.particles.Count; i++)
		{
			Renderer.DrawParticleSystem(data.particles[i]);
		}
	}

	ParticleSystem getParticlesByName(string name)
	{
		foreach (ParticleSystem particle in data.particles)
		{
			if (particle.name == name)
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
