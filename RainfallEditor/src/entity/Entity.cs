using Rainfall;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


public enum ColliderType
{
	Box,
	Sphere,
	Capsule,
	Mesh,
}

public struct ColliderData
{
	public ColliderType type;

	public Vector3 size;
	public Vector3 offset;
	public Vector3 eulers;

	public string meshColliderPath;
	public Model meshCollider;


	public ColliderData(Vector3 size, Vector3 offset = default, Vector3 eulers = default)
	{
		type = ColliderType.Box;
		this.size = size;
		this.offset = offset;
		this.eulers = eulers;
	}

	public ColliderData(float radius, Vector3 offset = default, Vector3 eulers = default)
	{
		type = ColliderType.Sphere;
		size = new Vector3(2 * radius, 0, 0);
		this.offset = offset;
		this.eulers = eulers;
	}

	public ColliderData(float radius, float height, Vector3 offset = default, Vector3 eulers = default)
	{
		type = ColliderType.Capsule;
		size = new Vector3(2 * radius, height, 0);
		this.offset = offset;
		this.eulers = eulers;
	}

	public ColliderData(string path, Vector3 offset = default, Vector3 eulers = default)
	{
		type = ColliderType.Mesh;
		meshColliderPath = path;
		this.offset = offset;
		this.eulers = eulers;
	}

	public float radius
	{
		get => 0.5f * size.x;
		set { size.x = value * 2; }
	}
	public float height
	{
		get => size.y;
		set { size.y = value; }
	}

	public void reload()
	{
		if (meshColliderPath != null)
		{
			string compiledPath = RainfallEditor.instance.compileAsset(meshColliderPath);
			meshCollider = Resource.GetModel(compiledPath);
		}
		else
		{
			meshCollider = null;
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is ColliderData)
			return (ColliderData)obj == this;
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(ColliderData a, ColliderData b)
	{
		return a.type == b.type && a.size == b.size && a.offset == b.offset && a.eulers == b.eulers && a.meshColliderPath == b.meshColliderPath && a.meshCollider == b.meshCollider;
	}

	public static bool operator !=(ColliderData a, ColliderData b) => !(a == b);
}

public struct LightData
{
	public Vector3 color;
	public float intensity;
	public Vector3 offset;


	public LightData(Vector3 color, float intensity, Vector3 offset = default)
	{
		this.color = color;
		this.intensity = intensity;
		this.offset = offset;
	}

	public override bool Equals(object obj)
	{
		if (obj is LightData)
			return (LightData)obj == this;
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(LightData a, LightData b)
	{
		return a.color == b.color && a.intensity == b.intensity && a.offset == b.offset;
	}

	public static bool operator !=(LightData a, LightData b) => !(a == b);
}

public class Entity
{
	static uint idHash = (uint)Time.timestamp;

	static uint GenerateID()
	{
		idHash = Hash.hash(idHash);
		return idHash;
	}


	public Vector3 position = Vector3.Zero;
	public Quaternion rotation = Quaternion.Identity;
	public Vector3 scale = Vector3.One;

	public string name = "???";
	public uint id = 0;

	public string modelPath = null;
	public Model model = null;

	public List<ColliderData> colliders = new List<ColliderData>();
	public List<LightData> lights = new List<LightData>();
	public List<ParticleSystem> particles = new List<ParticleSystem>();


	public Entity(string name)
	{
		this.name = name;
		id = GenerateID();
	}

	public void reload()
	{
		if (modelPath != null)
		{
			string compiledPath = RainfallEditor.instance.compileAsset(modelPath);
			model = Resource.GetModel(compiledPath);
		}
		else
		{
			model = null;
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
		for (int i = 0; i < particles.Count; i++)
		{
			if (particles[i].bursts != null && particles[i].bursts.Count > 0 && particles[i].numParticles == 0)
			{
				bool allBurstsEmitted = true;
				for (int j = 0; j < particles[i].bursts.Count; j++)
				{
					if (particles[i].bursts[j].emitted < particles[i].bursts[j].count)
					{
						allBurstsEmitted = false;
						break;
					}
				}
				if (allBurstsEmitted)
					particles[i].restartEffect();
			}

			particles[i].update(transform);
		}
	}

	public void draw(GraphicsDevice graphics)
	{
		Matrix transform = getModelMatrix();

		if (model != null)
			Renderer.DrawModel(model, transform);

		for (int i = 0; i < lights.Count; i++)
		{
			Renderer.DrawLight(transform * lights[i].offset, lights[i].color * lights[i].intensity);
		}

		if (RainfallEditor.instance.currentTab.selectedEntity == id)
		{
			// Debug colliders
			Vector4 color = new Vector4(0, 1, 0, 1);
			for (int i = 0; i < colliders.Count; i++)
			{
				ColliderData collider = colliders[i];

				if (collider.type == ColliderType.Box)
				{
					Renderer.DrawDebugBox(collider.size, transform * Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers)), color);
				}
				else if (collider.type == ColliderType.Sphere)
				{
					Renderer.DrawDebugSphere(collider.radius, transform * Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers)), color);
				}
				else if (collider.type == ColliderType.Capsule)
				{
					Renderer.DrawDebugCapsule(collider.radius, collider.height, transform * Matrix.CreateTranslation(collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers)), color);
				}
				else if (collider.type == ColliderType.Mesh)
				{
					if (collider.meshCollider != null)
					{
						Renderer.DrawDebugBox(collider.meshCollider.boundingBox.Value.size, transform * Matrix.CreateTranslation(collider.meshCollider.boundingBox.Value.offset + collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers)), color);
						Renderer.DrawDebugSphere(collider.meshCollider.boundingSphere.Value.radius, transform * Matrix.CreateTranslation(collider.meshCollider.boundingBox.Value.offset + collider.offset) * Matrix.CreateRotation(Quaternion.FromEulerAngles(collider.eulers)), color);
					}
				}
			}
		}

		for (int i = 0; i < particles.Count; i++)
		{
			particles[i].draw(graphics);
		}
	}

	ParticleSystem getParticlesByName(string name)
	{
		foreach (ParticleSystem particle in particles)
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
		return Matrix.CreateTranslation(position + offset) * Matrix.CreateRotation(rotation) * Matrix.CreateScale(scale);
	}

	public Matrix getModelMatrix()
	{
		return getModelMatrix(Vector3.Zero);
	}
}
