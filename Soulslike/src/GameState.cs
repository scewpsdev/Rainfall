using Rainfall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GameState : State
{
	public static GameState instance { get; private set; }


	public Scene scene;
	public WorldManager world;

	public Camera camera;
	public Player player;


	public GameState()
	{
		instance = this;
	}

	public override void init()
	{
		scene = new Scene();

		scene.addEntity(world = new WorldManager());

		//DungeonGenerator.Generate(world, scene);

		MapLoader.Run(world, scene);

		/*
		Entity testmap = new Entity();
		testmap.model = Resource.GetModel("level/cemetary/cemetary.gltf");
		testmap.body = new RigidBody(testmap, RigidBodyType.Static);
		testmap.body.addMeshColliders(testmap.model, Matrix.Identity);
		scene.addEntity(testmap);

		scene.addEntity(new Fireplace(), new Vector3(0, 0, 0));

		scene.addEntity(new ItemEntity(new Longsword()), new Vector3(2, 1.5f, 0));
		scene.addEntity(new ItemEntity(new KingsSword()), new Vector3(3, 1.5f, 0));
		scene.addEntity(new ItemEntity(new BrokenSword()), new Vector3(4, 1.5f, 0));

		scene.addEntity(new Hollow(), new Vector3(2, 0, -2));
		*/

		scene.addEntity(player = new Player(), new Vector3(0, 1, -1));
		scene.addEntity(camera = new ThirdPersonCamera(player, new Vector3(0, 1.4f, 0)));

		scene.addEntity(new TargetDummy(), new Vector3(0, 0, -4));

		//scene.addEntity(camera = new Camera());
		//scene.addEntity(player = new Player(camera), world.spawnPoints["default"]);
	}

	public override void destroy()
	{
		scene.destroy();
	}

	public override void update()
	{
		Animator.Update(camera.getModelMatrix());
		ParticleSystem.Update(camera.position, camera.rotation);

		scene.update();
	}

	public override void fixedUpdate(float delta)
	{
		scene.fixedUpdate(delta);
	}

	public override void draw(GraphicsDevice graphics)
	{
		scene.draw(graphics);

		//float t = Time.gameTime % 10 / 10;
		//interpolateCurve(new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(0, 2, 0), new Vector3(1, 2, 0), t, out Vector3 p, out Vector3 q);
		//Renderer.DrawDebugLine(p, q, 0xFF000000);

		//Console.WriteLine(p.ToString() + "  |  " + q.ToString());
	}

	void interpolateCurve(Vector3 origin0, Vector3 tip0, Vector3 origin1, Vector3 tip1, float t, out Vector3 origin, out Vector3 tip)
	{
		float d0 = (tip0 - origin0).length;
		float d1 = (tip1 - origin1).length;
		Vector3 direction0 = (tip0 - origin0) / d0;
		Vector3 direction1 = (tip1 - origin1) / d1;
		Quaternion q0 = Quaternion.LookAt(direction0);
		Quaternion q1 = Quaternion.LookAt(direction1);

		closestPointsOnLines(origin0, tip0, origin1, tip1, out Vector3 closest0, out Vector3 closest1);

		Vector2 range0 = new Vector2(divideScalar(origin0 - closest0, direction0), divideScalar(tip0 - closest0, direction0));
		Vector2 range1 = new Vector2(divideScalar(origin1 - closest1, direction1), divideScalar(tip1 - closest1, direction1));

		Quaternion q = Quaternion.Slerp(q0, q1, t);
		Vector2 range = Vector2.Lerp(range0, range1, t);

		Vector3 intersection = Vector3.Lerp(closest0, closest1, t);
		origin = intersection + q.forward * range.x;
		tip = intersection + q.forward * range.y;
	}

	float divideScalar(Vector3 a, Vector3 b)
	{
		if (a.x != 0 && b.x != 0)
			return a.x / b.x;
		Debug.Assert(a.x == 0 && b.x == 0 || b.x != 0);
		if (a.y != 0 && b.y != 0)
			return a.y / b.y;
		Debug.Assert(a.y == 0 && b.y == 0 || b.y != 0);
		if (a.z != 0 && b.z != 0)
			return a.z / b.z;
		Debug.Assert(a.z == 0 && b.z == 0 || b.z != 0);
		return 0;
	}

	void closestPointsOnLines(Vector3 p1, Vector3 p2, Vector3 q1, Vector3 q2, out Vector3 result1, out Vector3 result2)
	{
		Vector3 u = p2 - p1;
		Vector3 v = q2 - q1;
		Vector3 w0 = p1 - q1;

		float a = Vector3.Dot(u, u); // u•u
		float b = Vector3.Dot(u, v); // u•v
		float c = Vector3.Dot(v, v); // v•v
		float d = Vector3.Dot(u, w0); // u•w0
		float e = Vector3.Dot(v, w0); // v•w0

		float denom = a * c - b * b;

		// Lines are nearly parallel
		if (MathF.Abs(denom) < 1e-6f)
		{
			// Arbitrarily choose s = 0
			float s = 0f;
			float t = (b > c ? d / b : e / c);

			result1 = p1 + s * u;
			result2 = q1 + t * v;
		}
		else
		{
			float s = (b * e - c * d) / denom;
			float t = (a * e - b * d) / denom;

			result1 = p1 + s * u;
			result2 = q1 + t * v;
		}
	}
}
