using Rainfall;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum DriveMode
{
	FrontWheel,
	RearWheel,
	AllWheel,
}

public class Cart : Entity
{
	const float wheelRadius = 0.138185f * 0.56f;
	static readonly Vector3[] tireLocalPositions = [
		new Vector3(0.37931f, 0.138185f, 0.598f) * 0.56f,
		new Vector3(-0.37931f, 0.138185f, 0.598f) * 0.56f,
		new Vector3(0.37931f, 0.138185f, -0.598f) * 0.56f,
		new Vector3(-0.37931f, 0.138185f, -0.598f) * 0.56f
	];

	DriveMode driveMode = DriveMode.FrontWheel;
	float frontWheelGrip = 0.5f;
	float rearWheelGrip = 0.7f;

	float maxSpeed = 40;
	float acceleration = 4;
	float rollingFriction = 0.3f;

	float[] tireHeights = new float[4];
	Vector3[] suspensionForces = new Vector3[4];
	Vector3[] steeringForces = new Vector3[4];
	Vector3[] forwardForces = new Vector3[4];
	Vector3[] tireVelocities = new Vector3[4];
	float[] wheelRotations = new float[4];
	bool[] wheelGrounded = new bool[4];

	float currentWheelSteer = 0;
	Vector3 currentSteerDirection;
	float currentMotor = 0;

	long turnedAroundSince = -1;

	public Vector3 lastVelocity;

	bool ejected = false;

	public float waterLevel = float.MaxValue;

	Model wheel;
	CartEngine engine;

	Sound sound, rocket;
	AudioSource audio;
	AudioSource rocketAudio;

	ParticleSystem[] wheelParticles = new ParticleSystem[4];


	public Cart()
	{
		bodyFriction = 0.0f;
		bodyRestitution = 1.0f;

		bodyFilterGroup = PhysicsFilter.Cart;
		bodyFilterMask = PhysicsFilter.Default;

		load("cart.rfs");

		sound = Resource.GetSound("sounds/cart.ogg");
		rocket = Resource.GetSound("sounds/rocket.ogg");

		wheel = Resource.GetModel("cart_wheel.gltf");
	}

	public override unsafe void init()
	{
		base.init();

		for (int i = 0; i < wheelParticles.Length; i++)
		{
			wheelParticles[i] = ParticleSystem.Create(getModelMatrix(), 100);
			wheelParticles[i].load("drift.rfs");
			wheelParticles[i].handle->spawnOffset = tireLocalPositions[i];
			wheelParticles[i].handle->emissionRate = 0;
			particles.Add(wheelParticles[i]);
		}

		audio = new AudioSource(position);
		audio.playSound(sound);
		audio.isLooping = true;

		rocketAudio = new AudioSource(position);
		rocketAudio.playSound(rocket);
		rocketAudio.isLooping = true;

		//body.setCenterOfMass(Vector3.Zero);
	}

	public override void destroy()
	{
		base.destroy();

		Resource.FreeSound(sound);
		Resource.FreeSound(rocket);
		Resource.FreeModel(wheel);

		unload();

		audio.destroy();
		rocketAudio.destroy();
	}

