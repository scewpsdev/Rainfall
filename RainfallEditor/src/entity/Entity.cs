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

	/*
	public Vector3 position = Vector3.Zero;
	public Quaternion rotation = Quaternion.Identity;
	public Vector3 scale = Vector3.One;

	public string name = "???";
	public uint id = 0;
	public bool isStatic = false;

	public string modelPath = null;
	public Model model = null;

	public RigidBodyType rigidBodyType;
	public List<SceneFormat.ColliderData> colliders = new List<SceneFormat.ColliderData>();
	public List<SceneFormat.LightData> lights = new List<SceneFormat.LightData>();
	public List<ParticleSystem> particles = new List<ParticleSystem>();
	*/


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
			data.model = Resource.GetModel(RainfallEditor.CompileAsset(data.modelPath));
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
		for (int i = 0; i < data.particles.Count; i++)
		{
			if (data.particles[i].bursts != null && data.particles[i].bursts.Count > 0 && data.particles[i].numParticles == 0)
			{
				bool allBurstsEmitted = true;
				for (int j = 0; j < data.particles[i].bursts.Count; j++)
				{
					if (data.particles[i].bursts[j].emitted < data.particles[i].bursts[j].count)
					{
						allBurstsEmitted = false;
						break;
					}
				}
				if (allBurstsEmitted)
					data.particles[i].restartEffect();
			}

			data.particles[i].update(transform);
		}
	}

	public void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();

		if (data.model != null)
			Renderer.DrawModel(data.model, transform);

		for (int i = 0; i < data.lights.Count; i++)
		{
			Renderer.DrawLight(transform * data.lights[i].offset, data.lights[i].color * data.lights[i].intensity);
		}

		if (RainfallEditor.instance.currentTab.selectedEntity == data.id)
		{
			// Debug colliders
			Vector4 color = new Vector4(0, 1, 0, 1);
			for (int i = 0; i < data.colliders.Count; i++)
			{
				SceneFormat.ColliderData collider = data.colliders[i];

				if (collider.type == SceneFormat.ColliderType.Box)
				{
					Renderer.DrawDebugBox(collider.size, transform * Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers)), color);
				}
				else if (collider.type == SceneFormat.ColliderType.Sphere)
				{
					Renderer.DrawDebugSphere(collider.radius, transform * Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers)), color);
				}
				else if (collider.type == SceneFormat.ColliderType.Capsule)
				{
					Renderer.DrawDebugCapsule(collider.radius, collider.height, transform * Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers)), color);
				}
				else if (collider.type == SceneFormat.ColliderType.Mesh)
				{
					if (collider.meshCollider != null)
					{
						Renderer.DrawDebugBox(collider.meshCollider.boundingBox.Value.size, transform * Matrix.CreateTranslation(collider.meshCollider.boundingBox.Value.offset + collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers)), color);
						Renderer.DrawDebugSphere(collider.meshCollider.boundingSphere.Value.radius, transform * Matrix.CreateTranslation(collider.meshCollider.boundingBox.Value.offset + collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers)), color);
					}
				}
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