	unsafe void simulateTire(int i, bool frontWheel, float delta)
	{
		// Suspension

		//const float suspensionRestPosition = 0.138185f;
		const float springStrength = 100;
		const float springDamper = 15;

		Matrix tireTransform = getModelMatrix() * Matrix.CreateTranslation(tireLocalPositions[i]);
		Vector3 springDir = tireTransform.rotation.up;
		Vector3 tireVelocity = body.getPointVelocity(tireTransform.translation);

		tireVelocities[i] = tireVelocity;

		Vector3 localTireVelocity = rotation.conjugated * tireVelocities[i];
		wheelRotations[i] = MathHelper.Lerp(wheelRotations[i], MathF.Atan2(-localTireVelocity.x, -localTireVelocity.z), 1 * delta * tireVelocities[i].length);

		float raycastHeight = 1;
		float suspensionRestPosition = raycastHeight;
		HitData? hit = Physics.Raycast(getModelMatrix() * (tireLocalPositions[i] * new Vector3(1, 0, 1)) + raycastHeight * tireTransform.rotation.up, tireTransform.rotation.down, raycastHeight + wheelRadius, QueryFilterFlags.Static);

		if (hit == null && waterLevel != float.MaxValue)
		{
			Vector3 origin = getModelMatrix() * (tireLocalPositions[i] * new Vector3(1, 0, 1)) + raycastHeight * tireTransform.rotation.up;
			float range = raycastHeight + wheelRadius;
			Vector3 dir = tireTransform.rotation.down;
			Vector3 dest = origin + range * dir;
			if (dest.y <= waterLevel - raycastHeight)
			{
				hit = new HitData()
				{
					distance = (origin.y - (waterLevel - raycastHeight)) / dir.y,
					normal = Vector3.Up
				};
			}
		}

		if (hit == null || hit.Value.normal.y < 0.5f)
		{
			tireHeights[i] = -wheelRadius;
			wheelGrounded[i] = false;
			suspensionForces[i] = Vector3.Zero;
			steeringForces[i] = Vector3.Zero;
			forwardForces[i] = Vector3.Zero;

			return;
		}

		wheelGrounded[i] = true;

		// Autocorrect rotation mid air for each wheel thats grounded
		body.addForceAtPosition(hit.Value.normal * 2, position + rotation.up * 3);
		body.addForceAtPosition(-hit.Value.normal * 2, position + rotation.down * 3);

		float offset = suspensionRestPosition - hit.Value.distance;
		float velocity = Vector3.Dot(springDir, tireVelocity);

		// more ground sucction
		if (offset < 0)
			offset *= 8;

		float suspensionForce = (offset * springStrength) - (velocity * springDamper);
		body.addForceAtPosition(springDir * suspensionForce, tireTransform.translation);

		tireHeights[i] = offset;
		suspensionForces[i] = springDir * suspensionForce;

		// Steering

		Vector3 steeringDirection = frontWheel ? currentSteerDirection : Vector3.Forward;
		Vector3 steeringDirectionRight = Vector3.Cross(steeringDirection, Vector3.Up);
		steeringDirection = rotation * steeringDirection;
		steeringDirectionRight = rotation * steeringDirectionRight;

		float slideAmount = Vector3.Dot(tireVelocity, steeringDirectionRight);
		Vector3 slide = slideAmount * steeringDirectionRight;
		float grip = frontWheel ? frontWheelGrip : rearWheelGrip;
		//float gripMultiplier = 1 - MathF.Abs(Vector3.Dot(tireVelocity.normalized, steeringDirectionRight));
		//Console.WriteLine(gripMultiplier);

		wheelParticles[i].handle->emissionRate = MathF.Max(MathF.Abs(slideAmount) - 2, 0) * 20;
		wheelParticles[i].handle->startVelocity.x = slideAmount;

		//float gripLoseThreshold = 0.7f;
		//gripMultiplier = gripMultiplier > gripLoseThreshold ? MathHelper.Remap(gripMultiplier, gripLoseThreshold, 1.0f, 0.1f, 1.0f) : MathHelper.Remap(gripMultiplier, 0.0f, gripLoseThreshold, 0.05f, 0.1f);
		Vector3 steerAcceleration = -slide * grip; // * gripMultiplier;
		float tireMass = 2 / 60.0f;
		Vector3 steerForce = steerAcceleration * tireMass / delta;
		//body.addForceAtPosition(steerForce, tireTransform.translation);

		steeringForces[i] = steerForce;

		// Forward force

		bool drive = driveMode == DriveMode.AllWheel || (!frontWheel && driveMode == DriveMode.RearWheel) || (frontWheel && driveMode == DriveMode.FrontWheel);
		if (drive)
		{
			Vector3 motorForce = steeringDirection * currentMotor;
			//body.addForceAtPosition(motorForce, tireTransform.translation);
			steerForce += motorForce;
			forwardForces[i] = motorForce;
		}

		// Apply force & Drifting

		float frictionCoefficient = 10;
		float maxFrictionForce = frictionCoefficient * MathF.Abs(suspensionForce);
		if (steerForce.length > maxFrictionForce)
		{
			steerForce = steerForce.normalized * maxFrictionForce;
		}
		body.addForceAtPosition(steerForce, tireTransform.translation);

		// Rolling Friction

		if (currentMotor == 0)
		{
			body.addForceAtPosition(-tireVelocity * 0.25f * rollingFriction, tireTransform.translation);
		}
	}

	public override void fixedUpdate(float delta)
	{
		base.fixedUpdate(delta);

		currentSteerDirection = new Vector3(Vector2.Rotate(Vector2.Down, -currentWheelSteer), 0).xzy;

		lastVelocity = body.getVelocity();

		float forwardSpeed = Vector3.Dot(rotation * currentSteerDirection, body.getVelocity());
		float motorIncrease = 15;
		if (Input.IsKeyDown(KeyCode.W) && !ejected)
			currentMotor = MathHelper.Linear(currentMotor, acceleration * MathF.Max(1 - MathF.Max(forwardSpeed, 0) / maxSpeed, 0), motorIncrease * delta);
		if (Input.IsKeyDown(KeyCode.S) && !ejected)
			currentMotor = MathHelper.Linear(currentMotor, forwardSpeed > 0 ? -2 * acceleration : -3, motorIncrease * delta);
		if (!Input.IsKeyDown(KeyCode.W) && !Input.IsKeyDown(KeyCode.S) || ejected)
			currentMotor = MathHelper.Linear(currentMotor, 0, motorIncrease * delta);

		float steer = 2.0f;
		if (Input.IsKeyDown(KeyCode.D) && !ejected)
			currentWheelSteer -= delta * steer;
		if (Input.IsKeyDown(KeyCode.A) && !ejected)
			currentWheelSteer += delta * steer;
		if (!Input.IsKeyDown(KeyCode.D) && !Input.IsKeyDown(KeyCode.A) || ejected)
			currentWheelSteer = MathHelper.Linear(currentWheelSteer, 0, delta * steer);

		float maxSteer = 0.35f * MathF.PI * (1 / (1 + MathF.Abs(forwardSpeed) * 0.3f));
		currentWheelSteer = MathHelper.Clamp(currentWheelSteer, -maxSteer, maxSteer);

		if (GameState.instance.hasEngine)
		{
			float rocketForce = 40;
			if (Input.IsKeyDown(KeyCode.Shift) && !ejected)
				body.addForce(rotation.forward * rocketForce);
		}

		for (int i = 0; i < tireLocalPositions.Length; i++)
			simulateTire(i, i >= 2, delta);

		int numGrounded = 0;
		for (int i = 0; i < wheelGrounded.Length; i++)
			numGrounded += wheelGrounded[i] ? 1 : 0;
		if (numGrounded == 0)
			body.setAngularVelocity(body.getAngularVelocity() * 0.99f);


		if (rotation.up.y < 0.1f)
		{
			if (turnedAroundSince == -1)
				turnedAroundSince = Time.currentTime;

			if ((Time.currentTime - turnedAroundSince) / 1e9f > 3)
			{
				respawn();
			}
		}
		else
		{
			turnedAroundSince = -1;
		}
	}

	public void respawn()
	{
		setTransform(GameState.instance.spawnPoint);
		body.setVelocity(Vector3.Zero);
		body.setAngularVelocity(Vector3.Zero);
		ejected = false;
	}

	public override void onContact(RigidBody other, CharacterController otherController, int shapeID, int otherShapeID, bool isTrigger, bool otherTrigger, ContactType contactType)
	{
		if (!isTrigger && !otherTrigger)
		{
			if (!ejected)
			{
				Vector3 velocity = body.getVelocity();
				Vector2 sum = (lastVelocity - velocity).xz;
				float d = sum.length;
				Console.WriteLine(d);
				if (d > (GameState.instance.hasEngine ? 25 : 15))
				{
					// eject jerome
					GameState.instance.player.eject(lastVelocity);
					ejected = true;
				}
			}
		}
	}

	public override void update()
	{
		base.update();

		if (GameState.instance.hasEngine && engine == null)
		{
			GameState.instance.scene.addEntity(engine = new CartEngine(), position);
		}
		else if (!GameState.instance.hasEngine && engine != null)
		{
			engine.destroy();
			engine = null;
		}

		if (engine != null)
		{
			engine.active = Input.IsKeyDown(KeyCode.Shift);
		}

		int numGrounded = 0;
		for (int i = 0; i < wheelGrounded.Length; i++)
			numGrounded += wheelGrounded[i] ? 1 : 0;
		audio.gain = 2 * numGrounded * (1 - MathF.Exp(-body.getVelocity().xz.length * 0.1f));
		audio.pitch = 1 + (1 - MathF.Exp(-body.getVelocity().xz.length * 0.3f));
		audio.setPosition(position);

		rocketAudio.setPosition(position);
		Audio.FadeVolume(rocketAudio.source, Input.IsKeyDown(KeyCode.Shift) && GameState.instance.hasEngine ? 5 : 0, 0.2f);
	}

	public override void draw(GraphicsDevice graphics)
	{
		base.draw(graphics);

		for (int i = 0; i < 4; i++)
		{
			Vector3 wheelLocalPosition = tireLocalPositions[i] * new Vector3(1, 0, 1) + new Vector3(0, tireHeights[i] + wheelRadius, 0);
			Matrix wheelTransform = getModelMatrix() * Matrix.CreateTranslation(wheelLocalPosition);
			//if (i >= 2)
			//	wheelTransform = wheelTransform * Matrix.CreateRotation(Vector3.Up, currentWheelSteer);
			wheelTransform = wheelTransform * Matrix.CreateRotation(Vector3.Up, wheelRotations[i]);
			Renderer.DrawModel(wheel, wheelTransform);

#if DEBUG
			Renderer.DrawDebugLine(wheelTransform.translation, wheelTransform.translation + suspensionForces[i] * 0.1f, 0xFF0000FF);
			Renderer.DrawDebugLine(wheelTransform.translation, wheelTransform.translation + steeringForces[i] * 0.1f, 0xFFFF0000);
			Renderer.DrawDebugLine(wheelTransform.translation, wheelTransform.translation + forwardForces[i] * 0.1f, 0xFF00FF00);
#endif
		}
	}
}
